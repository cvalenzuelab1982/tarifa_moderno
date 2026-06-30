namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class BeanCoberturaResponseDto
    {
        public string ISOCountryCode { get; set; }
        public string? descripcion { get; set; }
        public bool activo { get; set; }
        public string? codigoPais { get; set; }
        public string? idTimeZone { get; set; }
    }
}
