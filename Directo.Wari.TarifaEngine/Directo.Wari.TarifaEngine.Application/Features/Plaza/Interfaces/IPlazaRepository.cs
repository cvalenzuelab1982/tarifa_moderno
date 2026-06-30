namespace Directo.Wari.TarifaEngine.Application.Features.Plaza.Interfaces
{
    public interface IPlazaRepository
    {
        Task<decimal> RecalculoTarifaPlaza(int idZonaOrigen, int idZonaDestino, DateTime fechaServicio, CancellationToken cancellationToken);
        Task<bool> ValidarZonaPlaza(double latitud, double longitud, int idZona, CancellationToken cancellationToken = default);
    }
}
