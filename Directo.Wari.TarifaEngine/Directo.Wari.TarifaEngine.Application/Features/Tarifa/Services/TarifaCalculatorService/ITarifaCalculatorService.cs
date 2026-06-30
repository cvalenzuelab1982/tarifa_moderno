using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Services.TarifaCalculatorService
{
    public interface ITarifaCalculatorService
    {
        Task<Result<TarifaDetalleResponseDto>> Calcular(TarifaDestinoRequestDto inputBeanServ, CancellationToken cancellationToken);
    }
}
