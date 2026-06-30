namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaDestinoRequestDto
    {
        public int TipoServicio { get; set; }
        public double OrigenLatitud { get; set; }
        public double OrigenLongitud { get; set; }
        public double DestinoLatitud { get; set; }
        public double DestinoLongitud { get; set; }
        public int CantidadDestino { get; set; }
        public int PosicionDestino { get; set; }
        public bool IsOrigenDestinoAeropuerto { get; set; }
        public string DtFechaServicio { get; set; } = string.Empty;
        public int ModoReserva { get; set; }
        public bool Anticipadoalmomento { get; set; }
        public bool IsPeaje { get; set; }
        public int IdEmpresa { get; set; }
        public int IdTipoPago { get; set; }
        public List<ServicioDetalleRequestDto> LstMultiPuntos { get; set; } = new();
        public List<ServicioDestinoRequestDto> LstDestinosBO { get; set; } = new();
        public bool Anticipada { get; set; }
        public int IdCliente { get; set; }
        public bool PrimerDestino { get; set; } = true;
        public List<ServicioDestinoRequestDto> LstDestinosLejanos { get; set; } = new();
        public int ZonaLatamOrigen { get; set; }
        public int ZonaLatamDestino { get; set; }
        public int CantPasajeros { get; set; }
        public bool SoloTarifa {  get; set; }
        public bool IsPorTiempo { get; set; }
        public bool Multidestino { get; set; }
        public int IdPromoActivacion { get; set; }
        public int IdPromocion { get; set; }
        public bool SinDestino { get; set; }

    }
}
