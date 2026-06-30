namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class oldTarifaPeajeTarifaResponseDto
    {
        public int IdPeaje { get; set; }
        public decimal latitudPeaje { get; set; }
        public decimal longitudPeaje { get; set; }
        public decimal precioPeaje { get; set; }
        public string? NombrePeaje { get; set; }
        public decimal MontoPeaje { get; set; }
    }
}
