using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Common;
using Directo.Wari.TarifaEngine.Domain.Interfaces;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Sync
{
    /// <summary>
    /// Decorator de IUnitOfWork que implementa dual-write (Outbox Pattern).
    /// 
    /// Flujo:
    /// 1. Se ejecuta SaveChangesAsync en PostgreSQL (operación primaria).
    /// 2. Se detectan las entidades modificadas del ChangeTracker.
    /// 3. Se crean mensajes OutboxMessage en PostgreSQL.
    /// 4. Un job en background (OutboxProcessor) procesa los mensajes pendientes
    ///    y los replica a SQL Server.
    /// 
    /// Esto garantiza que:
    /// - Si PostgreSQL falla → la operación completa falla (correcto).
    /// - Si SQL Server falla → el cambio queda en PostgreSQL y se reintenta (consistencia eventual).
    /// </summary>
    public class DualWriteUnitOfWork : IUnitOfWork
    {
        private readonly IUnitOfWork _innerUnitOfWork;
        private readonly IApplicationDbContext _dbContext;
        private readonly DbContext _efContext;
        private readonly ILogger<DualWriteUnitOfWork> _logger;

        public DualWriteUnitOfWork(
            IUnitOfWork innerUnitOfWork,
            IApplicationDbContext dbContext,
            ILogger<DualWriteUnitOfWork> logger)
        {
            _innerUnitOfWork = innerUnitOfWork;
            _dbContext = dbContext;
            _efContext = (DbContext)dbContext;
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Capturar referencias a las entidades modificadas ANTES de guardar
            //    (el ChangeTracker pierde el estado después de SaveChanges)
            var pendingChanges = CaptureChangedEntities();

            // 2. Guardar en PostgreSQL (operación primaria)
            var result = await _innerUnitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Crear mensajes de outbox DESPUÉS del save (los IDs ya están generados)
            if (pendingChanges.Count > 0)
            {
                await CreateOutboxMessages(pendingChanges, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Inspecciona el ChangeTracker de EF Core para capturar entidades
        /// que serán insertadas, modificadas o eliminadas.
        /// Guarda referencias a las entidades (no snapshots) para que después del save
        /// se puedan leer los IDs generados por la base de datos.
        /// </summary>
        private List<PendingChange> CaptureChangedEntities()
        {
            var changes = new List<PendingChange>();

            foreach (var entry in _efContext.ChangeTracker.Entries<BaseEntity>())
            {
                var operationType = entry.State switch
                {
                    EntityState.Added => "Insert",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => null
                };

                if (operationType is null) continue;

                var entityType = entry.Entity.GetType();

                // Solo capturar entidades del dominio (excluir OutboxMessage, etc.)
                if (entityType.Namespace?.StartsWith("WariDirecto.Domain") != true)
                    continue;

                changes.Add(new PendingChange(
                    Entity: entry.Entity,
                    EntityTypeName: entityType.Name,
                    OperationType: operationType
                ));
            }

            return changes;
        }

        /// <summary>
        /// Crea mensajes OutboxMessage en PostgreSQL para los cambios capturados.
        /// Se ejecuta DESPUÉS del SaveChanges principal, cuando los IDs ya están generados.
        /// </summary>
        private async Task CreateOutboxMessages(List<PendingChange> changes, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var change in changes)
                {
                    // Serializar DESPUÉS del save — el Id ya está asignado por PostgreSQL
                    var payload = SerializeEntity(change.Entity);

                    var outboxMessage = OutboxMessage.Create(
                        change.EntityTypeName,
                        change.Entity.Id,
                        change.OperationType,
                        payload);

                    _efContext.Set<OutboxMessage>().Add(outboxMessage);

                    _logger.LogDebug(
                        "Outbox: {Operation} {EntityType} Id={EntityId}",
                        change.OperationType, change.EntityTypeName, change.Entity.Id);
                }

                await _efContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Outbox: {Count} mensaje(s) creado(s) para sincronización legacy",
                    changes.Count);
            }
            catch (Exception ex)
            {
                // CRÍTICO: Si falla la creación del outbox, loguear pero NO fallar la operación principal.
                // El cambio ya se guardó en PostgreSQL exitosamente.
                // Se puede implementar un mecanismo de reconciliación manual más adelante.
                _logger.LogError(ex,
                    "Error al crear mensajes de outbox. Los cambios se guardaron en PostgreSQL pero " +
                    "podrían no sincronizarse automáticamente a SQL Server. Cambios: {Changes}",
                    string.Join(", ", changes.Select(c => $"{c.OperationType} {c.EntityTypeName}#{c.Entity.Id}")));
            }
        }

        /// <summary>
        /// Serializa una entidad a JSON para almacenar en el outbox.
        /// </summary>
        private static string SerializeEntity(BaseEntity entity)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(entity, entity.GetType(), options);
        }

        public void Dispose()
        {
            _innerUnitOfWork.Dispose();
            GC.SuppressFinalize(this);
        }

        private record PendingChange(BaseEntity Entity, string EntityTypeName, string OperationType);
    }
}
