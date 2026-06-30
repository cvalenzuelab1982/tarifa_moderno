using Directo.Wari.TarifaEngine.Domain.Common;
using Directo.Wari.TarifaEngine.Infrastructure.Legacy.Persistence;
using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Sync
{
    /// <summary>
    /// Servicio de sincronización que replica cambios desde PostgreSQL hacia SQL Server (legacy).
    /// 
    /// Utiliza el LegacyDbContext (EF Core con SQL Server) para escribir las mismas entidades
    /// del dominio en el schema legacy. Para tablas con diferencias significativas, se puede
    /// usar stored procedures via ExecuteSqlRawAsync.
    /// </summary>
    public class LegacySyncService : ILegacySyncService
    {
        private readonly LegacyDbContext _legacyContext;
        private readonly ILogger<LegacySyncService> _logger;

        /// <summary>
        /// Mapa de tipos de entidad soportados para la sincronización.
        /// Agregar nuevas entidades aquí conforme se migren progresivamente.
        /// </summary>
        private static readonly Dictionary<string, Type> SupportedEntityTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            //["Servicio"] = typeof(Servicio),
            //["Destino"] = typeof(Destino),
            //["Cliente"] = typeof(Cliente),
            //["Conductor"] = typeof(Conductor)
        };

        public LegacySyncService(LegacyDbContext legacyContext, ILogger<LegacySyncService> logger)
        {
            _legacyContext = legacyContext;
            _logger = logger;
        }

        public async Task<bool> SyncToLegacyAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            if (!SupportedEntityTypes.TryGetValue(message.EntityType, out var entityType))
            {
                _logger.LogWarning(
                    "Tipo de entidad '{EntityType}' no soportado para sincronización legacy. Mensaje {MessageId} será marcado como procesado.",
                    message.EntityType, message.Id);
                return true; // Marcar como procesado para no reintentar
            }

            try
            {
                return message.OperationType switch
                {
                    "Insert" => await HandleInsertAsync(entityType, message, cancellationToken),
                    "Update" => await HandleUpdateAsync(entityType, message, cancellationToken),
                    "Delete" => await HandleDeleteAsync(entityType, message, cancellationToken),
                    _ => throw new InvalidOperationException($"OperationType '{message.OperationType}' no reconocido.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al sincronizar {OperationType} {EntityType} Id={EntityId} a SQL Server. " +
                    "Mensaje {MessageId}, Intento #{RetryCount}",
                    message.OperationType, message.EntityType, message.EntityId,
                    message.Id, message.RetryCount + 1);
                return false;
            }
        }

        private async Task<bool> HandleInsertAsync(Type entityType, OutboxMessage message, CancellationToken cancellationToken)
        {
            var entity = DeserializeEntity(entityType, message.Payload);
            if (entity is null) return false;

            // Verificar si ya existe (idempotencia — el mensaje pudo haber sido reintentado)
            var existing = await _legacyContext.FindAsync(entityType, [entity.Id], cancellationToken);
            if (existing is not null)
            {
                _logger.LogInformation(
                    "Entidad {EntityType} Id={EntityId} ya existe en SQL Server. Se actualizará en lugar de insertar.",
                    message.EntityType, message.EntityId);

                // Detach la existente y actualizar
                _legacyContext.Entry(existing).State = EntityState.Detached;
                _legacyContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                // Habilitar IDENTITY_INSERT para poder insertar con el ID específico de PostgreSQL
                var tableName = GetTableName(entityType);
                await _legacyContext.Database.ExecuteSqlRawAsync(
                    string.Format("SET IDENTITY_INSERT [{0}] ON", tableName), cancellationToken);

                _legacyContext.Add(entity);
            }

            await _legacyContext.SaveChangesAsync(cancellationToken);

            // Desactivar IDENTITY_INSERT si se activó
            if (_legacyContext.Entry(entity).State != EntityState.Modified)
            {
                var tableName = GetTableName(entityType);
                await _legacyContext.Database.ExecuteSqlRawAsync(
                    string.Format("SET IDENTITY_INSERT [{0}] OFF", tableName), cancellationToken);
            }

            _legacyContext.ChangeTracker.Clear();

            _logger.LogInformation(
                "Sincronizado INSERT {EntityType} Id={EntityId} a SQL Server",
                message.EntityType, message.EntityId);

            return true;
        }

        private async Task<bool> HandleUpdateAsync(Type entityType, OutboxMessage message, CancellationToken cancellationToken)
        {
            var entity = DeserializeEntity(entityType, message.Payload);
            if (entity is null) return false;

            // Verificar que existe en SQL Server
            var existing = await _legacyContext.FindAsync(entityType, [entity.Id], cancellationToken);
            if (existing is null)
            {
                _logger.LogWarning(
                    "Entidad {EntityType} Id={EntityId} no encontrada en SQL Server para UPDATE. Se intentará INSERT.",
                    message.EntityType, message.EntityId);

                _legacyContext.Add(entity);
            }
            else
            {
                _legacyContext.Entry(existing).State = EntityState.Detached;
                _legacyContext.Entry(entity).State = EntityState.Modified;
            }

            await _legacyContext.SaveChangesAsync(cancellationToken);
            _legacyContext.ChangeTracker.Clear();

            _logger.LogInformation(
                "Sincronizado UPDATE {EntityType} Id={EntityId} a SQL Server",
                message.EntityType, message.EntityId);

            return true;
        }

        private async Task<bool> HandleDeleteAsync(Type entityType, OutboxMessage message, CancellationToken cancellationToken)
        {
            var existing = await _legacyContext.FindAsync(entityType, [message.EntityId], cancellationToken);
            if (existing is null)
            {
                _logger.LogInformation(
                    "Entidad {EntityType} Id={EntityId} no encontrada en SQL Server para DELETE (ya eliminada o no sincronizada).",
                    message.EntityType, message.EntityId);
                return true; // Idempotente: ya no existe
            }

            _legacyContext.Remove(existing);
            await _legacyContext.SaveChangesAsync(cancellationToken);
            _legacyContext.ChangeTracker.Clear();

            _logger.LogInformation(
                "Sincronizado DELETE {EntityType} Id={EntityId} a SQL Server",
                message.EntityType, message.EntityId);

            return true;
        }

        /// <summary>
        /// Deserializa una entidad desde el payload JSON del outbox message.
        /// </summary>
        private BaseEntity? DeserializeEntity(Type entityType, string payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var entity = JsonSerializer.Deserialize(payload, entityType, options) as BaseEntity;
                if (entity is null)
                {
                    _logger.LogError("Error al deserializar {EntityType}: resultado null. Payload: {Payload}",
                        entityType.Name, payload);
                }

                return entity;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar payload JSON para {EntityType}. Payload: {Payload}",
                    entityType.Name, payload);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el nombre de la tabla SQL Server para un tipo de entidad.
        /// </summary>
        private string GetTableName(Type entityType)
        {
            var entityTypeModel = _legacyContext.Model.FindEntityType(entityType);
            return entityTypeModel?.GetTableName() ?? entityType.Name + "s";
        }
    }
}
