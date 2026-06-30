namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaRequestDto
    {
        public bool Anticipada { get; set; }
        public bool AnticipadoAlMomento { get; set; }
        public string DtFechaServicio { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public int? IdPromoActivacion { get; set; }
        public List<TarifaDestinoRequestDto> LstDestinos { get; set; } = new();
        public int ModoReserva { get; set; }
        public bool IsPeaje { get; set; }
        public int IdEmpresa { get; set; }
        public int IdTipoPago { get; set; }


    }
}
