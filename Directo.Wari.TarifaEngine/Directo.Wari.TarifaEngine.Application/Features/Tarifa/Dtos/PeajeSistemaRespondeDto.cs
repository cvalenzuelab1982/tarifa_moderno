namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class PeajeSistemaRespondeDto
    {
        public int IdPeaje { get; set; }
        public decimal Montopeaje { get; set; }
        public string Nombrepeaje { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
