using Directo.Wari.TarifaEngine.Application.Common.Constants;
using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Common.Util;
using Directo.Wari.TarifaEngine.Application.Features.Cliente.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Compania.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Dto;
using Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.HttpApi.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Peaje.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Plaza.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Services;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Services;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Services;
using Directo.Wari.TarifaEngine.Application.Features.Servicio.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Aggregates;
using Directo.Wari.TarifaEngine.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using static Directo.Wari.TarifaEngine.Application.Common.Constants.TarifaConstants;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Services.TarifaCalculatorService
{
    public class TarifaCalculatorService : ITarifaCalculatorService
    {
        private readonly ILogger<TarifaCalculatorService> _logger;
        private readonly IZonaRepository _zonaRepository;
        private readonly ITarifaRepository _tarifaRepository;
        private readonly IDescargarMaestroRepository _descargarMaestroRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IParametrosRepository _parametrosRepository;
        private readonly IHttpApiRepository _httpApiRepository;
        private readonly IRecargoEspecialRepository _recargoEspecialRepository;
        private readonly IHoraPuntaRepository _horaPuntaRepository;
        private readonly IRecargoReservaRepository _recargoReservaRepository;
        private readonly IPromocionesRepository _promocionesRepository;
        private readonly IPlazaRepository _plazaRepository;
        private readonly ICompaniaRepository _companiaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IPeajeRepository _peajeRepository;

        private readonly IRecargoEspecialService _recargoEspecialService;
        private readonly IRecargoReservaService _recargoReservaService;
        private readonly IPromocionesService _promocionesService;

        private readonly IRepository<ConfiguracionZona> _configuracionZonaRepository;

        private readonly ConfiguracionGenericasOptions _config;

        public TarifaCalculatorService(IOptions<ConfiguracionGenericasOptions> options, ILogger<TarifaCalculatorService> logger, IZonaRepository zonaRepository, ITarifaRepository tarifaRepository, IDescargarMaestroRepository descargarMaestroRepository, IServicioRepository servicioRepository, IParametrosRepository parametrosRepository, IHttpApiRepository httpApiRepository, IRecargoEspecialRepository recargoEspecialRepository, IHoraPuntaRepository horaPuntaRepository, IRecargoEspecialService recargoEspecialService, IRecargoReservaRepository recargoReservaRepository, IRecargoReservaService recargoReservaService, IPromocionesService promocionesService, IPromocionesRepository promocionesRepository, IPlazaRepository plazaRepository, ICompaniaRepository companiaRepository, IClienteRepository clienteRepository, IPeajeRepository peajeRepository, IRepository<ConfiguracionZona> configuracionZonaRepository)
        {
            _zonaRepository = zonaRepository;
            _tarifaRepository = tarifaRepository;
            _logger = logger;
            _descargarMaestroRepository = descargarMaestroRepository;
            _servicioRepository = servicioRepository;
            _parametrosRepository = parametrosRepository;
            _httpApiRepository = httpApiRepository;
            _config = options.Value;
            _recargoEspecialRepository = recargoEspecialRepository;
            _horaPuntaRepository = horaPuntaRepository;
            _recargoEspecialService = recargoEspecialService;
            _recargoReservaRepository = recargoReservaRepository;
            _recargoReservaService = recargoReservaService;
            _promocionesService = promocionesService;
            _promocionesRepository = promocionesRepository;
            _plazaRepository = plazaRepository;
            _companiaRepository = companiaRepository;
            _clienteRepository = clienteRepository;
            _peajeRepository = peajeRepository;
            _configuracionZonaRepository = configuracionZonaRepository;
        }

        /// <summary>
        /// IdEmpresas documento
        /// BBVA                = 13510
        /// MiBacno             = 1104
        /// Empresa1            = 13404
        /// Empresa2            = 13466
        /// LATAM               = 13235
        /// Empresa3            = 166
        /// Empresa4            = 175
        /// Parque del Recuerdo = Parque
        /// </summary>
        /// <param name="inputBeanServ"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<TarifaDetalleResponseDto>> Calcular(TarifaDestinoRequestDto inputBeanServ, CancellationToken cancellationToken)
        {
            var configZonas = await _configuracionZonaRepository.ListAllAsync(cancellationToken);
            var beanTarifa = new TarifaDetalleResponseDto();
            BeanPromocionAppResponseDto? promocion = null;
            int promoActivacion = 0;
            var beanGeneral = new Generic();

            bool validoBbva = await ValidarBbva(inputBeanServ, cancellationToken);

            string tipoNegocio = "";

            var origenCobertura = await _tarifaRepository.RecuperarCobertura(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
            var param_ENABLE_LOGICA = await _parametrosRepository.GetParameterValue("ENABLE_LOGICA_TIPO_NEGOCIO", cancellationToken);
            var param_EMPRESAS_KM = await _parametrosRepository.GetParameterValue("EMPRESAS_TARIFAS_KM_TARIFA", cancellationToken);
            var param_REDONDEO = await _parametrosRepository.GetParameterValue("REDONDEO_MITAD_CERCANO", cancellationToken);
            var param_MEJORA_KM = await _parametrosRepository.GetParameterValue("MEJORA_CALCULO_TARIFA_KM", cancellationToken);
            var param_ACTIVA_PORCENTAJE = await _parametrosRepository.GetParameterValue("ACTIVAR_EMPRESAS_%", cancellationToken);
            var param_EMPRESA_PORCENTAJE = await _parametrosRepository.GetParameterValue("EMPRESAS_%", cancellationToken);
            var param_UNIDAD_DISTANCIA = await _parametrosRepository.GetParameterValue("UNIDAD_DISTANCIA", cancellationToken);
            var param_MENSAJE_PROMO = await _parametrosRepository.GetParameterValue("MENSAJE_PROMOCION", cancellationToken);

            if (origenCobertura == null)
            {
                return Result.Failure<TarifaDetalleResponseDto>(Error.Validation("Por el momento esta zona esta fuera de nuestra cobertura.(Origen)"));
            }

            if (inputBeanServ.TipoServicio == 0)
            {

                var tipoServicios = await _descargarMaestroRepository.RecuperaTipoServicioCobertura(inputBeanServ.IdEmpresa, 2, cancellationToken);
                var resultado = tipoServicios.Where(p => p.ISOCountryCode == origenCobertura.ISOCountryCode).ToList();
                if (resultado.Count > 0)
                {
                    inputBeanServ.TipoServicio = resultado[0].CODI_ORDEN;
                }
            }

            if (inputBeanServ.IdTipoPago == 0)
            {
                var tipoPago = await _descargarMaestroRepository.RecuperaTipoPagoClienteCobertura(inputBeanServ.IdEmpresa, 2, inputBeanServ.IdCliente, cancellationToken);
                var resultado = tipoPago.Where(p => p.ISOCountryCode == origenCobertura.ISOCountryCode).ToList();
                if (resultado.Count > 0)
                {
                    inputBeanServ.IdTipoPago = resultado[0].CODI_ORDEN;
                }
            }

            var fechaActualServidor = DateTime.Now;

            // CAMBIO ETAPA 1 (SIN TRY/CATCH)
            if (string.IsNullOrEmpty(inputBeanServ.DtFechaServicio))
            {
                inputBeanServ.DtFechaServicio = fechaActualServidor.ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                if (!DateTime.TryParseExact(inputBeanServ.DtFechaServicio, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaActualServidor) &&
                    !DateTime.TryParseExact(inputBeanServ.DtFechaServicio, "dd/MM/yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaActualServidor))
                {
                    _logger.LogInformation($"Error en solicitarPrecio formato de fecha incorrecto : {inputBeanServ.DtFechaServicio}");
                    return Result.Failure<TarifaDetalleResponseDto>(Error.Validation($"Error en solicitarPrecio formato de fecha incorrecto : {inputBeanServ.DtFechaServicio}"));
                }
            }

            var primerDestinoSiemreEmpresa = await _servicioRepository.TodosPrimerDestinoEmpresa(inputBeanServ.IdEmpresa, cancellationToken);
            inputBeanServ.PrimerDestino = primerDestinoSiemreEmpresa ? primerDestinoSiemreEmpresa : inputBeanServ.PrimerDestino;
            beanTarifa.DescuentoSinPromocion = false;

            try
            {

                var destinoCobertura = await _tarifaRepository.RecuperarCobertura(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);

                if (origenCobertura == null)
                    return Result.Failure<TarifaDetalleResponseDto>(Error.Validation("Por el momento esta zona esta fuera de nuestra cobertura.(Origen)"));

                if (destinoCobertura == null)
                    return Result.Failure<TarifaDetalleResponseDto>(Error.Validation("Por el momento esta zona esta fuera de nuestra cobertura.(Destino)"));

                if (origenCobertura.ISOCountryCode != destinoCobertura.ISOCountryCode)
                    return Result.Failure<TarifaDetalleResponseDto>(Error.Validation("Por el momento esta zona esta fuera de nuestra cobertura."));

                _logger.LogInformation($"El codigo de Pais de la cobertura es: {origenCobertura.codigoPais}");

                #region Origen,DestinoEsZona
                var origenEsZona1 = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                var destinoEsZona1 = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                #endregion Origen,DestinoEsZona

                if (param_ENABLE_LOGICA == "1")
                {
                    _logger.LogInformation("ENABLE_LOGICA_TIPO_NEGOCIO is enable");

                    var lstLista = Utilitaries.GetListaDeEmpresas(param_EMPRESAS_KM);

                    if (Utilitaries.TienePropiedad(configZonas, origenEsZona1.IdZona, "USA_FORMULA_ZONA") ||
                        Utilitaries.TienePropiedad(configZonas, destinoEsZona1.IdZona, "USA_FORMULA_ZONA") ||
                        (!validoBbva && inputBeanServ.IdEmpresa == 13510))
                    {

                        if (inputBeanServ.IdEmpresa == 13235 && (inputBeanServ.LstDestinosLejanos != null && inputBeanServ.LstDestinosLejanos.Count > 0))
                        {
                            inputBeanServ.ZonaLatamOrigen = origenEsZona1.IdZona;
                            inputBeanServ.ZonaLatamDestino = destinoEsZona1.IdZona;

                            var result = await CalcularZonaLejana(inputBeanServ, cancellationToken);
                            inputBeanServ.ZonaLatamDestino = result.ZonaDestino;
                            inputBeanServ.ZonaLatamOrigen = result.ZonaOrigen;
                        }

                        var (response, zonaOrigenDescripcion, zonaDestinoDescripcion) = await SolicitarFormulaXZona(inputBeanServ, cancellationToken);

                        beanTarifa = response;
                        beanTarifa.ZonaDestino = zonaDestinoDescripcion;
                        beanTarifa.ZonaOrigen = zonaOrigenDescripcion;

                        await AsignarDistancia(beanTarifa, inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                    }
                    else
                    {

                        tipoNegocio = await _servicioRepository.ObteneTipoFormaCalculoEmpresa(inputBeanServ.TipoServicio, 2, inputBeanServ.IdEmpresa, cancellationToken);

                        if (tipoNegocio.Equals("1") || tipoNegocio.Equals("2"))
                        {

                            var parametrosEmpresa = await _tarifaRepository.ObtenerTarifaFormulaEmpresa(inputBeanServ.IdEmpresa, inputBeanServ.TipoServicio, cancellationToken);
                            var redondeo_mitad_cercano = await _parametrosRepository.GetParameterValue("REDONDEO_MITAD_CERCANO", cancellationToken);
                            #region Origen,DestinoEsZonaX

                            var (ExisteO, IdZonaO) = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                            var (ExisteD, IdZonaD) = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                            #endregion Origen,DestinoEsZonaX

                            _logger.LogInformation("CALCULANDO POR DISTANCIA");

                            string unidadDistancia = parametrosEmpresa[KeyFormulaEmpresa.UNIDAD_DISTANCIA];
                            decimal total = 0;
                            decimal montoPorDistancia = 0;

                            var (distancia2, overView2, time2) = await _httpApiRepository.ObtenerMetrajeDePuntosWithOverview(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                            decimal Distancia2 = distancia2;
                            string Overview2 = overView2;
                            decimal Time2 = time2;
                            decimal cambiosDeDistancia = decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.CANTIDAD_POR_METRO]);
                            Distancia2 *= cambiosDeDistancia;

                            decimal minKm = decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.MIN_KM]);
                            decimal constanteZona = 0;

                            constanteZona = await _tarifaRepository.ObtenerConstanteZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);

                            if (constanteZona == 0)
                            {
                                constanteZona = await _tarifaRepository.ObtenerConstanteZona(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                            }

                            _logger.LogInformation($"Empresa solicitar precio empresa : {inputBeanServ.IdEmpresa} distancia:{Distancia2}");

                            if (inputBeanServ.IdEmpresa == 166 || inputBeanServ.IdEmpresa == 175)
                            {
                                if (Distancia2 >= 20)
                                {
                                    parametrosEmpresa[KeyFormulaEmpresa.COSTO_POR_DISTANCIA] = "1.5";
                                }
                            }

                            //TODO:Demasiada carga de Logs...

                            if (parametrosEmpresa.ContainsKey(KeyFormulaEmpresa.DELTA) && parametrosEmpresa[KeyFormulaEmpresa.DELTA] != "")
                            {
                                decimal distanciaAdicional = Convert.ToDecimal(parametrosEmpresa[KeyFormulaEmpresa.DELTA]);
                                if (parametrosEmpresa.ContainsKey(KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO) && parametrosEmpresa[KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO] == "1" && inputBeanServ.PrimerDestino)
                                {
                                    Distancia2 += (distanciaAdicional * cambiosDeDistancia);
                                }
                                else if (!parametrosEmpresa.ContainsKey(KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO) || (parametrosEmpresa.ContainsKey(KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO) && (parametrosEmpresa[KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO] == "0" || parametrosEmpresa[KeyFormulaEmpresa.DELTA_ONLY_DESTINO_INICIO] == "")))
                                {
                                    Distancia2 += (distanciaAdicional * cambiosDeDistancia);
                                }
                            }

                            if (inputBeanServ.PrimerDestino && Distancia2 <= minKm /*&& minKm >= 0 */&& parametrosEmpresa[KeyFormulaEmpresa.ENABLE_MIN_COST].Equals("1"))
                            {
                                total = decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.MIN_COST].ToString()) + constanteZona;
                                _logger.LogInformation($"Distancia menor que minimo distancia:{Distancia2} minKm:{minKm} primerDesitno? {inputBeanServ.PrimerDestino}");
                            }
                            else
                            {
                                montoPorDistancia = decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.COSTO_POR_DISTANCIA]);
                                _logger.LogInformation($"montoPorDistancia: {montoPorDistancia}");
                                _logger.LogInformation($"validacion punto: {inputBeanServ.PrimerDestino}");

                                if (inputBeanServ.IdEmpresa == 1104 || inputBeanServ.IdEmpresa == 13510)
                                {
                                    Distancia2 = (Math.Truncate(10 * Distancia2) / 10);
                                }

                                if (inputBeanServ.PrimerDestino)
                                {
                                    if (Distancia2 > minKm)
                                    {
                                        Distancia2 -= minKm;
                                        total = Distancia2 * montoPorDistancia;
                                        Distancia2 += minKm;
                                    }
                                    else
                                    {
                                        if (param_MEJORA_KM == "1")
                                        {
                                            total = 0;
                                        }
                                        else
                                        {
                                            total = Distancia2 * montoPorDistancia;
                                        }
                                    }

                                }
                                else
                                {
                                    if (inputBeanServ.IdEmpresa == 13466 || inputBeanServ.IdEmpresa == 13404 || inputBeanServ.IdEmpresa == 1104 || (inputBeanServ.IdEmpresa == 13510 && validoBbva))
                                    {
                                        if (Distancia2 > minKm)
                                        {
                                            Distancia2 -= minKm;
                                            total = Distancia2 * montoPorDistancia;
                                            Distancia2 += minKm;
                                        }
                                        else
                                        {
                                            if ((await _parametrosRepository.GetParameterValue("MEJORA_CALCULO_TARIFA_KM", cancellationToken)) == "1")
                                            {
                                                total = 0;
                                            }
                                            else
                                            {
                                                total = Distancia2 * montoPorDistancia;
                                            }
                                        }
                                    }
                                }

                                if (inputBeanServ.PrimerDestino)
                                {
                                    total = total + (parametrosEmpresa[KeyFormulaEmpresa.CONSTANTE_COBRO] == null ? decimal.Parse("0") : decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.CONSTANTE_COBRO])) + constanteZona;
                                }
                                else
                                {
                                    if (inputBeanServ.IdEmpresa == 13466 || inputBeanServ.IdEmpresa == 13404 || inputBeanServ.IdEmpresa == 1104 || (inputBeanServ.IdEmpresa == 13510 && validoBbva))
                                    {
                                        total = total + (parametrosEmpresa[KeyFormulaEmpresa.CONSTANTE_COBRO] == null ? decimal.Parse("0") : decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.CONSTANTE_COBRO])) + constanteZona;

                                    }
                                }

                                var costoTotalTiempo = Math.Round(Time2 / 60) * decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.COSTO_POR_MINUTO] == null ? "0" : parametrosEmpresa[KeyFormulaEmpresa.COSTO_POR_MINUTO]);
                                total += costoTotalTiempo;

                                _logger.LogInformation($"TotalMonto {(total - costoTotalTiempo)} distancia = {Distancia2} monto x distancia :{montoPorDistancia} tiempo :{Time2} costoXtiempo : {parametrosEmpresa[KeyFormulaEmpresa.COSTO_POR_MINUTO]}  sale = {costoTotalTiempo}");
                                _logger.LogInformation($"total = {total}");

                                var tipoRound = parametrosEmpresa[KeyFormulaEmpresa.TYPE_ROUND];

                                if (tipoRound == "FLOOR" && (inputBeanServ.IdEmpresa == 1104 || inputBeanServ.IdEmpresa == 13510))
                                {
                                    total = Math.Floor(total * 10) / 10;
                                }
                                else
                                {
                                    total = AplicarRedondeo(total, tipoRound, param_REDONDEO);
                                }

                                total = Math.Truncate(100 * total) / 100;

                                if (inputBeanServ.PrimerDestino && parametrosEmpresa[KeyFormulaEmpresa.ENABLE_MIN_COST].Equals("1"))
                                {
                                    decimal costoMinimo = decimal.Parse(parametrosEmpresa[KeyFormulaEmpresa.MIN_COST]/*ConfigurationManager.AppSettings["MIN_COST"]*/.ToString());
                                    if (total < costoMinimo)
                                    {
                                        total = costoMinimo;
                                    }
                                }
                            }


                            if (param_ACTIVA_PORCENTAJE == "1")
                            {
                                if (inputBeanServ.IdEmpresa == Convert.ToInt32(param_EMPRESA_PORCENTAJE) && inputBeanServ.IdTipoPago == 6)
                                {
                                    _logger.LogInformation("SE APLICA LOGICA % PARQUE");
                                    total = AplicarPorcentaje(total, inputBeanServ);
                                }
                            }

                            beanTarifa.Distancia = (Math.Truncate(10 * Distancia2) / 10) + " " + unidadDistancia;
                            beanTarifa.Kilometros = (Math.Truncate(10 * Distancia2) / 10);
                            beanTarifa.TiempoViaje = Math.Round(Time2 / 60);

                            AsignarTarifaBase(beanTarifa, total, Overview2);
                        }
                        else if (tipoNegocio.Equals("0") || tipoNegocio.Equals("3"))
                        {
                            if (inputBeanServ.IdEmpresa == 13235 && (inputBeanServ.LstDestinosLejanos != null && inputBeanServ.LstDestinosLejanos.Count > 0))
                            {
                                inputBeanServ.ZonaLatamDestino = destinoEsZona1.IdZona;
                                inputBeanServ.ZonaLatamOrigen = origenEsZona1.IdZona;

                                var result = await CalcularZonaLejana(inputBeanServ, cancellationToken);
                                inputBeanServ.ZonaLatamDestino = result.ZonaDestino;
                                inputBeanServ.ZonaLatamOrigen = result.ZonaOrigen;
                            }

                            var (response, zonaOrigenDescripcion, zonaDestinoDescripcion) = await SolicitarFormulaXZona(inputBeanServ, cancellationToken);
                            beanTarifa = response;

                            if ((await _parametrosRepository.GetParameterValue("ACTIVAR_EMPRESAS_%", cancellationToken)) == "1")
                            {
                                if (inputBeanServ.IdEmpresa == Convert.ToUInt32(await _parametrosRepository.GetParameterValue("EMPRESAS_%", cancellationToken)))
                                {
                                    if (inputBeanServ.LstDestinosBO.Count == 2 && inputBeanServ.PosicionDestino == 1)
                                    {
                                        AplicarPorcentajeTarifa(beanTarifa, inputBeanServ);
                                    }

                                    if (inputBeanServ.LstDestinosBO.Count >= 3)
                                    {
                                        if (inputBeanServ.LstDestinosBO.Count == (inputBeanServ.PosicionDestino + 1))
                                        {
                                            beanTarifa.Monto = ((beanTarifa.Monto * 50) / 100);
                                            beanTarifa.TotalTarifa = ((beanTarifa.TotalTarifa * 50) / 100);
                                            beanTarifa.TarifaBase = ((beanTarifa.TarifaBase * 50) / 100);
                                        }
                                    }

                                    if (inputBeanServ.CantidadDestino == 2 && inputBeanServ.PosicionDestino == 1)
                                    {
                                        AplicarPorcentajeTarifa(beanTarifa, inputBeanServ);
                                    }

                                    if (inputBeanServ.CantidadDestino >= 3)
                                    {
                                        if (inputBeanServ.CantidadDestino == ((inputBeanServ.PosicionDestino + 1)))
                                        {
                                            beanTarifa.Monto = ((beanTarifa.Monto * 50) / 100);
                                            beanTarifa.TotalTarifa = ((beanTarifa.TotalTarifa * 50) / 100);
                                            beanTarifa.TarifaBase = ((beanTarifa.TarifaBase * 50) / 100);
                                        }
                                    }
                                }
                            }

                            beanTarifa.ZonaDestino = zonaDestinoDescripcion;
                            beanTarifa.ZonaOrigen = zonaOrigenDescripcion;

                            await AsignarDistancia(beanTarifa, inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                        }

                        beanTarifa.FormaCalculo = tipoNegocio;
                    }
                }
                else
                {
                    if (inputBeanServ.TipoServicio == 7)
                    {
                        beanTarifa.Distancia = "";
                        beanTarifa.Abono = /*tarifa.Abono*/0;
                        beanTarifa.Monto = 0;
                        beanTarifa.TarifaBase = 0;
                        beanTarifa.TotalTarifa = 0;
                        beanTarifa.PagoAdelantado = false;
                        beanTarifa.OverviewPolyline = "";
                        beanTarifa.IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_OK_NOMSG;
                    }
                    else if (inputBeanServ.TipoServicio == 8)
                    {
                        var redondeo_mitad_cercano = await _parametrosRepository.GetParameterValue("REDONDEO_MITAD_CERCANO", cancellationToken);

                        var origenEsZona2 = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                        var destinoEsZona2 = await _tarifaRepository.VerificarPuntoEnZonaIdZona(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                        var unidadDistancia = param_UNIDAD_DISTANCIA;
                        decimal total = 0;
                        decimal montoPorDistancia = 0;

                        var (distancia4, overView4, time4) = await _httpApiRepository.ObtenerMetrajeDePuntosWithOverview(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                        decimal Distancia4 = distancia4;
                        string Overview4 = overView4;
                        decimal Time4 = time4;
                        decimal cambiosDeDistancia = _config.CANTIDAD_POR_METRO; ;
                        Distancia4 *= cambiosDeDistancia;
                        montoPorDistancia = _config.COSTO_POR_DISTANCIA;


                        total = Distancia4 * montoPorDistancia;

                        total = total + (_config.CONSTANTE_COBRO == 0 ? 0 : _config.CONSTANTE_COBRO);
                        total += Math.Round(Time4 / 60) * (_config.COSTO_POR_MINUTO == 0 ? 0 : _config.COSTO_POR_MINUTO);

                        total = AplicarRedondeo(total, _config.TYPE_ROUND, param_REDONDEO);

                        if (_config.ENABLE_MIN_COST == "1")
                        {
                            decimal costoMinimo = _config.MIN_COST;
                            if (total < costoMinimo)
                            {
                                total += costoMinimo;
                            }
                        }

                        if ((await _parametrosRepository.GetParameterValue("ACTIVAR_EMPRESAS_%", cancellationToken)) == "1")
                        {
                            if (inputBeanServ.IdEmpresa == Convert.ToInt32(await _parametrosRepository.GetParameterValue("EMPRESAS_%", cancellationToken)))
                            {

                                total = AplicarPorcentaje(total, inputBeanServ);
                            }

                        }

                        beanTarifa.Distancia = (Math.Truncate(10 * Distancia4) / 10) + " " + unidadDistancia;
                        beanTarifa.Kilometros = (Math.Truncate(10 * Distancia4) / 10);
                        beanTarifa.TiempoViaje = Math.Round(Time4 / 60);

                        AsignarTarifaBase(beanTarifa, total, Overview4);
                    }
                    else
                    {
                        beanTarifa = await SolicitarPrecio(inputBeanServ, cancellationToken);

                        if ((await _parametrosRepository.GetParameterValue("ACTIVAR_EMPRESAS_%", cancellationToken)) == "1")
                        {
                            if (inputBeanServ.IdEmpresa == Convert.ToInt32(await _parametrosRepository.GetParameterValue("EMPRESAS_%", cancellationToken)))
                            {

                                if (inputBeanServ.LstDestinosBO.Count == 2 && inputBeanServ.PosicionDestino == 1)
                                {
                                    AplicarPorcentajeTarifa(beanTarifa, inputBeanServ);
                                }

                                if (inputBeanServ.LstDestinosBO.Count >= 3)
                                {
                                    if (inputBeanServ.LstDestinosBO.Count == (inputBeanServ.PosicionDestino + 1))
                                    {
                                        beanTarifa.Monto = ((beanTarifa.Monto * 50) / 100);
                                        beanTarifa.TotalTarifa = ((beanTarifa.TotalTarifa * 50) / 100);
                                        beanTarifa.TarifaBase = ((beanTarifa.TarifaBase * 50) / 100);
                                    }
                                }

                                if (inputBeanServ.CantidadDestino == 2 && inputBeanServ.PosicionDestino == 1)
                                {
                                    AplicarPorcentajeTarifa(beanTarifa, inputBeanServ);
                                }

                                if (inputBeanServ.CantidadDestino >= 3)
                                {
                                    if (inputBeanServ.CantidadDestino == ((inputBeanServ.PosicionDestino + 1)))
                                    {
                                        beanTarifa.Monto = ((beanTarifa.Monto * 50) / 100);
                                        beanTarifa.TotalTarifa = ((beanTarifa.TotalTarifa * 50) / 100);
                                        beanTarifa.TarifaBase = ((beanTarifa.TarifaBase * 50) / 100);
                                    }
                                }

                            }

                        }

                        if (!inputBeanServ.SoloTarifa)
                        {
                            await AsignarDistancia(beanTarifa, inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                        }
                    }
                }

                var zonaOrigenId = await _zonaRepository.ObtenerIdZona(
                    inputBeanServ.OrigenLatitud,
                    inputBeanServ.OrigenLongitud,
                    cancellationToken);

                var zonaDestinoId = await _zonaRepository.ObtenerIdZona(
                    inputBeanServ.DestinoLatitud,
                    inputBeanServ.DestinoLongitud,
                    cancellationToken);

                //TODO: USO DE ZONAS ESPECIALIZADA SEGUN DOCUMENTO REQUERIMIENTO, SE CREO TABLA CONFIGURABLE "configuracion_zonas" EN POSTGRES
                beanTarifa.IsAirportModulo = Utilitaries.TienePropiedad(configZonas, zonaOrigenId, "USA_FORMULA_ZONA");
                beanTarifa.IsJockeyModulo = Utilitaries.TienePropiedad(configZonas, zonaOrigenId, "ES_JOCKEY");
                beanTarifa.IsAirportOrigin = Utilitaries.TienePropiedad(configZonas, zonaOrigenId, "USA_FORMULA_ZONA");
                beanTarifa.IsAirportDestino = Utilitaries.TienePropiedad(configZonas, zonaDestinoId, "USA_FORMULA_ZONA");

                if (beanTarifa.IsAirportOrigin)
                {
                    beanTarifa.IsAirport = true;
                }

                decimal totalTarifaBase = inputBeanServ.LstDestinosBO != null && inputBeanServ.LstDestinosBO.Count > 0 ? inputBeanServ.LstDestinosBO.Sum(destino => destino.Tarifa) : 0;

                var zonaOrigen = await _zonaRepository.RecuperaByPosicion(inputBeanServ.IdEmpresa, inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                var zonaDestino = await _zonaRepository.RecuperaByPosicion(inputBeanServ.IdEmpresa, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);

                var recargoEspecial = new BeanRecargoEspecialResponseDto();
                var horaPunta = new BeanHoraPuntaResponseDto();
                bool validacionRecargoZona = true;
                bool incrementoMax = false;

                if (Utilitaries.TienePropiedad(configZonas, zonaOrigen, "BLOQUEA_RECARGO") || Utilitaries.TienePropiedad(configZonas, zonaDestino, "BLOQUEA_RECARGO"))
                {
                    validacionRecargoZona = false;
                }

                if (validacionRecargoZona)
                {
                    if (beanTarifa.IsAirportOrigin || beanTarifa.IsAirportDestino)
                    {

                        recargoEspecial = await _recargoEspecialRepository.GetRecargoEspecialAeropuerto(inputBeanServ.IdEmpresa, inputBeanServ.IdTipoPago, inputBeanServ.TipoServicio, fechaActualServidor, beanTarifa.IsAirportOrigin ? 1 : 2, cancellationToken);
                    }
                    else
                    {

                        recargoEspecial = await _recargoEspecialRepository.GetRecargoEspecialTipoServicioTipoPago(inputBeanServ.IdEmpresa, inputBeanServ.IdTipoPago, inputBeanServ.TipoServicio, fechaActualServidor, cancellationToken);

                        if (recargoEspecial == null)
                        {

                            horaPunta = await _horaPuntaRepository.GetHoraPuntaOnValue(inputBeanServ.IdEmpresa, inputBeanServ.IdTipoPago, inputBeanServ.TipoServicio, fechaActualServidor, cancellationToken);
                            int zona_origen_1 = 0;

                            if (inputBeanServ.LstDestinosBO != null)
                            {
                                if (inputBeanServ.LstDestinosBO.Count > 0)
                                {

                                    zona_origen_1 = await _zonaRepository.ObtenerIdZona(inputBeanServ.LstDestinosBO[0].OrigenLatitud, inputBeanServ.LstDestinosBO[0].OrigenLatitud, cancellationToken);
                                    if (zona_origen_1 == 35)
                                    {
                                        if (horaPunta != null)
                                        {
                                            horaPunta.Activo = false;
                                            horaPunta.HoraInicio = TimeSpan.Zero;
                                            horaPunta.HoraFin = TimeSpan.Zero;
                                            horaPunta.Incremento = 0;
                                            horaPunta.TipoCalculo = 0;
                                            horaPunta.IdEmpresa = 0;
                                            horaPunta.IdHoraPunta = 0;
                                            horaPunta.IdTipoServicio = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #region Todo esto es Hora Punta
                if (horaPunta != null && horaPunta.IdHoraPunta != 0)
                {
                    if (!beanTarifa.IsAirportOrigin && inputBeanServ.TipoServicio != 5 && horaPunta != null && !inputBeanServ.IsPorTiempo)
                    {
                        int zona_origen_ = 0;
                        if (inputBeanServ.LstMultiPuntos != null && inputBeanServ.LstMultiPuntos.Count > 0)
                        {

                            zona_origen_ = await _zonaRepository.ObtenerIdZona(inputBeanServ.LstMultiPuntos[0].OrigenLatitud, inputBeanServ.LstMultiPuntos[0].OrigenLongitud, cancellationToken);
                        }

                        if (zona_origen_ > 0)
                        {

                            beanTarifa = await CalcularIncremento2(beanTarifa, horaPunta, totalTarifaBase, zona_origen_);
                        }
                        else
                        {

                            beanTarifa = await CalcularIncremento(beanTarifa, horaPunta, totalTarifaBase);
                        }

                    }

                    if (promocion == null)
                    {
                        if (!beanTarifa.DescuentoSinPromocion)
                        {
                            beanTarifa.MontoSinDescuento = beanTarifa.Monto;
                        }
                    }
                }
                #endregion Todo esto es Hora Punta

                #region Recargo Especial
                if (recargoEspecial != null && recargoEspecial.idRecargoEspecial != 0)
                {

                    beanTarifa = await _recargoEspecialService.CalcularIncremento(beanTarifa, recargoEspecial, totalTarifaBase, incrementoMax, cancellationToken);
                }
                #endregion Recargo Especial

                #region Recargo reserva
                if (validacionRecargoZona)
                {

                    var recargoReserva = await _recargoReservaRepository.GetRecargoReservaOnValue(inputBeanServ.IdTipoPago, inputBeanServ, beanTarifa, fechaActualServidor, totalTarifaBase, cancellationToken);
                    if (recargoReserva != null && recargoReserva.IdRecargoReserva != 0)
                    {
                        if (!beanTarifa.IsAirportOrigin && inputBeanServ.TipoServicio != 5 && recargoReserva != null && !inputBeanServ.IsPorTiempo && inputBeanServ.Multidestino == false)
                        {
                            beanTarifa = await _recargoReservaService.CalcularIncremento(beanTarifa, recargoReserva, totalTarifaBase, cancellationToken);
                        }

                        if (promocion == null)
                        {
                            if (!beanTarifa.DescuentoSinPromocion)
                            {
                                beanTarifa.MontoSinDescuento = beanTarifa.Monto;
                            }
                        }

                    }
                }
                #endregion Recargo reserva

                #region Tiempo minimo reserva
                var beanTarifaZona = new TarifaZonaResponseDto();
                beanTarifaZona.IdZona = await _zonaRepository.ObtenerIdZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                beanTarifaZona.IdEmpresa = inputBeanServ.IdEmpresa;
                beanTarifaZona.HoraActual = DateTime.Now;
                beanTarifaZona.Minutos = await _servicioRepository.ObtenerTiempoPorZona(beanTarifaZona.IdZona, beanTarifaZona.HoraActual, beanTarifaZona.IdEmpresa, cancellationToken);
                beanTarifa.TiempoMinimoReserva = beanTarifaZona.Minutos;
                #endregion Tiempo minimo reserva

                beanTarifa.TotalTarifaApp = beanTarifa.TotalTarifa;

                #region DESCUENTO PROMOCION
                if (inputBeanServ.IdPromoActivacion != 0)
                {
                    PromoValidacionRequestDto MyDynamic = new PromoValidacionRequestDto();
                    MyDynamic.IdCliente = inputBeanServ.IdCliente;
                    MyDynamic.IdEmpresa = inputBeanServ.IdEmpresa;
                    MyDynamic.OrigenLatitud = inputBeanServ.OrigenLatitud;
                    MyDynamic.OrigenLongitud = inputBeanServ.OrigenLongitud;
                    MyDynamic.DestinoLatitud = inputBeanServ.DestinoLatitud;
                    MyDynamic.DestinoLongitud = inputBeanServ.DestinoLongitud;
                    MyDynamic.TipoPago = inputBeanServ.IdTipoPago;
                    MyDynamic.TipoServicio = inputBeanServ.TipoServicio;
                    MyDynamic.FechaServicio = String.IsNullOrEmpty(inputBeanServ.DtFechaServicio) ? DateTime.Now.ToString("yyyy-mm-dd HH:mm") : inputBeanServ.DtFechaServicio;
                    MyDynamic.TotalServicio = beanTarifa.TotalTarifa;
                    MyDynamic.IdPromocionActivacion = inputBeanServ.IdPromoActivacion;
                    MyDynamic.IdPromocion = inputBeanServ.IdPromocion;
                    var beanValidarPromo = new Generic();

                    try
                    {
                        //validacion
                        if (promoActivacion != 2)
                        {
                            beanValidarPromo = await _promocionesService.ValidarPromo(MyDynamic, cancellationToken);
                            promoActivacion = beanValidarPromo.IdResultado;
                            beanGeneral = beanValidarPromo;
                        }
                        else
                        {
                            beanValidarPromo = beanGeneral;
                        }

                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error validarPromo solicitarTarifa ex {e.Message}");
                        _logger.LogError($"Error validarPromo solicitarTarifa ex stack {e.StackTrace}");
                    }

                    if (beanValidarPromo.IdResultado == 2 && beanValidarPromo.Value != "0")
                    {
                        promocion = await _promocionesRepository.ObtenerPromocionClienteId(inputBeanServ.IdCliente, inputBeanServ.IdPromoActivacion, 0, cancellationToken);
                        if (promocion == null)
                        {
                            return Result.Failure<TarifaDetalleResponseDto>(Error.Validation("La promocion seleccionada ya no esta disponible."));
                        }
                    }

                }
                #endregion DESCUENTO PROMOCION

                if (promocion != null)
                {
                    if (promocion.I057_ModalidadPromocion == 1)
                    {
                        beanTarifa.MontoSinDescuento = beanTarifa.Monto;

                        beanTarifa.Descuento = promocion.ValorPromocion;

                        beanTarifa.NombrePromocion = promocion.Nombre;

                        beanTarifa.Monto -= beanTarifa.Descuento > beanTarifa.Monto ? beanTarifa.Monto : beanTarifa.Descuento;

                        beanTarifa.TotalTarifaApp -= beanTarifa.Descuento > beanTarifa.TotalTarifa ? beanTarifa.TotalTarifa : beanTarifa.Descuento;
                    }
                    else if (promocion.I057_ModalidadPromocion == 2)
                    {
                        beanTarifa.MontoSinDescuento = beanTarifa.Monto;
                        beanTarifa.Descuento = Convert.ToDecimal(Math.Round((double)promocion.ValorPromocion * (double)beanTarifa.Monto * 0.01, 1));
                        beanTarifa.Monto -= beanTarifa.Descuento > beanTarifa.Monto ? beanTarifa.Monto : beanTarifa.Descuento;
                        beanTarifa.TotalTarifaApp -= beanTarifa.Descuento > beanTarifa.TotalTarifa ? beanTarifa.TotalTarifa : beanTarifa.Descuento;

                        beanTarifa.NombrePromocion = promocion.Nombre;
                    }
                }
                else
                {
                    beanTarifa.MontoSinDescuento = beanTarifa.Monto;
                }

                #region Plazas Directo

                if (await _plazaRepository.ValidarZonaPlaza(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, beanTarifa.ZonaOrigenId) && inputBeanServ.ModoReserva == 2)
                {
                    decimal Monto = await _plazaRepository.RecalculoTarifaPlaza(beanTarifa.ZonaOrigenId, beanTarifa.ZonaDestinoId, fechaActualServidor, cancellationToken);
                    if (Monto != 0)
                    {
                        beanTarifa.Monto = Monto;
                        beanTarifa.MontoSinDescuento = Monto;
                        beanTarifa.TotalTarifa = Monto;
                        beanTarifa.TotalTarifaApp = Monto;
                        beanTarifa.TarifaBase = Monto;
                    }
                }
                #endregion Plazas Directo

                beanTarifa.LstPromociones = await _promocionesRepository.ObtenerPromocionCliente(inputBeanServ.IdCliente, cancellationToken);
                string mensajePromocion = param_MENSAJE_PROMO;
                if (mensajePromocion.Length > 0)
                {
                    beanTarifa.MensajePromocion = mensajePromocion;
                }

                var region = new RegionInfo(CultureInfo.CurrentCulture.Name);

                beanTarifa.CurrencySymbol = region.CurrencySymbol;
                beanTarifa.ISOCountryCodeAlt = origenCobertura.ISOCountryCode;

                beanTarifa.OrigenPeligro = await _zonaRepository.IsZonaPeligrosa(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                beanTarifa.DestinoPeligro = await _zonaRepository.IsZonaPeligrosa(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                beanTarifa.Compania = await _companiaRepository.RecuperarCompania(inputBeanServ.IdEmpresa, inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);


            }
            catch (Exception ex)
            {
                beanTarifa.IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_ERROR_MSG;
                beanTarifa.Resultado = ex.Message;
                //return Result.Failure<TarifaDetalleResponseDto>(Error.Validation($"Error en servicio metodo Calcular: {ex.Message}"));
            }

            if (inputBeanServ.SinDestino)
            {
                beanTarifa.Monto = 0;
                beanTarifa.MontoSinDescuento = 0;
            }

            beanTarifa.Values = await _clienteRepository.ObtenerePresupuestoCliente(inputBeanServ.IdCliente, cancellationToken);

            try
            {
                List<int> listaZonasParaPeaje = new List<int>();

                if (inputBeanServ.LstDestinosBO != null && inputBeanServ.LstDestinosBO.Count > 0)
                {
                    foreach (var punto in inputBeanServ.LstDestinosBO)
                    {
                        int zOrigen = await _zonaRepository.ObtenerIdZona(punto.OrigenLatitud, punto.OrigenLongitud, cancellationToken);
                        if (zOrigen > 0) listaZonasParaPeaje.Add(zOrigen);
                        int zDestino = await _zonaRepository.ObtenerIdZona(punto.DestinoLatitud, punto.DestinoLongitud, cancellationToken);
                        if (zDestino > 0) listaZonasParaPeaje.Add(zDestino);
                    }
                }
                else
                {

                    int zOrigenSimple = await _zonaRepository.ObtenerIdZona(inputBeanServ.OrigenLatitud, inputBeanServ.OrigenLongitud, cancellationToken);
                    if (zOrigenSimple > 0) listaZonasParaPeaje.Add(zOrigenSimple);

                    int zDestinoSimple = await _zonaRepository.ObtenerIdZona(inputBeanServ.DestinoLatitud, inputBeanServ.DestinoLongitud, cancellationToken);
                    if (zDestinoSimple > 0) listaZonasParaPeaje.Add(zDestinoSimple);
                }

                List<int> zonasUnicas = listaZonasParaPeaje.Distinct().ToList();

                _logger.LogInformation($"Zonas detectadas para peaje manual: {string.Join(",", zonasUnicas)}");

                if (zonasUnicas.Count > 0)
                {
                    beanTarifa.LstPeajeSistema = await _peajeRepository.ListarPeajesPorZonas(zonasUnicas, cancellationToken);
                }
                else
                {
                    beanTarifa.LstPeajeSistema = new List<PeajeSistemaRespondeDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error critico calculando Peajes Manuales: {ex.Message}");
                //beanTarifa.LstPeajeSistema = new List<PeajeSistemaRespondeDto>();
                //return Result.Failure<TarifaDetalleResponseDto>(Error.Validation($"Error critico calculando Peajes Manuales: {ex.Message}"));
            }

            return Result.Success(beanTarifa);
        }

        private void AsignarTarifaBase(TarifaDetalleResponseDto beanTarifa, decimal total, string overview)
        {
            beanTarifa.Abono = 0;
            beanTarifa.TarifaBase = total;
            beanTarifa.TotalTarifa = total;
            beanTarifa.Monto = total;
            beanTarifa.PagoAdelantado = false;
            beanTarifa.OverviewPolyline = overview;
            beanTarifa.IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_OK_NOMSG;
        }

        private decimal AplicarRedondeo(decimal total, string? tipoRound, string redondeoMitad)
        {
            var tipo = tipoRound?.ToUpper();

            if (tipo == "FLOOR")
            {
                return redondeoMitad == "1"
                    ? Math.Floor(total * 2) / 2
                    : Math.Floor(total);
            }

            if (tipo == "CEILING")
            {
                return redondeoMitad == "1"
                    ? Math.Ceiling(total * 2) / 2
                    : Math.Ceiling(total);
            }

            // default = ROUND
            return redondeoMitad == "1"
                ? Math.Round(total * 2) / 2
                : Math.Round(total);
        }

        private decimal AplicarPorcentaje(decimal valor, TarifaDestinoRequestDto input)
        {
            if (input.CantidadDestino == 2 && input.PosicionDestino == 1)
                return (valor * 70) / 100;

            if (input.CantidadDestino >= 3 && input.CantidadDestino == (input.PosicionDestino + 1))
                return (valor * 50) / 100;

            return valor;
        }

        private void AplicarPorcentajeTarifa(TarifaDetalleResponseDto tarifa, TarifaDestinoRequestDto input)
        {
            tarifa.Monto = AplicarPorcentaje(tarifa.Monto, input);
            tarifa.TotalTarifa = AplicarPorcentaje(tarifa.TotalTarifa, input);
            tarifa.TarifaBase = AplicarPorcentaje(tarifa.TarifaBase, input);
        }

        private async Task AsignarDistancia(TarifaDetalleResponseDto beanTarifa, double origenLat, double origenLon, double destinoLat, double destinoLon, CancellationToken cancellationToken)
        {
            var (distancia, overView, time) = await _httpApiRepository.ObtenerMetrajeDePuntosWithOverview(origenLat, origenLon, destinoLat, destinoLon, cancellationToken);

            decimal Distancia = distancia;
            decimal cambiosDeDistancia = _config.CANTIDAD_POR_METRO;
            string unidadDistancia = _config.UNIDAD_DISTANCIA;

            Distancia *= cambiosDeDistancia;

            var km = Math.Truncate(10 * Distancia) / 10;

            beanTarifa.Distancia = km + " " + unidadDistancia;
            beanTarifa.Kilometros = km;
            beanTarifa.TiempoViaje = Math.Round(time / 60);
            beanTarifa.OverviewPolyline = overView;
        }

        private async Task<bool> ValidarBbva(TarifaDestinoRequestDto input, CancellationToken cancellationToken)
        {
            if (input.IdEmpresa != 13510)
                return false;

            // caso multipuntos
            if (input.LstMultiPuntos != null && input.LstDestinosBO.Count == 0)
            {
                foreach (var punto in input.LstMultiPuntos)
                {
                    var idZonaOrigen = await _zonaRepository.ObtenerIdZona(punto.OrigenLatitud, punto.OrigenLongitud, cancellationToken);
                    var idZonaDestino = await _zonaRepository.ObtenerIdZona(punto.DestinoLatitud, punto.DestinoLongitud, cancellationToken);

                    if (await _tarifaRepository.ValidarZona(idZonaOrigen, idZonaDestino, cancellationToken))
                        return true;
                }
            }

            // caso normal
            var origen = await _zonaRepository.ObtenerIdZona(input.OrigenLatitud, input.OrigenLongitud, cancellationToken);
            var destino = await _zonaRepository.ObtenerIdZona(input.DestinoLatitud, input.DestinoLongitud, cancellationToken);

            return await _tarifaRepository.ValidarZona(origen, destino, cancellationToken);
        }

        private async Task<(int ZonaOrigen, int ZonaDestino)> CalcularZonaLejana(TarifaDestinoRequestDto serv, CancellationToken cancellationToken)
        {
            if (serv.LstDestinosLejanos == null || serv.LstDestinosLejanos.Count == 0)
                return (0, 0);

            bool esAeropuertoSalida = serv.ZonaLatamOrigen is 1106 or 1107 or 1108;
            bool esAeropuertoEntrada = serv.ZonaLatamDestino is 1106 or 1107 or 1108;

            if (!esAeropuertoSalida && !esAeropuertoEntrada)
                return (0, 0);

            decimal maxDistancia = 0;
            ServicioDestinoRequestDto? destinoSeleccionado = null;

            foreach (var destino in serv.LstDestinosLejanos)
            {
                var distanciaOverview = esAeropuertoSalida
                    ? await _httpApiRepository.ObtenerMetrajeDePuntosWithOverview(
                        serv.OrigenLatitud,
                        serv.OrigenLongitud,
                        destino.DestinoLatitud,
                        destino.DestinoLongitud,
                        cancellationToken)
                    : await _httpApiRepository.ObtenerMetrajeDePuntosWithOverview(
                        serv.DestinoLatitud,
                        serv.DestinoLongitud,
                        destino.OrigenLatitud,
                        destino.OrigenLongitud,
                        cancellationToken);

                var distancia = distanciaOverview.Item1;

                if (distancia > maxDistancia)
                {
                    maxDistancia = distancia;
                    destinoSeleccionado = destino;
                }
            }

            if (destinoSeleccionado == null)
                return (0, 0);

            if (esAeropuertoSalida)
            {
                return (
                    serv.ZonaLatamOrigen,
                    await _zonaRepository.RecuperaByPosicion(
                        13235,
                        destinoSeleccionado.DestinoLatitud,
                        destinoSeleccionado.DestinoLongitud,
                        cancellationToken)
                );
            }

            // entrada
            return (
                await _zonaRepository.RecuperaByPosicion(
                    13235,
                    destinoSeleccionado.OrigenLatitud,
                    destinoSeleccionado.OrigenLongitud,
                    cancellationToken),
                serv.ZonaLatamDestino
            );
        }

        private async Task<(TarifaDetalleResponseDto Response, string ZonaOrigen, string ZonaDestino)> SolicitarFormulaXZona(TarifaDestinoRequestDto beanServicio, CancellationToken cancellationToken)
        {
            string zonaOrigenDescripcion = "";
            string zonaDestinoDescripcion = "";

            var beanTarifa = new TarifaDetalleResponseDto();

            // =========================
            // ZONAS (PARALELO)
            // =========================
            var zonaOrigenTask = _zonaRepository.RecuperaByPosicion(
                beanServicio.IdEmpresa,
                beanServicio.OrigenLatitud,
                beanServicio.OrigenLongitud,
                cancellationToken);

            var zonaDestinoTask = _zonaRepository.RecuperaByPosicion(
                beanServicio.IdEmpresa,
                beanServicio.DestinoLatitud,
                beanServicio.DestinoLongitud,
                cancellationToken);

            await Task.WhenAll(zonaOrigenTask, zonaDestinoTask);

            var zonaOrigen = await zonaOrigenTask;
            var zonaDestino = await zonaDestinoTask;

            // =========================
            // TARIFA
            // =========================
            RecuperaByIdFormaCalculoResponseDto tarifa;

            if (beanServicio.IdEmpresa == 13235 && beanServicio.LstDestinosLejanos?.Count > 0)
            {
                tarifa = await _tarifaRepository.RecuperaByIdFormaCalculoLatam(
                    beanServicio.IdEmpresa,
                    beanServicio.IdCliente,
                    beanServicio.TipoServicio,
                    beanServicio.IdTipoPago,
                    beanServicio.ZonaLatamOrigen,
                    beanServicio.ZonaLatamDestino,
                    beanServicio.CantPasajeros,
                    cancellationToken);
            }
            else if (beanServicio.IdEmpresa == 13407 && beanServicio.LstDestinosLejanos?.Count > 0)
            {
                tarifa = await _tarifaRepository.RecuperaByIdFormaCalculo(
                    beanServicio.IdEmpresa,
                    beanServicio.TipoServicio,
                    beanServicio.IdTipoPago,
                    zonaOrigen,
                    zonaDestino,
                    beanServicio.IdCliente,
                    beanServicio.CantPasajeros,
                    cancellationToken);
            }
            else
            {
                if (beanServicio.LstDestinosBO.Count > 0)
                {
                    beanServicio.IdTipoPago = beanServicio.LstDestinosBO[0].IdTipoPago;
                }

                tarifa = await _tarifaRepository.RecuperaByIdFormaCalculo(
                    beanServicio.IdEmpresa,
                    beanServicio.TipoServicio,
                    beanServicio.IdTipoPago,
                    zonaOrigen,
                    zonaDestino,
                    beanServicio.IdCliente,
                    0,
                    cancellationToken);
            }

            // =========================
            // VALIDACION TARIFA
            // =========================
            if (tarifa == null)
            {
                return (
                    new TarifaDetalleResponseDto
                    {
                        IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_ERROR_MSG,
                        Resultado = "Por el momento no contamos con tarifa para estos lugares"
                    },
                    "",
                    ""
                );
            }

            // =========================
            // ASIGNACION BASE
            // =========================
            beanTarifa.Abono = 0;
            beanTarifa.Monto = tarifa.Monto;
            beanTarifa.TarifaBase = tarifa.Monto;
            beanTarifa.TotalTarifa = tarifa.Monto;
            beanTarifa.PagoAdelantado = false;
            beanTarifa.TotalServicioDolares = tarifa.TotalServicioDolares;
            beanTarifa.IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_OK_NOMSG;

            // =========================
            // DESCRIPCION ZONAS (PARALELO)
            // =========================
            var zonaOrigenDescTask = _zonaRepository.RecuperaById(zonaOrigen, cancellationToken);
            var zonaDestinoDescTask = _zonaRepository.RecuperaById(zonaDestino, cancellationToken);

            await Task.WhenAll(zonaOrigenDescTask, zonaDestinoDescTask);

            var zonaOrigenO = await zonaOrigenDescTask;
            var zonaDestinoO = await zonaDestinoDescTask;

            zonaOrigenDescripcion = zonaOrigenO?.Descripcion ?? string.Empty;
            zonaDestinoDescripcion = zonaDestinoO?.Descripcion ?? string.Empty;

            beanTarifa.ZonaOrigenId = zonaOrigen;
            beanTarifa.ZonaDestinoId = zonaDestino;

            _logger.LogInformation("SolicitarFormulaXZona beanTarifa ...");

            return (beanTarifa, zonaOrigenDescripcion, zonaDestinoDescripcion);
        }

        private async Task<TarifaDetalleResponseDto> SolicitarPrecio(TarifaDestinoRequestDto beanServicio, CancellationToken cancellationToken)
        {
            // paralelo (mejora real)
            var zonaOrigenTask = _zonaRepository.RecuperaByPosicion(
                beanServicio.IdEmpresa,
                beanServicio.OrigenLatitud,
                beanServicio.OrigenLongitud,
                cancellationToken);

            var zonaDestinoTask = _zonaRepository.RecuperaByPosicion(
                beanServicio.IdEmpresa,
                beanServicio.DestinoLatitud,
                beanServicio.DestinoLongitud,
                cancellationToken);

            await Task.WhenAll(zonaOrigenTask, zonaDestinoTask);

            var zonaOrigen = await zonaOrigenTask;
            var zonaDestino = await zonaDestinoTask;

            var tarifa = await _tarifaRepository.RecuperaById(
                beanServicio.IdEmpresa,
                beanServicio.IdCliente,
                beanServicio.TipoServicio,
                beanServicio.IdTipoPago,
                zonaOrigen,
                zonaDestino,
                cancellationToken);

            // simplificado
            if (tarifa == null)
            {
                return new TarifaDetalleResponseDto
                {
                    IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_ERROR_MSG,
                    Resultado = "Por el momento no contamos con tarifa para estos lugares"
                };
            }

            return new TarifaDetalleResponseDto
            {
                Abono = tarifa.Abono,
                Monto = tarifa.Monto,
                TarifaBase = tarifa.Monto,
                PagoAdelantado = tarifa.PagoAdelantado,
                IdResultado = BeanConfiguracion.HTTP_RESPONSE.HTTP_ERROR_NOMSG
            };
        }

        private async Task<TarifaDetalleResponseDto> CalcularIncremento2(TarifaDetalleResponseDto tarifa, BeanHoraPuntaResponseDto HoraPunta, decimal totalTarifaBase = 0, int idZona = 0, CancellationToken cancellationToken = default)
        {
            tarifa.TipoIncremento = HoraPunta.TipoCalculo;

            // =========================
            // CASO DIRECTO
            // =========================
            if (HoraPunta.TipoCalculo == 1)
            {
                var incremento = HoraPunta.Incremento;

                tarifa.RecargoHorario = incremento;
                tarifa.TotalTarifa += incremento;
                tarifa.TieneRecargo = true;
                tarifa.ValorRecargoIncremento = incremento;

                tarifa.MontoSinDescuento += incremento;
                tarifa.Monto += incremento;

                tarifa.MontoSinDescuento = tarifa.Monto;
                return tarifa;
            }

            // =========================
            // PARAMETROS (UNA SOLA VEZ)
            // =========================
            var value = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_DECIMALES", cancellationToken);
            int cantidadDecimales = int.TryParse(value, out var result) ? result : 2;

            var tipoRedondeo = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken);

            tarifa.PorcentajeRecargo = HoraPunta.Incremento.ToString("0") + "%";

            // =========================
            // CALCULO BASE
            // =========================
            decimal montoBO = tarifa.Monto + totalTarifaBase;
            decimal baseCalculo = (montoBO * (HoraPunta.Incremento / 100)) * cantidadDecimales;

            decimal valorRecargo = tipoRedondeo switch
            {
                "ROUND" => Math.Round(baseCalculo),
                "FLOOR" => Math.Floor(baseCalculo),
                "CEILING" => Math.Ceiling(baseCalculo),
                "NORMAL" => Math.Floor(baseCalculo),
                _ => Math.Floor(baseCalculo)
            } / cantidadDecimales;

            // =========================
            // REGLA ESPECIAL
            // =========================
            if (idZona == 35)
                valorRecargo = 0;

            // =========================
            // ASIGNACION FINAL
            // =========================
            tarifa.RecargoHorario = valorRecargo;
            tarifa.TotalTarifa += valorRecargo;
            tarifa.TieneRecargo = true;
            tarifa.ValorRecargoIncremento = valorRecargo;

            tarifa.MontoSinDescuento += valorRecargo;
            tarifa.Monto += valorRecargo;

            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }

        private async Task<TarifaDetalleResponseDto> CalcularIncremento(TarifaDetalleResponseDto tarifa, BeanHoraPuntaResponseDto HoraPunta, decimal totalTarifaBase = 0, CancellationToken cancellationToken = default)
        {
            tarifa.TipoIncremento = HoraPunta.TipoCalculo;

            // =========================
            // CASO DIRECTO
            // =========================
            if (HoraPunta.TipoCalculo == 1)
            {
                var incremento = HoraPunta.Incremento;

                tarifa.RecargoHorario = incremento;
                tarifa.TotalTarifa += incremento;
                tarifa.TieneRecargo = true;
                tarifa.ValorRecargoIncremento = incremento;

                tarifa.MontoSinDescuento += incremento;
                tarifa.Monto += incremento;

                tarifa.PorcentajeRecargo = "";
                tarifa.MontoSinDescuento = tarifa.Monto;

                return tarifa;
            }

            // =========================
            // PARAMETROS (UNA SOLA VEZ)
            // =========================
            var value = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_DECIMALES", cancellationToken);
            int cantidadDecimales = int.TryParse(value, out var result) ? result : 2;

            var tipoRedondeo = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken);

            tarifa.PorcentajeRecargo = HoraPunta.Incremento.ToString("0") + "%";

            // =========================
            // CALCULO BASE
            // =========================
            decimal montoBO = tarifa.Monto + totalTarifaBase;
            decimal baseCalculo = (montoBO * (HoraPunta.Incremento / 100)) * cantidadDecimales;

            decimal valorRecargo = tipoRedondeo switch
            {
                "ROUND" => Math.Round(baseCalculo),
                "FLOOR" => Math.Floor(baseCalculo),
                "CEILING" => Math.Ceiling(baseCalculo),
                "NORMAL" => Math.Floor(baseCalculo),
                _ => Math.Floor(baseCalculo)
            } / cantidadDecimales;

            // =========================
            // ASIGNACION FINAL
            // =========================
            tarifa.RecargoHorario = valorRecargo;
            tarifa.TotalTarifa += valorRecargo;
            tarifa.TieneRecargo = true;
            tarifa.ValorRecargoIncremento = valorRecargo;

            tarifa.MontoSinDescuento += valorRecargo;
            tarifa.Monto += valorRecargo;

            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }

    }
}
