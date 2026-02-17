using Application.Common;
using Application.Services.Event.DomainEvents;
using Domain.Entities.Event.DomainEvents;
using Domain.Enums;

namespace Application.Services.Event.Handlers
{
    public class EventCompletedDomainEventHandlerTests
    {
        [Fact]
        public async Task Handle_Should_CompleteWithoutError()
        {
            var handler = new EventClosedDomainEventHandler();
            var domainEvent = new EventClosedDomainEvent("123", EventStatusType.Completed);
            var notification = new DomainEventNotification<EventClosedDomainEvent>(domainEvent);

            await handler.Handle(notification, default);
        }
    }
}
