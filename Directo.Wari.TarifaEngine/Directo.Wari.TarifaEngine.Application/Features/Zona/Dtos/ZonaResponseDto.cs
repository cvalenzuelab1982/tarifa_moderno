namespace Directo.Wari.TarifaEngine.Application.Features.Zona.Dtos
{
    public class ZonaResponseDto
    {
        public SqlGeographyResponseDto? Centro { get; set; }
        public SqlGeographyResponseDto? Geocerca { get; set; }
        public int I025_TipoZona { get; set; }
        public int I014_Zonificacion { get; set; }
        public int IdUbigeo { get; set; }
        public string? Habilitado { get; }
        public string? Abreviatura { get; set; }
        public string? Descripcion { get; set; }
        public int IdZona { get; set; }
        public string? Distrito { get; set; }
        public int IdEmpresa { get; set; }
        public bool Activar { get; set; }
    }
}
