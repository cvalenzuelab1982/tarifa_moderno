using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Domain.Enums;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces
{
    public interface ITarifaRepository
    {
        Task<BeanCoberturaResponseDto?> RecuperarCobertura(double latitud, double longitud, CancellationToken cancellationToken);
        Task<BeanCoberturaResponseDto?> RecuperarCoberturaOrigen(double latitud, double longitud, CancellationToken cancellationToken);
        Task<bool> ValidarZona(int idZonaOrigen, int idZonaDestino, CancellationToken cancellationToken);
        Task<(bool Existe, int IdZona)> VerificarPuntoEnZonaIdZona(double latitud, double longitud, CancellationToken cancellationToken);
        Task<decimal> ObtenerTarifaCustom(int idZonaOrigen, int idZonaDestino, CancellationToken cancellationToken);
        Task<Dictionary<string,string>> ObtenerTarifaFormulaEmpresa(int idEmpresa, int tipoServicio, CancellationToken cancellationToken);
        Task<Dictionary<string, string>> ObtenerTarifaFormulaEmpresaCobertura(int idEmpresa, int tipoServicio, string cobertura, CancellationToken cancellationToken);
        Task<bool> IsZonaAeropuerto(double latitud, double longitud, CancellationToken cancellationToken);
        Task<decimal> ObtenerConstanteZonaV2(double latitud, double longitud, int idEmpresa, CancellationToken cancellationToken);
        Task<bool> VerificarPuntoEnZonaTipoZona(double latitud, double longitud, TipoZona tipoZona, CancellationToken cancellationToken);
        Task<decimal> ObtenerConstanteZona(double latitud, double longitud, CancellationToken cancellationToken);
        Task<bool> IsZonaDirecto(double latitud, double longitud, int idZona, CancellationToken cancellationToken);
        Task<decimal> ObtenerDescuentoSedeServicio(DetBeanServicioRequestSPDto inputBeanServ, oldBeanTarifaRequestDto beanTarifa, CancellationToken cancellationToken);
        Task<Dictionary<string, decimal>?> ObtenerMontoIncrementoEmpresa(int idEmpresa, CancellationToken cancellationToken);
        Task<RecuperaByIdFormaCalculoResponseDto> RecuperaByIdFormaCalculo(int Empresa,int I011_TipoServicio,int I008_TipoPago,int Origen,int Destino,int idCliente,int cantPasajeros,CancellationToken cancellationToken);
        Task<RecuperaByIdFormaCalculoResponseDto> RecuperaByIdFormaCalculoLatam(int Empresa, int idCliente, int I011_TipoServicio, int I008_TipoPago, int Origen, int Destino, int cantPasajeros, CancellationToken cancellationToken);
        Task<RecuperaByIdResponseDto?> RecuperaById(int Empresa, int Cliente, int I011_TipoServicio, int I008_TipoPago, int Origen, int Destino, CancellationToken cancellationToken);
    }
}
