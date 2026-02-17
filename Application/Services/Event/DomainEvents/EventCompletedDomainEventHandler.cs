using Domain.Entities.Event.DomainEvents;
using Application.Common;

namespace Application.Services.Event.DomainEvents
{
    /// <summary>
    /// Handles event completion/cancellation.
    /// With RabbitMQ, search jobs are self-terminating — the consumer checks event status
    /// and stops re-publishing when the event is no longer pending.
    /// This handler serves as an extension point for future side effects
    /// (e.g., analytics, audit logging, cleanup).
    /// </summary>
    public sealed class EventCompletedDomainEventHandler : IDomainEventHandler<EventCompletedDomainEvent>
    {
        public Task Handle(DomainEventNotification<EventCompletedDomainEvent> notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
