using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Services
{
    public interface IRecargoEspecialService
    {
        Task<TarifaDetalleResponseDto> CalcularIncremento(TarifaDetalleResponseDto tarifa, BeanRecargoEspecialResponseDto beanRecargoEspecial, decimal totalTarifaBase, bool incrementoMax, CancellationToken cancellationToken);
    }
}
