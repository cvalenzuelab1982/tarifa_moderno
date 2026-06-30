using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Sync
{
    /// <summary>
    /// Interfaz interna para el servicio de sincronización con la base de datos legacy SQL Server.
    /// No se expone a las capas superiores (Application/Domain) — solo la usa Infrastructure.Legacy.
    /// </summary>
    public interface ILegacySyncService
    {
        /// <summary>
        /// Procesa un mensaje de outbox y sincroniza el cambio a SQL Server.
        /// </summary>
        /// <param name="message">Mensaje de outbox a procesar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>True si se procesó exitosamente, false si falló.</returns>
        Task<bool> SyncToLegacyAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
