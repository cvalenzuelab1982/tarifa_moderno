namespace Directo.Wari.TarifaEngine.Application.Features.Compania.Interfaces
{
    public interface ICompaniaRepository
    {
        Task<int> RecuperarCompania(int idEmpresa, double latitudOrigen, double longitudOrigen, double latitudDestino, double longitudDestino, CancellationToken cancellationToken);
    }
}
