namespace Directo.Wari.TarifaEngine.Application.Features.HoraPunta.Dto
{
    public class BeanHoraPuntaResponseDto
    {
        public int? IdHoraPunta { get; set; }

        public int? IdEmpresa { get; set; }

        public int? IdTipoServicio { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public decimal Incremento { get; set; }

        public bool? Activo { get; set; }

        public int TipoCalculo { get; set; }
    }
}
