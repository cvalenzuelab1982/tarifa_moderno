using Directo.Wari.TarifaEngine.Application.Features.Parametros.Interfaces;
using Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos;
using Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Services
{
    public class RecargoEspecialService : IRecargoEspecialService
    {
        private readonly IParametrosRepository _parametrosRepository;

        public RecargoEspecialService(IParametrosRepository parametrosRepository)
        {
            _parametrosRepository = parametrosRepository;
        }

        public async Task<TarifaDetalleResponseDto> CalcularIncremento(TarifaDetalleResponseDto tarifa, BeanRecargoEspecialResponseDto beanRecargoEspecial, decimal totalTarifaBase = 0, bool incrementoMax = false, CancellationToken cancellationToken = default)
        {
            tarifa.TipoIncremento = beanRecargoEspecial.tipoRecargo;

            // =========================
            // CASO DIRECTO
            // =========================
            if (beanRecargoEspecial.tipoRecargo == 1)
            {
                var incremento = beanRecargoEspecial.ValorRecargo;

                tarifa.RecargoHorario = incremento;
                tarifa.TotalTarifa += incremento;
                tarifa.TieneRecargo = true;
                tarifa.ValorRecargoIncremento = 0;

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

            tarifa.PorcentajeRecargo = beanRecargoEspecial.ValorRecargo.ToString("0") + "%";

            // =========================
            // CALCULO BASE
            // =========================
            decimal montoBO = tarifa.Monto + totalTarifaBase;
            decimal baseCalculo = (montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidadDecimales;

            decimal valorRecargo = tipoRedondeo switch
            {
                "ROUND" => Math.Round(baseCalculo),
                "FLOOR" => Math.Floor(baseCalculo),
                "CEILING" => Math.Ceiling(baseCalculo),
                "NORMAL" => Math.Floor(baseCalculo),
                _ => Math.Floor(baseCalculo)
            } / cantidadDecimales;

            // =========================
            // REGLA MAXIMA
            // =========================
            if (incrementoMax && valorRecargo > 20)
                valorRecargo = 20;

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

        //TODO: CODIGO LEGACY PENDIENTE POR ELIMINAR
        public async Task<TarifaDetalleResponseDto> CalcularIncremento_legacy(TarifaDetalleResponseDto tarifa, BeanRecargoEspecialResponseDto beanRecargoEspecial, decimal totalTarifaBase = 0, bool incrementoMax = false, CancellationToken cancellationToken = default)
        {
            if (beanRecargoEspecial.tipoRecargo == 1)
            {
                tarifa.TipoIncremento = beanRecargoEspecial.tipoRecargo;
                tarifa.RecargoHorario = beanRecargoEspecial.ValorRecargo;
                tarifa.TotalTarifa += tarifa.RecargoHorario;
                tarifa.TieneRecargo = true;
                tarifa.ValorRecargoIncremento = 0;

                tarifa.MontoSinDescuento += beanRecargoEspecial.ValorRecargo;
                tarifa.Monto += beanRecargoEspecial.ValorRecargo;
            }

            else
            {
                tarifa.TipoIncremento = beanRecargoEspecial.tipoRecargo;
             
                var value = await _parametrosRepository.GetParameterValue("RECAR_HORARIO_DECIMALES", cancellationToken);
                int cantidad_decimales = int.TryParse(value, out var result) ? result : 2;


                tarifa.PorcentajeRecargo = beanRecargoEspecial.ValorRecargo.ToString("0") + "%";
                decimal valorRecargo = 0;
                decimal montoBO = tarifa.Monto + totalTarifaBase;
                if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "ROUND")
                {
                    valorRecargo = Math.Round((montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "FLOOR")
                {
                    valorRecargo = Math.Floor((montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "CEILING")
                {
                    valorRecargo = Math.Ceiling((montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else if ((await _parametrosRepository.GetParameterValue("RECAR_HORARIO_TIPO_REDONDEO", cancellationToken)) == "NORMAL")
                {
                    valorRecargo = Math.Floor((montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidad_decimales) / cantidad_decimales;
                }
                else
                {
                    valorRecargo = Math.Floor((montoBO * (beanRecargoEspecial.ValorRecargo / 100)) * cantidad_decimales) / cantidad_decimales;
                }

                if (incrementoMax && valorRecargo > 20)
                    valorRecargo = 20;
                tarifa.RecargoHorario = valorRecargo;
                tarifa.TotalTarifa += tarifa.RecargoHorario;
                tarifa.TieneRecargo = true;
                tarifa.ValorRecargoIncremento = valorRecargo;

                tarifa.MontoSinDescuento += valorRecargo;
                tarifa.Monto += valorRecargo;
            }

            tarifa.MontoSinDescuento = tarifa.Monto;

            return tarifa;
        }
    }
}
