using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Interfaces
{
    public interface IPromocionesRepository
    {
        Task<List<BeanPromocionAppResponseDto>> ObtenerPromocionCliente(int idCliente, CancellationToken cancellationToken);
        Task<BeanPromocionAppResponseDto?> ObtenerPromocionClienteId(int idCliente, int idPromocion, decimal totalServicio, CancellationToken cancellationToken);
        Task<ValidatePromocionResponseDto?> ValidatePromocion(ValidatePromocionRequestDto request, CancellationToken cancellationToken);
        Task<(List<int> ZonaOrigen, List<int> ZonaDestino)> ObtenerZonasPromocion(int idPromocion, CancellationToken cancellationToken);
    }
}
