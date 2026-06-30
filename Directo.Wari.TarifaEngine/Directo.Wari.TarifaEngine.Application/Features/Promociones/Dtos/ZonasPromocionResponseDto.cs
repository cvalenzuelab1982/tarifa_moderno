namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos
{
    public class ZonasPromocionResponseDto
    {
        public List<int> ZonasOrigen { get; set; } = new();
        public List<int> ZonasDestino { get; set; } = new();
    }
}
