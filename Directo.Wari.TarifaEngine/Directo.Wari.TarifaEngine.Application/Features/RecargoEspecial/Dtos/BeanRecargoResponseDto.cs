namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos
{
    public class BeanRecargoResponseDto
    {
        public decimal RecargoHorario { get; set; }
        public int TipoIncremento { get; set; }
        public decimal ValorRecargoIncremento { get; set; }
        public string PorcentajeRecargo { get; set; } = string.Empty;
    }
}
