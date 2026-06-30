namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos
{
    public class BeanPeajeNewResponseDto
    {
        public int IdPeaje { get; set; }
        public decimal MontoPeaje { get; set; }
        public string NombrePeaje { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
