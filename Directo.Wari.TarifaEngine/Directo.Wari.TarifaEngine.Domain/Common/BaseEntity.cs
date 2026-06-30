namespace Directo.Wari.TarifaEngine.Domain.Common
{
    /// <summary>
    /// Clase base para todas las entidades del dominio.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
