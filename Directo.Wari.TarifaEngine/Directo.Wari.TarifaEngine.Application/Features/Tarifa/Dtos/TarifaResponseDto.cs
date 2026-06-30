using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using System.Text.Json.Serialization;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaResponseDto
    {
        public decimal TotalTarifa { get; set; }
        public decimal TotalIncrementoTarifa { get; set; }
        public decimal AcumDistancia { get; set; }
        public int? IdResultado { get; set; }
        public string? Resultado { get; set; }
        public bool? IsAirportOrigin { get; set; }                                      //No se usa en esta logica
        public decimal? Abono { get; set; }                                             //No se usa en esta logica
        public bool? PagoAdelantado { get; set; }                                       //No se usa en esta logica
        public decimal? Monto { get; set; }                                             //No se usa en esta logica
        public string? Distancia { get; set; }                                          //No se usa en esta logica

        [JsonPropertyName("Kilometros")]
        public decimal Kilometros { get; set; }

        public string? OverviewPolyline { get; set; }                                   //No se usa en esta logica
        public decimal? Descuento { get; set; }                                         //No se usa en esta logica
        public decimal? MontoSinDescuento { get; set; }
        public bool? IsAirport { get; set; }

        [JsonPropertyName("IsoCountryCode")]
        public string IsoCountryCode { get; set; } = string.Empty;
        public List<TarifaDetalleResponseDto> LstTarifa { get; set; } = new();
        public List<BeanPromocionAppResponseDto> LstPromociones { get; set; } = new();  //No se usa en esta logica
        public int? IdPromoActivacion { get; set; }

        [JsonPropertyName("ISOCountryCodeAlt")]
        public string ISOCountryCodeAlt { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;
        public string? MsjTarifa { get; set; }                                          //No se usa en esta logica
        public bool OrigenPeligro { get; set; }
        public string? MensajeOrigenPeligro { get; set; }                               //No se usa en esta logica
        public int Compania { get; set; }
        public decimal RecargoHorario { get; set; }
        public decimal ValorRecargoIncremento { get; set; }
        public decimal TarifaBase { get; set; }
        public int TipoIncremento { get; set; }
        public bool TieneRecargo { get; set; }
        public decimal TiempoViaje { get; set; }
        public RecargoReservaResponseDto? RecargoReserva { get; set; }
        public decimal TotalServicioDolares { get; set; }
        public List<PeajeTarifaResponseDto> LstPeaje { get; set; } = new();
        public List<PeajeSistemaRespondeDto> LstPeajeSistema { get; set; } = new();







    }
}
