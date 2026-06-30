namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class SolicitarFormulaZonaRequestDto
    {
        public double OrigenLatitud { get; set; }
        public double OrigenLongitud { get; set; }
        public double DestinoLatitud { get; set; }
        public double DestinoLongitud { get; set; }
        public int IdEmpresa { get; set; }
        public int TipoServicio { get; set; }
        public int IdTipoPago { get; set; }
        public string? DtFechaServicio { get; set; }
    }
}
