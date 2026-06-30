using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.Peaje.Interfaces
{
    public interface IPeajeRepository
    {
        Task<List<PeajeSistemaRespondeDto>> ListarPeajesPorZonas(List<int> listaIdsZonas, CancellationToken cancellationToken);
    }
}
