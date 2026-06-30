using Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos;
using System.Text.Json.Serialization;

namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaDetalleResponseDto
    {
        public decimal TotalTarifa { get; set; }
        public decimal TotalIncrementoTarifa { get; set; }                             //No se usa en esta logica
        public decimal AcumDistancia { get; set; }                                     //No se usa en esta logica
        public int IdResultado { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public bool IsAirportOrigin { get; set; }
        public decimal Abono { get; set; }
        public bool PagoAdelantado { get; set; }
        public decimal Monto { get; set; }
        public string? Distancia { get; set; }

        [JsonPropertyName("Kilometros")]
        public decimal Kilometros { get; set; }

        public string OverviewPolyline { get; set; } = string.Empty;
        public decimal Descuento { get; set; }
        public decimal MontoSinDescuento { get; set; }
        public bool IsAirport { get; set; }

        [JsonPropertyName("IsoCountryCode")]
        public string? IsoCountryCode { get; set; }                                     //No se usa en esta logica
        public List<TarifaDetalleResponseDto>? LstTarifa { get; set; }                  //No se usa en esta logica
        public List<BeanPromocionAppResponseDto> LstPromociones { get; set; } = new();
        public int IdPromoActivacion { get; set; }                                      //No se usa en esta logica

        [JsonPropertyName("ISOCountryCodeAlt")]
        public string ISOCountryCodeAlt { get; set; } = string.Empty;

        public string CurrencySymbol { get; set; } = string.Empty;
        public string? MsjTarifa { get; set; }
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
        public List<PeajeTarifaResponseDto>? LstPeaje { get; set; }
        public List<PeajeSistemaRespondeDto>? LstPeajeSistema { get; set; }

        //TODO: ESTA PARTE DE REQUEST NO CAE EN CONDICIONES
        [JsonIgnore]
        public bool DescuentoSinPromocion { get; set; }
        [JsonIgnore]
        public string ZonaOrigen { get; set; } = string.Empty;
        [JsonIgnore]
        public string ZonaDestino { get; set; } = string.Empty;
        [JsonIgnore]
        public int ZonaOrigenId { get; set; }
        [JsonIgnore]
        public int ZonaDestinoId { get; set; }
        [JsonIgnore]
        public string FormaCalculo { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsAirportModulo { get; set; }
        [JsonIgnore]
        public bool IsJockeyModulo { get; set; }
        [JsonIgnore]
        public bool IsAirportDestino { get; set; }
        [JsonIgnore]
        public string PorcentajeRecargo { get; set; } = string.Empty;
        [JsonIgnore]
        public int TiempoMinimoReserva { get; set; }
        [JsonIgnore]
        public decimal TotalTarifaApp { get; set; }
        [JsonIgnore]
        public string NombrePromocion { get; set; } = string.Empty;
        [JsonIgnore]
        public string MensajePromocion = "Usted cuenta con promociones diponibles";
        [JsonIgnore]
        public bool DestinoPeligro { get; set; }
        [JsonIgnore]
        public Dictionary<string, string>? Values { get; set; }


    }
}
