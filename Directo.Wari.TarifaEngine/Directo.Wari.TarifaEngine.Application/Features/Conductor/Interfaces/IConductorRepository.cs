namespace Directo.Wari.TarifaEngine.Application.Features.Conductor.Interfaces
{
    public interface IConductorRepository
    {
        Task<bool> ExisteConductoresCercanos(double Longitud, double Latitud, int IdEmpresa, int IdCliente, CancellationToken cancellationToken);
    }
}
