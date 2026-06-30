namespace Directo.Wari.TarifaEngine.Application.Features.Promociones.Dtos
{
    public class BeanPromocionAppResponseDto
    {
        public int IdPromoActivacion { get; set; }
        public int IdPromocion { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaActivado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int ModalidadPromocion { get; set; }
        public decimal ValorPromocion { get; set; }

        public string UrlPromocion { get; set; } = string.Empty;
        public string UrlImagen { get; set; } = string.Empty;

        public int Cantidad { get; set; }
        public int Consumido { get; set; }
        public int Disponible { get; set; }

        public bool IsAgotarValor { get; set; }
        public bool IsClienteNuevo { get; set; }
        public decimal ValorConsumido { get; set; }

        public int IsPrecargado { get; set; }
        public int I057_ModalidadPromocion { get; set; }

        public string InfoAgotarValor =>
            IsAgotarValor
                ? "Consume el monto total del descuento a tu manera antes de la fecha límite."
                : "El descuento se aplicará automáticamente en tu próximo viaje.";

    }
}
