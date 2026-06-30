namespace Directo.Wari.TarifaEngine.Application.Common.Options
{
    public class ConfiguracionGenericasOptions
    {
        public string COUNTRY_CODE { get; set; } = string.Empty;
        public string CURRENCY { get; set; } = string.Empty;
        public decimal CANTIDAD_POR_METRO { get; set; }
        public string UNIDAD_DISTANCIA { get; set; } = string.Empty;
        public decimal COSTO_POR_DISTANCIA { get; set; }
        public decimal CONSTANTE_COBRO {  get; set; }
        public decimal COSTO_POR_MINUTO { get; set; }
        public string TYPE_ROUND {  get; set; } = string.Empty; 
        public string ENABLE_MIN_COST {  get; set; } = string.Empty;   
        public decimal MIN_COST {  get; set; }
    }
}
 