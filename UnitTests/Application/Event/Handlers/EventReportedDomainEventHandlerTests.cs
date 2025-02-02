using Application.Common;
using Application.Services.Event.Contracts;
using Application.Services.Event.DomainEvents;
using Application.Services.Event.Worker;
using Domain.Entities.Event.DomainEvents;
using NSubstitute;

namespace Application.Services.Event.Handlers
{
    public class EventReportedDomainEventHandlerTests
    {
        private readonly IWorkerManager worker;
        private readonly IEventCoordinatorService coordinator;


        public EventReportedDomainEventHandlerTests()
        {
            worker = Substitute.For<IWorkerManager>();
            coordinator = Substitute.For<IEventCoordinatorService>();
        }

        [Fact]
        public async Task Handle_Should_CreateWorkerJob()
        {
            var handler = new EventReportedDomainEventHandler(worker, coordinator);
            var domainEvent = new EventReportedDomainEvent("123");
            var notification = new DomainEventNotification<EventReportedDomainEvent>(domainEvent);

            await handler.Handle(notification, default);

            worker.Received()
                .AddLoopJob(Arg.Is(domainEvent.Id), Arg.Any<Func<CancellationToken, Task>>());
        }
    }
}