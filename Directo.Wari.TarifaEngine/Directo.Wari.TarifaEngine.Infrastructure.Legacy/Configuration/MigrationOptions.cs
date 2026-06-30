namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Configuration
{
    /// <summary>
    /// Opciones de configuración para la migración dual-database.
    /// Controla las fases de migración entre SQL Server (legacy) y PostgreSQL (nuevo).
    /// </summary>
    public class MigrationOptions
    {
        public const string SectionName = "Migration";

        /// <summary>
        /// Fuente de lectura de datos.
        /// "SqlServer" = leer del sistema legacy.
        /// "PostgreSql" = leer de la nueva base de datos.
        /// </summary>
        public string ReadSource { get; set; } = "SqlServer";

        /// <summary>
        /// Indica si se debe sincronizar los cambios hacia la base de datos legacy (SQL Server).
        /// true = dual-write activo (escribe en PostgreSQL + sincroniza a SQL Server).
        /// false = solo escribe en PostgreSQL.
        /// </summary>
        public bool SyncToLegacy { get; set; } = true;

        /// <summary>
        /// Cantidad máxima de reintentos al sincronizar un mensaje del outbox.
        /// </summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>
        /// Intervalo en segundos entre cada ejecución del procesador de outbox.
        /// </summary>
        public int OutboxProcessorIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Cantidad máxima de mensajes a procesar por lote.
        /// </summary>
        public int OutboxBatchSize { get; set; } = 50;

        public bool ReadFromLegacy => ReadSource.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);
        public bool ReadFromPostgreSql => ReadSource.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase);
    }
}
