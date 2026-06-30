using Directo.Wari.TarifaEngine.Domain.Common;
using MediatR;

namespace Directo.Wari.TarifaEngine.Application.Common.Interfaces
{
    /// <summary>
    /// Wrapper para publicar domain events a través de MediatR.
    /// Conecta el IDomainEvent del Domain (sin dependencias externas) con INotification de MediatR.
    /// </summary>
    public interface IDomainEventPublisher
    {
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
        Task PublishAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Wrapper genérico que convierte un IDomainEvent en INotification para MediatR.
    /// </summary>
    public class DomainEventNotification<TDomainEvent> : INotification
        where TDomainEvent : IDomainEvent
    {
        public TDomainEvent DomainEvent { get; }

        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
