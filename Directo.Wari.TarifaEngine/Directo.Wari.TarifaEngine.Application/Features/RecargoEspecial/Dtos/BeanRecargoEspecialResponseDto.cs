namespace Directo.Wari.TarifaEngine.Application.Features.RecargoEspecial.Dtos
{
    public class BeanRecargoEspecialResponseDto
    {
        public int? idRecargoEspecial { get; set; }
        public int? idEmpresa { get; set; }
        public decimal ValorRecargo { get; set; }
        public bool? Activo { get; set; }
        public int tipoRecargo { get; set; }
        public string? tipoRedondeo { get; set; }
        public int cantidadDecimal { get; set; }
        public int? idTipoPago { get; set; }
    }
}
