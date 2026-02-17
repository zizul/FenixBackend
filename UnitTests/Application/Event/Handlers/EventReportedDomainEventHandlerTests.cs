using Application.Common;
using Application.Services.Event.DomainEvents;
using Application.Services.Event.Messages;
using Domain.Entities.Event.DomainEvents;
using NSubstitute;

namespace Application.Services.Event.Handlers
{
    public class EventReportedDomainEventHandlerTests
    {
        private readonly IMessagePublisher messagePublisher;


        public EventReportedDomainEventHandlerTests()
        {
            messagePublisher = Substitute.For<IMessagePublisher>();
        }

        [Fact]
        public async Task Handle_Should_PublishSearchCommand()
        {
            var handler = new EventReportedDomainEventHandler(messagePublisher);
            var domainEvent = new EventReportedDomainEvent("123");
            var notification = new DomainEventNotification<EventReportedDomainEvent>(domainEvent);

            await handler.Handle(notification, default);

            await messagePublisher.Received()
                .PublishAsync(
                    Arg.Is<SearchRespondersCommand>(cmd => cmd.EventId == domainEvent.EventId),
                    Arg.Any<CancellationToken>());
        }
    }
}
