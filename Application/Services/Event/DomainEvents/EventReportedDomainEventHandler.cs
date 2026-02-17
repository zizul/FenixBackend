using Application.Common;
using Application.Services.Event.Messages;
using Domain.Entities.Event.DomainEvents;

namespace Application.Services.Event.DomainEvents
{
    /// <summary>
    /// Reacts to a reported event by publishing a search command to RabbitMQ.
    /// Unlike the previous in-memory worker approach, messages survive application restarts
    /// and benefit from RabbitMQ's retry/error queue infrastructure.
    /// </summary>
    public sealed class EventReportedDomainEventHandler : IDomainEventHandler<EventReportedDomainEvent>
    {
        private readonly IMessagePublisher messagePublisher;


        public EventReportedDomainEventHandler(IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

        public async Task Handle(DomainEventNotification<EventReportedDomainEvent> notification, CancellationToken cancellationToken)
        {
            // Publish search command to RabbitMQ — the consumer handles the responder search loop.
            // Durable queue ensures the search survives application restarts (fixes in-memory job loss).
            var command = new SearchRespondersCommand
            {
                EventId = notification.DomainEvent.Id
            };

            await messagePublisher.PublishAsync(command, cancellationToken);
        }
    }
}
