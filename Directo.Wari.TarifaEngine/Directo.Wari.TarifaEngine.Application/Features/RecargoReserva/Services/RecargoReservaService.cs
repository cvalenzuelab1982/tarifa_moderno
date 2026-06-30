using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoReserva.Services
{
    public class RecargoReservaService : IRecargoReservaService
    {
        private readonly IParametrosRepository _parametrosRepository;

        public RecargoReservaService(IParametrosRepository parametrosRepository)
        {
            _parametrosRepository = parametrosRepository;
        }

        public async Task<TarifaDetalleResponseDto> CalcularIncremento(TarifaDetalleResponseDto tarifa, BeanRecargoReservaResponseDto HoraPunta, decimal totalTarifaBase,CancellationToken cancellationToken)
        {
            var recargoReserva = new RecargoReservaResponseDto
            {
                TipoIncremento = HoraPunta.TipoCalculo
            };

            // =========================
            // CASO DIRECTO
            // =========================
            if (HoraPunta.TipoCalculo == 1)
            {
                var incremento = HoraPunta.Incremento;

                recargoReserva.RecargoHorario = incremento;
                recargoReserva.ValorRecargoIncremento = incremento;

                tarifa.TotalTarifa += incremento;
                tarifa.TieneRecargo = true;

                tarifa.MontoSinDescuento += incremento;
                tarifa.Monto += incremento;
            }
            else
            {
                tarifa.PorcentajeRecargo = HoraPunta.Incremento.ToString("0") + "%";

                // =========================
                // PARAMETROS (UNA SOLA VEZ)
                // =========================
                var value = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_DECIMALES", cancellationToken);
                int cantidadDecimales = int.TryParse(value, out var result) ? result : 2;

                var tipoRedondeo = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken);

                // =========================
                // CALCULO BASE (OJO: usa TotalTarifa)
                // =========================
                decimal tarifaV2 = tarifa.TotalTarifa + totalTarifaBase;
                decimal baseCalculo = (tarifaV2 * (HoraPunta.Incremento / 100)) * cantidadDecimales;

                decimal valorRecargo = tipoRedondeo switch
                {
                    "ROUND" => Math.Round(baseCalculo),
                    "FLOOR" => Math.Floor(baseCalculo),
                    "CEILING" => Math.Ceiling(baseCalculo),
                    "NORMAL" => Math.Floor(baseCalculo),
                    _ => Math.Floor(baseCalculo)
                } / cantidadDecimales;

                recargoReserva.RecargoHorario = valorRecargo;
                recargoReserva.ValorRecargoIncremento = valorRecargo;

                // FIX IMPORTANTE
                tarifa.TotalTarifa += valorRecargo;

                tarifa.TieneRecargo = true;
                tarifa.MontoSinDescuento += valorRecargo;
                tarifa.Monto += valorRecargo;
            }

            tarifa.RecargoReserva = recargoReserva;
            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }

        //TODO: CODIGO LEGACY PENDIENTE POR ELIMINAR
        public async Task<TarifaDetalleResponseDto> CalcularIncremento_legacy(TarifaDetalleResponseDto tarifa, BeanRecargoReservaResponseDto HoraPunta, decimal totalTarifaBase, CancellationToken cancellationToken)
        {
            var recargoReserva = new RecargoReservaResponseDto();
            if (HoraPunta.TipoCalculo == 1)
            {
                recargoReserva.TipoIncremento = HoraPunta.TipoCalculo;
                recargoReserva.RecargoHorario = HoraPunta.Incremento;
                tarifa.TotalTarifa += tarifa.RecargoHorario;
                tarifa.TieneRecargo = true;
                recargoReserva.ValorRecargoIncremento = recargoReserva.RecargoHorario;

                tarifa.MontoSinDescuento += HoraPunta.Incremento;
                tarifa.Monto += HoraPunta.Incremento;
            }
            else
            {
                decimal tarifaV2 = tarifa.TotalTarifa + totalTarifaBase;
                recargoReserva.TipoIncremento = HoraPunta.TipoCalculo;
                tarifa.PorcentajeRecargo = HoraPunta.Incremento.ToString("0") + "%";

                var value = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_DECIMALES", cancellationToken);
                int cantidad_decimales = int.TryParse(value, out var result) ? result : 2;

                decimal valorRecargo = 0;
                if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "ROUND")
                {
                    valorRecargo = Math.Round((tarifaV2 * (HoraPunta.Incremento / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "FLOOR")
                {
                    valorRecargo = Math.Floor((tarifaV2 * (HoraPunta.Incremento / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "CEILING")
                {
                    valorRecargo = Math.Ceiling((tarifaV2 * (HoraPunta.Incremento / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "NORMAL")
                {
                    valorRecargo = Math.Floor((tarifaV2 * (HoraPunta.Incremento / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else
                {
                    valorRecargo = Math.Floor((tarifaV2 * (HoraPunta.Incremento / 100)) * cantidad_decimales) / cantidad_decimales;
                }

                recargoReserva.RecargoHorario = valorRecargo;
                tarifa.TotalTarifa += tarifa.RecargoHorario;
                tarifa.TieneRecargo = true;
                recargoReserva.ValorRecargoIncremento = valorRecargo;
                tarifa.MontoSinDescuento += valorRecargo;
                tarifa.Monto += valorRecargo;
            }

            tarifa.RecargoReserva = recargoReserva;
            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }
    }
}
