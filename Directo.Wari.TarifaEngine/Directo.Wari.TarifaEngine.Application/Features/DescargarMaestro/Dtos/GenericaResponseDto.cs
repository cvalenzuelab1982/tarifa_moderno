namespace Directo.Wari.TarifaEngine.Application.Features.DescargarMaestro.Dtos
{
    public class GenericaResponseDto
    {
        public int? IDENT_CAMPO { get; set; }
        public string? CODL_CAMPO { get; set; }
        public string? CODC_CAMPO { get; set; }
        public string? DESCM_CAMPO { get; set; }
        public int? CANT_CAMPO { get; set; }
        public int? IDENT_TABLA { get; set; }
        public int CODI_ORDEN { get; set; }
        public bool? Activo { get; set; }
        public string? MODIFICACION_USUARIO { get; set; }
        public string? CREACION_USUARIO { get; set; }
        public string? Habilitado { get; }
    }
}
