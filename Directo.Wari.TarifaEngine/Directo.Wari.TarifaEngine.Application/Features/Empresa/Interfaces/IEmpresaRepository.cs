namespace Directo.Wari.TarifaEngine.Application.Features.Empresa.Interfaces
{
    public interface IEmpresaRepository
    {
        Task<int> ConstanteZona(int idEmpresa, CancellationToken cancellationToken);
        Task<bool> ValidarDia(int idEmpresa, DateTime fechaServicio, CancellationToken cancellationToken);
        Task<bool> ValidaConductorCercano(int idEmpresa, CancellationToken cancellationToken);
    }
}
