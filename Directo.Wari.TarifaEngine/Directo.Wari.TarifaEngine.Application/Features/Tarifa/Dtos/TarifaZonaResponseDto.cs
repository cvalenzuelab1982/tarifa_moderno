namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaZonaResponseDto
    {
        public int IdTRZona { set; get; }
        public int IdZona { set; get; }
        public int IdEmpresa { set; get; }
        public DateTime HoraActual { get; set; }
        public TimeSpan HoraInicio { set; get; }
        public TimeSpan HoraFin { set; get; }
        public int Minutos { set; get; }
        public bool Activo { get; set; }
    }
}
