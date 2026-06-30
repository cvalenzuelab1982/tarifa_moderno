using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Services
{
    public interface IPromocionesService
    {
        Task<Generic> ValidarPromo(PromoValidacionRequestDto json, CancellationToken cancellationToken);
    }
}
