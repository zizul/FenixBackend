using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.Queries;
using Application.Services.Map.PointsOfInterest.Mappers;
using AutoMapper;
using Domain.Entities.Event;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace Application.Services.Event.Handlers
{
    public class GetReportedEventByIdQueryHandlerTests
    {
        private readonly IReportedEventsRepository repositoryMock;
        private readonly IMapper mapper;


        public GetReportedEventByIdQueryHandlerTests()
        {
            repositoryMock = Substitute.For<IReportedEventsRepository>();
            mapper = MapperUtils.CreateMapper<ReportedEventMappingProfile>();
        }

        [Fact]
        public async Task Handle_Should_ReturnReportedEventForResponder()
        {
            var reportedEvent = GetReportedEvent("123", "0", "1");
            SetupRepository(reportedEvent);
            var handler = new GetReportedEventByIdQueryHandler(repositoryMock, mapper);
            var query = new GetReportedEventByIdQueryDto(reportedEvent.Id, "2", true);

            var result = await handler.Handle(query, default);

            Assert.Equal(reportedEvent.Id, result.Id);
            Assert.Equivalent(reportedEvent.Coordinates, result.Data!.Coordinates);
            Assert.Collection(result.Responders, x =>
            {
                Assert.Equal("0", x.IdentityId);
                Assert.Equivalent(new Coordinates(2, 2), x.Coordinates);
            });
            Assert.Equal(reportedEvent.Description, result.Data.Description);
            Assert.Equal(reportedEvent.Status, result.Status);
            Assert.Equal(reportedEvent.EventType, result.Data.EventType);
            Assert.Equal(reportedEvent.InjuredCount, result.Data.InjuredCount);
            Assert.Equivalent(reportedEvent.Address, result.Data.Address);
            Assert.Equivalent(reportedEvent.CreatedAt, result.CreatedAt);
            Assert.Equivalent(reportedEvent.ClosedAt, result.ClosedAt);
        }

        [Fact]
        public async Task Handle_Should_ReturnReportedEventForReporter()
        {
            var reportedEvent = GetReportedEvent("123", "0", "1");
            SetupRepository(reportedEvent);
            var handler = new GetReportedEventByIdQueryHandler(repositoryMock, mapper);
            var query = new GetReportedEventByIdQueryDto(reportedEvent.Id, "1", false);

            var result = await handler.Handle(query, default);

            Assert.Equal(reportedEvent.Id, result.Id);
            Assert.Collection(result.Responders, x =>
            {
                Assert.Equal("0", x.IdentityId);
                Assert.Null(x.Coordinates);
            });
            Assert.Null(result.Data);
            Assert.Null(result.Reporter);
            Assert.Equal(reportedEvent.Status, result.Status);
            Assert.Equivalent(reportedEvent.CreatedAt, result.CreatedAt);
            Assert.Equivalent(reportedEvent.ClosedAt, result.ClosedAt);
        }

        private void SetupRepository(ReportedEvent reportedEvent)
        {
            repositoryMock.Get(Arg.Any<string>()).Returns(reportedEvent);
            repositoryMock.IsUserReporter(Arg.Any<ReportedEvent>(), Arg.Is("1")).Returns(true);
            repositoryMock.IsUserReporter(Arg.Any<ReportedEvent>(), Arg.Is("2")).Returns(false);
        }

        private ReportedEvent GetReportedEvent(string eventId, string responderId, string reporterId)
        {
            return new ReportedEvent()
            {
                Id = eventId,
                Coordinates = new Coordinates(1, 1),
                Responders = new List<Responder>()
                {
                    new Responder(eventId: null, identityId: responderId, status: ResponderStatusType.Pending, coordinates: new Coordinates(2, 2) )
                },
                Description = "test description",
                Status = EventStatusType.Accepted,
                Address = new Address("test street", "1", "1"),
                EventType = "test type",
                InjuredCount = 1,
                Reporter = new Reporter() { UserId = reporterId },
                CreatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(15)),
                ClosedAt = DateTime.UtcNow,
            };
        }
    }
}