using Application.Common;
using Application.Services.Event.DomainEvents;
using Application.Services.Event.Worker;
using Domain.Entities.Event.DomainEvents;
using Domain.Enums;
using NSubstitute;

namespace Application.Services.Event.Handlers
{
    public class EventCompletedDomainEventHandlerTests
    {
        private readonly IWorkerManager worker;


        public EventCompletedDomainEventHandlerTests()
        {
            worker = Substitute.For<IWorkerManager>();
        }

        [Fact]
        public async Task Handle_Should_CancelWorkerJob()
        {
            var handler = new EventCompletedDomainEventHandler(worker);
            var domainEvent = new EventCompletedDomainEvent("123", EventStatusType.Completed);
            var notification = new DomainEventNotification<EventCompletedDomainEvent>(domainEvent);

            await handler.Handle(notification, default);

            worker.Received()
                .CancelRunningJob(Arg.Is(domainEvent.Id));
        }
    }
}