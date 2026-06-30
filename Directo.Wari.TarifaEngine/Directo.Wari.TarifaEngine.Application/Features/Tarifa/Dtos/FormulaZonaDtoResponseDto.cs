namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class FormulaZonaDtoResponseDto
    {
        public decimal TarifaBase { get; set; }
        public decimal CostoPorKm { get; set; }
        public decimal CostoPorMinuto { get; set; }
        public decimal Constante { get; set; }

        public bool AplicaDistancia { get; set; }
        public bool AplicaTiempo { get; set; }

        public string TipoRedondeo { get; set; } = "ROUND";

        public string ZonaOrigenDescripcion { get; set; } = string.Empty;
        public string ZonaDestinoDescripcion { get; set; } = string.Empty;
    }
}
