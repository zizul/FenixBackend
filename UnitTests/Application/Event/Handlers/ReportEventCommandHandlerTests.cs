using Application.Common;
using Application.Services.Event.Commands;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using Application.Services.Map.PointsOfInterest.Mappers;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainEvents;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace Application.Services.Event.Handlers
{
    public class ReportEventCommandHandlerTests
    {
        private readonly IReportedEventsRepository repositoryMock;
        private readonly IDomainEventConsumer eventsConsumerMock;
        private readonly IMapper mapper;
        private List<IDomainEvent> capturedEvents = new();


        public ReportEventCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IReportedEventsRepository>();
            eventsConsumerMock = Substitute.For<IDomainEventConsumer>();
            mapper = MapperUtils.CreateMapper<ReportedEventMappingProfile>();

            // Capture events at call time — DomainEvents is a live reference that gets cleared after Consume
            eventsConsumerMock.Consume(Arg.Any<IReadOnlyList<IDomainEvent>>())
                .Returns(Task.CompletedTask)
                .AndDoes(x => capturedEvents = x.Arg<IReadOnlyList<IDomainEvent>>().ToList());
        }

        [Fact]
        public async Task Handle_Should_ReportEvent()
        {
            var newEvent = GetNewReportedEvent("0");
            SetupRepository(newEvent);
            var handler = new ReportEventCommandHandler(repositoryMock, mapper, eventsConsumerMock);
            var command = new ReportEventCommandDto("0", new ReportedEventDto()
            {
                Coordinates = new Coordinates(2.5, 2.5),
                Description = "test description",
            });

            var result = await handler.Handle(command, default);

            Assert.Equal(newEvent.Id, result.Id);
            Assert.Equal(newEvent.Status, result.Status);
            Assert.Equal(newEvent.Description, result.Data.Description);
            Assert.Equivalent(newEvent.Coordinates, result.Data.Coordinates);
            await repositoryMock.Received()
                .Add(Arg.Is<ReportedEvent>(x => x.Reporter.UserId == "0"), Arg.Any<string>());
            Assert.Single(capturedEvents);
            Assert.Equal(newEvent.Id, ((EventReportedDomainEvent)capturedEvents[0]).EventId);
        }

        private void SetupRepository(ReportedEvent reportedEvent)
        {
            repositoryMock.Add(Arg.Any<ReportedEvent>(), Arg.Any<string>()).Returns(reportedEvent);
        }
        
        private ReportedEvent GetNewReportedEvent(string reporterId)
        {
            return new ReportedEvent()
            {
                Id = "123",
                Coordinates = new Coordinates(2.5, 2.5),
                Description = "test description",
                Status = EventStatusType.Pending,
                Reporter = new Reporter() { UserId = reporterId },
            };
        }
    }
}
