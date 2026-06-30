namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos
{
    public class PromoValidacionRequestDto
    {
        public double OrigenLatitud { get; set; }
        public double OrigenLongitud { get; set; }
        public double DestinoLatitud { get; set; }
        public double DestinoLongitud { get; set; }
        public int TipoPago { get; set; }
        public int TipoServicio { get; set; }
        public int IdEmpresa { get; set; }
        public int IdCliente { get; set; }
        public int IdPromocionActivacion { get; set; }
        public int IdPromocion { get; set; }
        public string? FechaServicio { get; set; }
        public decimal TotalServicio { get; set; }
    }
}
