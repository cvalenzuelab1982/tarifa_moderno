namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class DetBeanServicioRequestDto
    {
        public double? destinoLatitud { get; set; }
        public double? destinoLongitud { get; set; }
        public string? dirDestino { get; set; }
        public string? dirOrigen { get; set; }
        public int? idCliente { get; set; }
        public int? idEmpresa { get; set; }
        public int? idTipoPago { get; set; }
        public double? origenLatitud { get; set; }
        public double? origenLongitud { get; set; }
        public int? tipoServicio { get; set; }
        public int? idPromoActivacion { get; set; }
        public bool primerDestino { get; set; }
        public List<DetBeanServicioRequestDto> lstDestinos { get; set; } = new();
        public string? dtfechaServicio { get; set; }
        public int modoReserva { get; set; }
        public bool anticipada { get; set; }
        public bool anticipadoalmomento { get; set; }
        public bool multidestino { get; set; }
        public bool isOrigenDestinoAeropuerto { get; set; }
        public int posicionDestino { get; set; }
        public int cantidadDestino { get; set; }
        public bool isPeaje { get; set; }
    }
}
