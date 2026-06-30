namespace Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces
{
    public interface IParametrosRepository
    {
        /// <summary>
        /// Equivalente a ConstantesGlobales.GetParameterValue o getParameterStringNull del sistema antiguo.
        /// </summary>
        Task<string> GetParameterValue(string nombreParametro,CancellationToken cancellationToken);
    }
}
