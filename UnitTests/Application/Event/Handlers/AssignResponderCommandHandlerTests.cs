using Application.Services.Event.Commands;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.User.Contracts;
using Domain.Entities.Event;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;

namespace Application.Services.Event.Handlers
{
    public class AssignResponderCommandHandlerTests
    {
        private readonly IReportedEventsRepository repositoryMock;
        private readonly IDeviceRepository deviceRepositoryMock;


        public AssignResponderCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IReportedEventsRepository>();
            deviceRepositoryMock = Substitute.For<IDeviceRepository>();
        }

        [Fact]
        public async Task Handle_Should_UpdateStatus()
        {
            var reportedEvent = GetReportedEvent("123", "0");
            SetupRepository(reportedEvent);
            var handler = new AssignResponderCommandHandler(repositoryMock, deviceRepositoryMock);
            var command = new AssignResponderCommandDto("123", "1");

            await handler.Handle(command, default);

            await repositoryMock.Received()
                .Update(
                    Arg.Is<string>(id => id == "123"), 
                    Arg.Any<Action<ReportedEvent>>());
        }

        private void SetupRepository(ReportedEvent reportedEvent)
        {
            repositoryMock.Update(Arg.Is(reportedEvent.Id), Arg.Any<Action<ReportedEvent>>())
                .Returns(reportedEvent)
                .AndDoes(x => x.Arg<Action<ReportedEvent>>().Invoke(reportedEvent));
        }
        
        private ReportedEvent GetReportedEvent(string eventId, string reporterId)
        {
            return new ReportedEvent()
            {
                Id = eventId,
                Coordinates = new Coordinates(2.5, 2.5),
                Description = "test description",
                Status = EventStatusType.Pending,
                Reporter = new Reporter() { UserId = reporterId },
                Responders = new List<Responder>()
            };
        }
    }
}