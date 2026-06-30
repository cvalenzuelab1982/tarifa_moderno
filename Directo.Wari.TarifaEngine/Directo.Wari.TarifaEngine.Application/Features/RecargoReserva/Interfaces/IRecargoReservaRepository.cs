using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Interfaces
{
    public interface IRecargoReservaRepository
    {
        Task<BeanRecargoReservaResponseDto?> GetRecargoReservaOnValue(int idTipoPago, TarifaDestinoRequestDto beanServicio, TarifaDetalleResponseDto beanTarifa, DateTime FechaServicio, decimal totalTarifaBase, CancellationToken cancellationToken);
    }
}
