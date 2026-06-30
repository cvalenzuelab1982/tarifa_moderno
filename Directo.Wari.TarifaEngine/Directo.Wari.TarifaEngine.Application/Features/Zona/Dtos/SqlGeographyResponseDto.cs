namespace Directo.Wari.TarifaEngine.Application.Features.Zona.Dtos
{
    public class SqlGeographyResponseDto
    {
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public double? Z { get; set; }
        public double? M { get; set; }
        public int? SRID { get; set; }
        public bool IsNull { get; set; }
        public bool HasZ { get; set; }
        public bool HasM { get; set; }
    }
}
