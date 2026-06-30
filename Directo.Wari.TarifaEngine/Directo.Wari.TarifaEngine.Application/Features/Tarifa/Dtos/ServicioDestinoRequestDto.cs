namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class ServicioDestinoRequestDto
    {
        public double DestinoLatitud { get; set; }
        public double DestinoLongitud { get; set; }
        public double OrigenLatitud { get; set; }
        public double OrigenLongitud { get; set; }
        public int IdTipoPago { get; set; }
        public decimal Tarifa { get; set; }
    }
}
