using Directo.Wari.TarifaEngine.Application.Common.Constants;
using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;

namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos
{
    public class BeanTarifaResponseDto
    {
        public int IdResultado { get; set; } = BeanConfiguracion.HTTP_RESPONSE.HTTP_OK_MSG;
        public string Resultado { get; set; } = string.Empty;

        public decimal Abono { get; set; }
        public bool PagoAdelantado { get; set; }
        public decimal? Monto { get; set; }
        public decimal Kilometros { get; set; }

        public string Distancia { get; set; } = string.Empty;
        public string OverviewPolyline { get; set; } = string.Empty;

        public List<BeanPromocionAppResponseDto> LstPromociones { get; set; } = new();

        public string MensajePromocion { get; set; } = "Usted cuenta con promociones diponibles";

        public decimal Descuento { get; set; }
        public decimal? MontoSinDescuento { get; set; }

        public bool IsAirport { get; set; }

        public string ZonaOrigen { get; set; } = string.Empty;
        public string ZonaDestino { get; set; } = string.Empty;

        public int ZonaOrigenId { get; set; }
        public int ZonaDestinoId { get; set; }

        public bool IsAirportOrigin { get; set; }
        public bool IsAirportDestino { get; set; }

        public bool IsAirportModulo { get; set; }
        public bool IsJockeyModulo { get; set; }

        public bool HideFactura { get; set; }

        public string CurrencySymbol { get; set; } = string.Empty;
        public string ISOCountryCode { get; set; } = string.Empty;

        public string FormaCalculo { get; set; } = string.Empty;

        public decimal TiempoViaje { get; set; }

        public bool DescuentoSinPromocion { get; set; }

        public string MsjTarifa { get; set; } = string.Empty;

        public bool ConductoresCercanos { get; set; }

        public bool OrigenPeligro { get; set; }
        public bool DestinoPeligro { get; set; }

        public string MensajeOrigenPeligro { get; set; } = string.Empty;

        /* Tarifa */

        public decimal TarifaBase { get; set; }
        public decimal RecargoHorario { get; set; }
        public decimal TotalTarifa { get; set; }

        public bool TieneRecargo { get; set; }

        public int? TipoIncremento { get; set; }

        public decimal? ValorRecargoIncremento { get; set; }

        public string PorcentajeRecargo { get; set; } = string.Empty;

        public decimal TotalTarifaApp { get; set; }

        /* Empresa */

        public int Compania { get; set; }

        public int TiempoMinimoReserva { get; set; }

        /* Cliente */

        public int IdCliente { get; set; }

        public double PresupuestoMensual { get; set; }
        public double PresupuestoPendiente { get; set; }

        public double ServicioMensual { get; set; }

        public int ServicioPendiente { get; set; }

        public Dictionary<string, string> Values { get; set; } = new();

        public BeanRecargoResponseDto? RecargoReserva { get; set; }

        public decimal TotalServicioDolares { get; set; }

        public int IsServicioAprobador { get; set; }

        public string NombrePromocion { get; set; } = string.Empty;

        public List<BeanTarifaResponseDto> LstTarifa { get; set; } = new();

        public List<string> LstOver { get; set; } = new();

        public List<BeanPeajeNewResponseDto> LstPeajeSistema { get; set; } = new();
    }
}
