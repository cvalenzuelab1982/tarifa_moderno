using Directo.Wari.TarifaEngine.Domain.Common;

namespace Directo.Wari.TarifaEngine.Domain.Aggregates
{
    public class ConfiguracionZona : AggregateRoot<int>
    {
        public int IdZona { get; private set; }
        public string Propiedad { get; private set; } = default!;
        public bool Valor { get; private set; }
    }
}
