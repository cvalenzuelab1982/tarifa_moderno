namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class Generic
    {
        public int IdResultado { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public Dictionary<string, string>? Values { get; set; }

        public List<object> List { get; set; } = new();

        public string NXVCodigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
    }
}
