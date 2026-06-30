namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class BeanHoraPuntaRequestDto
    {
        public int IdHoraPunta { get; set; }
        public int IdEmpresa { get; set; }
        public int IdTipoServicio { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public decimal Incremento { get; set; }
        public bool Activo { get; set; }
        public int TipoCalculo { get; set; }
    }
}
