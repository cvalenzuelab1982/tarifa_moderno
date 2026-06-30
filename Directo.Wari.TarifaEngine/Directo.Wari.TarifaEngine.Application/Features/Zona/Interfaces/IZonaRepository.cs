using Directo.Wari.TarifaEngine.Application.Features.Zona.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces
{
    public interface IZonaRepository
    {
        Task<int> ObtenerIdZona(double latitud, double longitud, CancellationToken cancellationToken);
        Task<int> RecuperaByPosicion(int IdEmpresa, double Latitud, double Longitud, CancellationToken cancellationToken);
        Task<bool> IsZonaPeligrosa(double latitud, double longitud, CancellationToken cancellationToken);
        Task<ZonaResponseDto> RecuperaById(int Zona, CancellationToken cancellationToken);
    }
}
