using Directo.Wari.TarifaEngine.Application.Common.Models;
using Directo.Wari.TarifaEngine.Application.Common.Options;
using Directo.Wari.TarifaEngine.Application.Common.Util;
using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Services.TarifaCalculatorService;
using Directo.Wari.TarifaEngine.Application.Features.Zona.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Queries.SolicitarPrecios
{
    public class SolicitarPreciosHandler : IRequestHandler<SolicitarPreciosQuery, Result<TarifaResponseDto>>
    {
        private readonly ITarifaCalculatorService _tarifaCalculatorService;
        private readonly IZonaRepository _zonaRepository;
        private readonly IRecargoEspecialRepository _recargoEspecialRepository;
        private readonly IParametrosRepository _parametrosRepository;
        private readonly ITarifaRepository _tarifaRepository;
        private readonly ConfiguracionGenericasOptions _config;

        public SolicitarPreciosHandler(IOptions<ConfiguracionGenericasOptions> options, ITarifaCalculatorService tarifaCalculatorService, IZonaRepository zonaRepository, IRecargoEspecialRepository recargoEspecialRepository, IParametrosRepository parametrosRepository, ITarifaRepository tarifaRepository)
        {
            _tarifaCalculatorService = tarifaCalculatorService;
            _zonaRepository = zonaRepository;
            _recargoEspecialRepository = recargoEspecialRepository;
            _parametrosRepository = parametrosRepository;
            _tarifaRepository = tarifaRepository;
            _config = options.Value;
        }

        public async Task<Result<TarifaResponseDto>> Handle(SolicitarPreciosQuery query, CancellationToken cancellationToken)
        {
            var request = query.request;

            if (request.LstDestinos.Count == 0)
                return Result.Failure<TarifaResponseDto>(Error.Validation("No hay destinos para el calculo"));

            if (request.LstDestinos[0].TipoServicio == 0)
                return Result.Failure<TarifaResponseDto>(Error.Validation("Por favor Seleccione un Tipo de Servicio"));

            var tarifa = new TarifaResponseDto
            {
                IdResultado = 1,
                LstTarifa = new List<TarifaDetalleResponseDto>()
            };

            int cantidadDestino = request.LstDestinos.Count;

            // =========================
            // VALIDAR AEROPUERTO (OPTIMIZADO O(n))
            // =========================
            bool isAeropuerto = false;

            foreach (var d in request.LstDestinos)
            {
                var idOrigen = await _zonaRepository.ObtenerIdZona(d.OrigenLatitud, d.OrigenLongitud, cancellationToken);
                var idDestino = await _zonaRepository.ObtenerIdZona(d.DestinoLatitud, d.DestinoLongitud, cancellationToken);

                if (idOrigen == 0 || idDestino == 0)
                {
                    return Result.Success(new TarifaResponseDto { IdResultado = -1, Resultado = "Por el momento esta zona esta fuera de nuestra cobertura.(Destino)" });
                }

                if (idOrigen == 35 || idDestino == 35)
                {
                    isAeropuerto = true;
                }

                break;

            }

            // =========================
            // PARAMETROS (UNA SOLA VEZ)
            // =========================
            var envioTarifaBase = await _parametrosRepository.GetParameterValue("SERVICIO_ENVIO_TARIFA_BASE", cancellationToken);
            var flagTotalIncrementoServicio = await _parametrosRepository.GetParameterValue("TOTAL_INCREMENTO_SERVICIO", cancellationToken);

            BeanRecargoEspecialResponseDto? recargoEspecial = null;

            for (int i = 0; i < cantidadDestino; i++)
            {
                var destino = request.LstDestinos[i];

                destino.CantidadDestino = cantidadDestino;
                destino.PosicionDestino = i;
                destino.IsOrigenDestinoAeropuerto = isAeropuerto;
                destino.DtFechaServicio = request.DtFechaServicio;
                destino.ModoReserva = request.ModoReserva;
                destino.Anticipadoalmomento = request.AnticipadoAlMomento;
                destino.IsPeaje = request.IsPeaje;

                var result = await _tarifaCalculatorService.Calcular(destino, cancellationToken);
                if (!result.IsSuccess) return Result.Failure<TarifaResponseDto>(result.Error!);

                var tarAux = result.Value;
                if (tarAux is null)
                {
                    return Result.Failure<TarifaResponseDto>(Error.Validation("MD Tarifa: No se obtuvo el Calcula de la tarifas", -1));
                }

                // =========================
                // RECARGO ESPECIAL (UNA SOLA VEZ)
                // =========================
                if (recargoEspecial == null)
                {

                    recargoEspecial = await _recargoEspecialRepository
                        .GetRecargoEspecialTipoServicioTipoPago(
                            destino.IdEmpresa,
                            destino.IdTipoPago,
                            destino.TipoServicio,
                            DateTimeHelper.ParseExact(destino.DtFechaServicio),
                            cancellationToken);
                }

                if (recargoEspecial != null && tarAux.TieneRecargo)
                {
                    tarAux.TotalTarifa -= tarAux.RecargoHorario;
                    tarAux.MontoSinDescuento -= tarAux.RecargoHorario;
                    tarAux.Monto -= tarAux.RecargoHorario;
                    tarAux.TieneRecargo = false;
                    tarAux.ValorRecargoIncremento = 0;
                    tarAux.RecargoHorario = 0;
                }

                if (tarAux.IdResultado < 0)
                {
                    tarifa.IdResultado = -1;
                    tarifa.Resultado = tarAux.Resultado;
                }

                // =========================
                // ACUMULADORES
                // =========================
                tarifa.TotalTarifa += tarAux.Monto;
                tarifa.TotalServicioDolares += tarAux.TotalServicioDolares;

                var km = tarAux.Distancia != null
                    ? Convert.ToDecimal(tarAux.Distancia.Split(' ')[0])
                    : 0;

                tarifa.AcumDistancia += km;
                tarifa.Kilometros = tarifa.AcumDistancia;
                tarAux.Kilometros = tarifa.Kilometros;

                tarifa.ISOCountryCodeAlt = tarifa.IsoCountryCode = tarAux.ISOCountryCodeAlt;
                tarifa.Compania = tarAux.Compania;
                tarifa.TieneRecargo = tarAux.TieneRecargo;
                tarifa.TipoIncremento += tarAux.TipoIncremento;

                tarifa.TarifaBase += (!string.IsNullOrEmpty(envioTarifaBase) && envioTarifaBase == "1")
                    ? tarAux.TarifaBase
                    : tarAux.Monto;

                if (i == 0)
                    tarifa.RecargoReserva = tarAux.RecargoReserva;

                tarifa.ValorRecargoIncremento += tarAux.ValorRecargoIncremento;
                tarifa.RecargoHorario += tarAux.RecargoHorario;
                tarifa.TiempoViaje += tarAux.TiempoViaje;

                tarifa.LstTarifa.Add(tarAux);
                tarifa.MsjTarifa = tarAux.MsjTarifa;
                tarifa.OrigenPeligro = tarAux.OrigenPeligro;

                // =========================
                // LOGICA MULTIDESTINO
                // =========================
                if (cantidadDestino > 1 && tarAux.RecargoReserva != null)
                {
                    tarifa.TotalTarifa -= tarAux.RecargoReserva.RecargoHorario;

                    if (i == cantidadDestino - 1)
                        tarifa.TotalTarifa += tarAux.RecargoReserva.RecargoHorario;
                }

                tarifa.LstPeaje = tarAux.LstPeaje ?? new List<PeajeTarifaResponseDto>();
                tarifa.LstPeajeSistema = tarAux.LstPeajeSistema ?? new List<PeajeSistemaRespondeDto>();
            }

            // =========================
            // RECARGO FINAL
            // =========================
            if (recargoEspecial != null)
            {
                tarifa = CalcularIncrementoDirecto(tarifa, recargoEspecial);
            }

            tarifa.IdPromoActivacion = request.IdPromoActivacion;

            // =========================
            // COBERTURA
            // =========================
            var cobertura = await _tarifaRepository.RecuperarCobertura(
                request.LstDestinos[0].OrigenLatitud,
                request.LstDestinos[0].OrigenLongitud,
                cancellationToken);

            if (cobertura == null)
                return Result.Failure<TarifaResponseDto>(Error.Validation($"No se obtuvo la cobertura, con las cordenadas OrigenLatitud:{request.LstDestinos[0].OrigenLatitud}, OrigenLongitud:{request.LstDestinos[0].OrigenLongitud}"));

            tarifa.CurrencySymbol = _config.CURRENCY;
            tarifa.ISOCountryCodeAlt = cobertura.ISOCountryCode;

            // =========================
            // FLAG AEROPUERTO
            // =========================
            tarifa.IsAirport = tarifa.LstTarifa.Any(x => x.IsAirport);

            // =========================
            // MENSAJE ERROR LEGACY
            // =========================
            if (tarifa.LstTarifa.Count > 0 &&
                tarifa.LstTarifa[0].Resultado.Contains("The process cannot access the file"))
            {
                tarifa.Resultado = "Ocurrio un problema al consultar la tarifa, por favor intente nuevamente.";
            }

            // =========================
            // INCREMENTO EMPRESA
            // =========================
            if (!string.IsNullOrEmpty(flagTotalIncrementoServicio) && flagTotalIncrementoServicio == "1")
            {
                var resultado = await _tarifaRepository.ObtenerMontoIncrementoEmpresa(
                    request.LstDestinos[0].IdEmpresa,
                    cancellationToken);

                if (resultado != null)
                {
                    tarifa.TotalIncrementoTarifa = resultado.TryGetValue("TOTAL_INCREMENTO_EMPRESA", out var incEmp) ? Convert.ToDecimal(incEmp) : 0;
                    tarifa.TotalTarifa +=  resultado.TryGetValue("TOTAL_INCREMENTO", out var incTotal) ? Convert.ToDecimal(incTotal) : 0;
                }
            }

            return Result.Success(tarifa);
        }

        private TarifaResponseDto CalcularIncrementoDirecto(TarifaResponseDto tarifa, BeanRecargoEspecialResponseDto bean)
        {
            tarifa.TipoIncremento = bean.tipoRecargo;

            decimal valorRecargo;

            if (bean.tipoRecargo == 1)
            {
                valorRecargo = bean.ValorRecargo;
            }
            else
            {
                var baseCalculo = (tarifa.TarifaBase * (bean.ValorRecargo / 100)) * bean.cantidadDecimal;

                valorRecargo = bean.tipoRedondeo switch
                {
                    "ROUND" => Math.Round(baseCalculo),
                    "FLOOR" => Math.Floor(baseCalculo),
                    "CEILING" => Math.Ceiling(baseCalculo),
                    "TRUNCATE" => Math.Truncate(baseCalculo),
                    _ => Math.Floor(baseCalculo) // NORMAL y default
                } / bean.cantidadDecimal;
            }

            // =========================
            // ASIGNACIONES (UNA SOLA VEZ)
            // =========================
            tarifa.RecargoHorario = valorRecargo;
            tarifa.ValorRecargoIncremento = valorRecargo;
            tarifa.TotalTarifa = tarifa.TarifaBase + valorRecargo;
            tarifa.TieneRecargo = true;

            tarifa.Monto += valorRecargo;
            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }

       
    }
}
