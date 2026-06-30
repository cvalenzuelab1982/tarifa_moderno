using Directo.Wari.TarifaEngine.Application.Features.HttpApi.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.HttpApi.Interfaces
{
    public interface IHttpApiRepository
    {
        Task<(decimal Distancia, string OverView, decimal Time)> ObtenerMetrajeDePuntosWithOverview(double origenLatitud, double origenLongitud, double destinoLatitud, double destinoLongitud, CancellationToken cancellationToken);
        Task<(decimal Distancia, string OverView, decimal Time)> ObtenerMetrajeDePuntosWithOverviewWayPoint(double origenLatitud, double origenLongitud, double destinoLatitud, double destinoLongitud, List<SrvDestinoResponseDto> lstDestino, CancellationToken cancellationToken);

    }
}
