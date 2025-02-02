using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.Queries;
using Application.Services.Map.PointsOfInterest.Mappers;
using AutoMapper;
using Domain.Entities.Event;
using Domain.Enums;
using NSubstitute;
using UnitTests.Utils;

namespace Application.Services.Event.Handlers
{
    public class GetUserEventsQueryHandlerTests
    {
        private readonly IReportedEventsRepository repositoryMock;
        private readonly IMapper mapper;

        public static List<EventStatusType> eventStatusPending = new List<EventStatusType>() { EventStatusType.Pending };


        public GetUserEventsQueryHandlerTests()
        {
            repositoryMock = Substitute.For<IReportedEventsRepository>();
            mapper = MapperUtils.CreateMapper<ReportedEventMappingProfile>();
        }

        [Theory]
        [MemberData(nameof(GetEventsCases))]
        public async Task Handle_Should_ReturnEvents(bool isResponder, List<EventStatusType> statusList, int expectedReportedEventsCount, int expectedAssigneddEventsCount)
        {
            SetupRepository();
            var handler = new GetUserEventsQueryHandler(repositoryMock, mapper);
            var command = new GetUserEventsQueryDto(
                "1",
                isResponder,
                statusList);

            var result = await handler.Handle(command, default);

            await repositoryMock
                .Received()
                .GetReportedEvents("1", statusList);

            if (isResponder)
            {
                await repositoryMock
                    .Received()
                    .GetAssignedEvents("1", statusList);
            }

            Assert.Equal(expectedReportedEventsCount, result.ReportedEvents.Count);
            Assert.Equal(expectedAssigneddEventsCount, result.AssignedEvents.Count);
        }

        public static IEnumerable<object[]> GetEventsCases() => new List<object[]> {
                new object[]
                {
                    true,
                    eventStatusPending,
                    2,
                    1
                },
                new object[]
                {
                    true,
                    null,
                    3,
                    3
                },
                new object[]
                {
                    false,
                    eventStatusPending,
                    2,
                    0
                },
                new object[]
                {
                    false,
                    null,
                    3,
                    0
                }
            };

        private void SetupRepository()
        {
            repositoryMock.GetReportedEvents(Arg.Any<string>(), eventStatusPending)
                .Returns(new List<ReportedEvent>()
                {
                    new ReportedEvent()
                    {
                        Id = "123"
                    },
                    new ReportedEvent()
                    {
                        Id = "234"
                    }
                });

            repositoryMock.GetReportedEvents(Arg.Any<string>(), null)
                .Returns(new List<ReportedEvent>()
                {
                    new ReportedEvent()
                    {
                        Id = "123"
                    },
                    new ReportedEvent()
                    {
                        Id = "234"
                    },
                    new ReportedEvent()
                    {
                        Id = "345"
                    }
                });


            repositoryMock.GetAssignedEvents(Arg.Any<string>(), eventStatusPending)
                .Returns(new List<ReportedEvent>()
                {
                    new ReportedEvent()
                    {
                        Id = "234"
                    }
                });


            repositoryMock.GetAssignedEvents(Arg.Any<string>(), null)
                .Returns(new List<ReportedEvent>()
                {
                    new ReportedEvent()
                    {
                        Id = "123"
                    },
                    new ReportedEvent()
                    {
                        Id = "234"
                    },
                    new ReportedEvent()
                    {
                        Id = "345"
                    }
                });
        }
    }
}
