namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaPromocionResponseDto
    {
        public int IdPromoActivacion { get; set; }
        public int IdPromocion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string FechaActivado { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaFin { get; set; } = string.Empty;
        public int I057ModalidadPromocion { get; set; }
        public decimal ValorPromocion { get; set; }
        public string UrlPromocion { get; set; } = string.Empty;
        public string UrlImagen { get; set; } = string.Empty;
        public string CodigoPais { get; set; } = string.Empty;
        public string ISOCountryCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal Descuento { get; set; }
        public string DescripcionCorta { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public int Consumido { get; set; }

        public string Estado { get; set; } = string.Empty;
        public int Cuota { get; set; }
        public decimal TarifaInicial { get; set; }
        public decimal TarifaFinal { get; set; }
    }
}
