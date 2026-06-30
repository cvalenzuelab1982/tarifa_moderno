using Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Interfaces
{
    public interface IDescargarMaestroRepository
    {
        Task<List<GenericExtensionResponseDto>> RecuperaTipoServicioCobertura(int IdEmpresa, int I007_Dispositivo, CancellationToken cancellationToken);
        Task<List<GenericExtensionResponseDto>> RecuperaTipoPagoClienteCobertura(int IdEmpresa, int I007_Dispositivo, int idCliente, CancellationToken cancellationToken);

    }
}
