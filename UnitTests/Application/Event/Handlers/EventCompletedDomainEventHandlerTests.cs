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
            var handler = new EventCompletedDomainEventHandler();
            var domainEvent = new EventCompletedDomainEvent("123", EventStatusType.Completed);
            var notification = new DomainEventNotification<EventCompletedDomainEvent>(domainEvent);

            await handler.Handle(notification, default);
        }
    }
}
