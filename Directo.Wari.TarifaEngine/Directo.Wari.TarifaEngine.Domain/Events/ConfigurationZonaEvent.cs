using Directo.Wari.TarifaEngine.Domain.Common;

namespace Directo.Wari.TarifaEngine.Domain.Events
{
    public record ConfigurationZonaEvent(int id) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}
