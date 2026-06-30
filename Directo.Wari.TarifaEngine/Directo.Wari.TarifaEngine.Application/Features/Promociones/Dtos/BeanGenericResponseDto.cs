namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos
{
    public class BeanGenericResponseDto
    {
        public int IdResultado { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public Dictionary<string, string> Values { get; set; } = new();
        public List<object> List { get; set; } = new();

        public string NXVCodigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
    }
}
