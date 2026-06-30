namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos
{
    public class ValidatePromocionRequestDto
    {
        public int ZonaOrigen { get; set; }
        public int ZonaDestino { get; set; }
        public int TipoPago { get; set; }
        public int TipoServicio { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaServicio { get; set; }
        public int IdPromoActivacion { get; set; }
        public int IdEmpresa { get; set; }
    }
}
