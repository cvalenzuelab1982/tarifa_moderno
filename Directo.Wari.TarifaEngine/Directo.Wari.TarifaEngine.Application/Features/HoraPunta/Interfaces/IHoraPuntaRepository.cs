using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Dto;

namespace Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Interfaces
{
    public interface IHoraPuntaRepository
    {
        /// <summary>
        /// Formato de fecha HH:mm:ss
        /// </summary>
        /// <param name="idEmpresa"></param>
        /// <param name="tipopago"></param>
        /// <param name="idTipoServicio"></param>
        /// <param name="FechaServicio"></param>
        /// <returns></returns>
        Task<BeanHoraPuntaResponseDto?> GetHoraPuntaOnValue(int idEmpresa, int tipopago, int idTipoServicio, DateTime FechaServicio, CancellationToken cancellationToken);
    }
}
