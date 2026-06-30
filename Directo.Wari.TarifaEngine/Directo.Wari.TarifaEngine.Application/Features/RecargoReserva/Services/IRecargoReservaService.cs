using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Services
{
    public interface IRecargoReservaService
    {
        Task<TarifaDetalleResponseDto> CalcularIncremento(TarifaDetalleResponseDto tarifa, BeanRecargoReservaResponseDto HoraPunta, decimal totalTarifaBase, CancellationToken cancellationToken);
    }
}
