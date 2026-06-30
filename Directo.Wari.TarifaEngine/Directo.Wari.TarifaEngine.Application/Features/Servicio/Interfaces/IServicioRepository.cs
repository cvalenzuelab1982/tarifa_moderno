using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Servicio.Interfaces
{
    public interface IServicioRepository
    {
        Task<bool> TodosPrimerDestinoEmpresa(int idEmpresa, CancellationToken cancellationToken);
        Task<string> ObteneTipoFormaCalculoEmpresa(int idTipoServicio, int dispositivo, int idEmpresa, CancellationToken cancellationToken);
        Task<int> ObtenerTiempoPorZona(int idZona, DateTime horaActual, int idEmpresa, CancellationToken cancellationToken);
        Task<int> TiempoReservaAlo(int idZona, DateTime horaActual, int idEmpresa);
    }
}
