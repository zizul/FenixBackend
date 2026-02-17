using Application.Common;
using Application.Services.Event.Commands;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using Application.Services.User.Contracts;
using Domain.Entities;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainEvents;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;

namespace Application.Services.Event.Handlers
{
    public class UpdateResponderStatusCommandHandlerTests
    {
        private readonly IReportedEventsRepository repositoryMock;
        private readonly IDomainEventConsumer eventsConsumerMock;
        private readonly IDeviceRepository deviceRepositoryMock;
        private List<IDomainEvent> capturedEvents = new();


        public UpdateResponderStatusCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IReportedEventsRepository>();
            eventsConsumerMock = Substitute.For<IDomainEventConsumer>();
            deviceRepositoryMock = Substitute.For<IDeviceRepository>();

            // Capture events at call time — DomainEvents is a live reference that gets cleared after Consume
            eventsConsumerMock.Consume(Arg.Any<IReadOnlyList<IDomainEvent>>())
                .Returns(Task.CompletedTask)
                .AndDoes(x => capturedEvents = x.Arg<IReadOnlyList<IDomainEvent>>().ToList());
        }

        [Fact]
        public async Task Handle_Should_UpdateStatus()
        {
            var reportedEvent = GetReportedEvent("123", "0", "1");
            SetupRepository(reportedEvent);
            var handler = new UpdateResponderStatusCommandHandler(repositoryMock, eventsConsumerMock, deviceRepositoryMock);
            var command = new UpdateResponderStatusCommandDto("123", "1", new ResponderInputDto()
            {
                Status = ResponderStatusType.Completed
            });

            await handler.Handle(command, default);

            await repositoryMock.Received()
                .Update(
                    Arg.Is<string>(id => id == "123"), 
                    Arg.Any<Func<ReportedEvent, Task>>());
            Assert.Single(capturedEvents);
            Assert.Equal(EventStatusType.Completed, ((EventClosedDomainEvent)capturedEvents[0]).FinalStatus);
        }

        private void SetupRepository(ReportedEvent reportedEvent)
        {
            repositoryMock.Get(Arg.Any<string>()).Returns(reportedEvent);
            repositoryMock.Update(Arg.Is(reportedEvent.Id), Arg.Any<Func<ReportedEvent, Task>>())
                .Returns(reportedEvent)
                .AndDoes(x => x.Arg<Func<ReportedEvent, Task>>().Invoke(reportedEvent).GetAwaiter().GetResult());
        }
        
        private ReportedEvent GetReportedEvent(string eventId, string reporterId, string responderId)
        {
            return new ReportedEvent()
            {
                Id = eventId,
                Coordinates = new Coordinates(2.5, 2.5),
                Description = "test description",
                Status = EventStatusType.Accepted,
                Reporter = new Reporter() { UserId = reporterId },
                Responders = new List<Responder>() 
                { 
                    new Responder(eventId: null, identityId: responderId, status: ResponderStatusType.Arrived)
                }
            };
        }
    }
}
