namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class BeanPromocionResponseDto
    {
        public int idPromoActivacion { get; set; }
        public int idPromocion { get; set; }
        public string? codigo { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public string? fechaActivado { get; set; }
        public string? fechaInicio { get; set; }
        public string? fechaFin { get; set; }
        public int I057_ModalidadPromocion { get; set; }
        public decimal ValorPromocion { get; set; }
        public string? urlPromocion { get; set; }
        public string? urlImagen { get; set; }
        public string? codigoPais { get; set; }
        public string? ISOCountryCode { get; set; }
        public string? currencySymbol { get; set; }
        public decimal descuento { get; set; }
        public string? descripcionCorta { get; set; }
        public int cantidad { get; set; }
        public int consumido { get; set; }
        public string? estado { get; set; }
        public int cuota { get; set; }
        public decimal tarifaInicial { get; set; }
        public decimal tarifaFinal { get; set; }
    }
}
