using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Interfaces
{
    public interface IRecargoEspecialRepository
    {
        /// <summary>
        /// Formato de fecha HH:mm:ss
        /// </summary>
        /// <param name="IdEmpresa"></param>
        /// <param name="IdTipoPago"></param>
        /// <param name="IdTipoServicio"></param>
        /// <param name="FechaServicio"></param>
        /// <param name="OrigenDestino"></param>
        /// <returns></returns>
        Task<BeanRecargoEspecialResponseDto?> GetRecargoEspecialAeropuerto(int IdEmpresa, int IdTipoPago, int IdTipoServicio, DateTime FechaServicio, int OrigenDestino, CancellationToken cancellationToken);


        /// <summary>
        /// Formato de fecha HH:mm:ss
        /// </summary>
        /// <param name="IdEmpresa"></param>
        /// <param name="IdTipoPago"></param>
        /// <param name="IdTipoServicio"></param>
        /// <param name="FechaServicio"></param>
        /// <returns></returns>
        Task<BeanRecargoEspecialResponseDto?> GetRecargoEspecialTipoServicioTipoPago(int IdEmpresa, int IdTipoPago, int IdTipoServicio, DateTime FechaServicio, CancellationToken cancellationToken);
    }
}
