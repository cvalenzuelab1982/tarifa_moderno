namespace Directo.Wari.TarifaEngine.Application.Features.HttpApi.Dtos
{
    public class SrvDestinoResponseDto
    {
        public double destinoLatitud { get; set; }
        public double destinoLongitud { get; set; }
        public string? dirDestino { get; set; }
        public string? dirOrigen { get; set; }
        public int idCliente { get; set; }
        public int idEmpresa { get; set; }
        public int idTipoPago { get; set; }
        public int modoReserva { get; set; }
        public double origenLatitud { get; set; }
        public double origenLongitud { get; set; }
        public int tipoServicio { get; set; }
        public int idPromoActivacion { get; set; }
        public bool primerDestino { get; set; }
        public decimal tarifa { get; set; }
    }
}
