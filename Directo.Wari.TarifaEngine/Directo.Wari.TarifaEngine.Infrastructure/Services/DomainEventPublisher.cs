using Directo.Wari.TarifaEngine.Application.Common.Interfaces;
using Directo.Wari.TarifaEngine.Domain.Common;
using MediatR;

namespace Directo.Wari.TarifaEngine.Infrastructure.Services
{
    /// <summary>
    /// Implementación del publicador de eventos de dominio usando MediatR.
    /// </summary>
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IPublisher _publisher;

        public DomainEventPublisher(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification is not null)
            {
                await _publisher.Publish(notification, cancellationToken);
            }
        }

        public async Task PublishAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var domainEvent in domainEvents)
            {
                await PublishAsync(domainEvent, cancellationToken);
            }
        }
    }
}
