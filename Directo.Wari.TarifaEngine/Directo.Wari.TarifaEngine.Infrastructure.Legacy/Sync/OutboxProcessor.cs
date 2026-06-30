using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Legacy.Configuration;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Sync
{
    /// <summary>
    /// Background service que procesa mensajes del outbox y los sincroniza a SQL Server.
    /// 
    /// Utiliza un patrón de polling periódico para leer mensajes pendientes de la tabla
    /// outbox_messages en PostgreSQL y procesarlos secuencialmente.
    /// 
    /// Características:
    /// - Procesamiento por lotes configurables (OutboxBatchSize).
    /// - Reintentos con backoff exponencial (hasta MaxRetryCount).
    /// - Mensajes que exceden los reintentos se marcan con error para revisión manual.
    /// - Se detiene automáticamente si SyncToLegacy se desactiva.
    /// </summary>
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly MigrationOptions _options;

        public OutboxProcessor(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessor> logger,
            IOptions<MigrationOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.SyncToLegacy)
            {
                _logger.LogInformation("OutboxProcessor: Sincronización legacy desactivada (SyncToLegacy=false). Servicio detenido.");
                return;
            }

            _logger.LogInformation(
                "OutboxProcessor: Iniciado. Intervalo={Interval}s, BatchSize={BatchSize}, MaxRetries={MaxRetries}",
                _options.OutboxProcessorIntervalSeconds, _options.OutboxBatchSize, _options.MaxRetryCount);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "OutboxProcessor: Error en el ciclo de procesamiento");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_options.OutboxProcessorIntervalSeconds),
                    stoppingToken);
            }

            _logger.LogInformation("OutboxProcessor: Detenido.");
        }

        /// <summary>
        /// Procesa un lote de mensajes pendientes del outbox.
        /// </summary>
        private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            // Obtener el DbContext de PostgreSQL para leer mensajes pendientes
            var appDbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var dbContext = (DbContext)appDbContext;
            var syncService = scope.ServiceProvider.GetRequiredService<ILegacySyncService>();

            var pendingMessages = await dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null && m.RetryCount < _options.MaxRetryCount)
                .OrderBy(m => m.CreatedAt)
                .Take(_options.OutboxBatchSize)
                .ToListAsync(cancellationToken);

            if (pendingMessages.Count == 0) return;

            _logger.LogDebug("OutboxProcessor: Procesando {Count} mensaje(s) pendiente(s)", pendingMessages.Count);

            var successCount = 0;
            var failCount = 0;

            foreach (var message in pendingMessages)
            {
                try
                {
                    var success = await syncService.SyncToLegacyAsync(message, cancellationToken);

                    if (success)
                    {
                        message.MarkAsProcessed();
                        successCount++;
                    }
                    else
                    {
                        message.MarkAsFailed("Sincronización retornó false");
                        failCount++;
                    }
                }
                catch (Exception ex)
                {
                    message.MarkAsFailed(ex.Message);
                    failCount++;

                    if (message.RetryCount >= _options.MaxRetryCount)
                    {
                        _logger.LogError(
                            "OutboxProcessor: Mensaje {MessageId} ({EntityType} Id={EntityId}) ha excedido " +
                            "el máximo de reintentos ({MaxRetries}). Requiere revisión manual.",
                            message.Id, message.EntityType, message.EntityId, _options.MaxRetryCount);
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (successCount > 0 || failCount > 0)
            {
                _logger.LogInformation(
                    "OutboxProcessor: Lote completado. Éxitos={Success}, Fallos={Failures}",
                    successCount, failCount);
            }
        }
    }
}
