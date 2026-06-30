namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Entidad que representa un mensaje pendiente de sincronización al sistema legacy (Outbox Pattern).
    /// Se persiste en PostgreSQL y se procesa asincrónicamente para replicar cambios a SQL Server.
    /// </summary>
    public class OutboxMessage
    {
        public Guid Id { get; private set; }

        /// <summary>
        /// Tipo de entidad afectada (ej: "Servicio", "Cliente", "Conductor").
        /// </summary>
        public string EntityType { get; private set; } = null!;

        /// <summary>
        /// ID de la entidad afectada.
        /// </summary>
        public int EntityId { get; private set; }

        /// <summary>
        /// Tipo de operación: "Insert", "Update", "Delete".
        /// </summary>
        public string OperationType { get; private set; } = null!;

        /// <summary>
        /// Snapshot serializado de la entidad en formato JSON.
        /// </summary>
        public string Payload { get; private set; } = null!;

        /// <summary>
        /// Fecha de creación del mensaje.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Fecha en que el mensaje fue procesado exitosamente.
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Último error ocurrido al intentar procesar el mensaje.
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Cantidad de reintentos realizados.
        /// </summary>
        public int RetryCount { get; private set; }

        private OutboxMessage() { } // EF Core

        /// <summary>
        /// Crea un nuevo mensaje de outbox.
        /// </summary>
        public static OutboxMessage Create(string entityType, int entityId, string operationType, string payload)
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EntityType = entityType,
                EntityId = entityId,
                OperationType = operationType,
                Payload = payload,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };
        }

        /// <summary>
        /// Marca el mensaje como procesado exitosamente.
        /// </summary>
        public void MarkAsProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
            Error = null;
        }

        /// <summary>
        /// Registra un error de procesamiento e incrementa el contador de reintentos.
        /// </summary>
        public void MarkAsFailed(string error)
        {
            Error = error;
            RetryCount++;
        }
    }
}
