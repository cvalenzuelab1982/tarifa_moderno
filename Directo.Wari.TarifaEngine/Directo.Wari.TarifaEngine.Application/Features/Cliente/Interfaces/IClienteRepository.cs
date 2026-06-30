namespace Directo.Wari.TarifaEngine.Application.Features.Cliente.Interfaces
{
    public interface IClienteRepository
    {
        /// <summary>
        /// Equivalente a MCliente.ServiciosPorCalificar del sistema antiguo.
        /// Consulta si el cliente tiene servicios pendientes por calificar.
        /// </summary>
        Task<bool> ServiciosPorCalificar(int idCliente,CancellationToken cancellationToken);
        Task<Dictionary<string, string>?> ObtenerePresupuestoCliente(int idCliente, CancellationToken cancellationToken);

    }
}
