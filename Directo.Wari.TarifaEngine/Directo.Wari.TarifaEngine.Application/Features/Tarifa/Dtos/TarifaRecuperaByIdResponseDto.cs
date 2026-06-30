namespace Directo.Wari.TarifaEngine.Application.Features.Tarifa.Dtos
{
    public class TarifaRecuperaByIdResponseDto
    {
        public decimal Abono;
        public bool PagoAdelantado;
        public int Empresa { get; set; }
        public int Origen { get; set; }
        public int Destino { get; set; }
        public decimal Monto { get; set; }
        public byte I001_TipoVehiculo { get; set; }
    }
}
