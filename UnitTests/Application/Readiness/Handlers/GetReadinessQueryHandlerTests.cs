using Application.Services.Readiness.Contracts;
using Application.Services.Readiness.DTOs;
using Application.Services.Readiness.Mappers;
using Application.Services.Readiness.Queries;
using AutoMapper;
using Domain.Entities.Readiness;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.User.Handlers
{
    public class GetReadinessQueryHandlerTests
    {
        private readonly IReadinessRepository repository;
        private readonly IMapper mapper;


        public GetReadinessQueryHandlerTests()
        {
            repository = Substitute.For<IReadinessRepository>();
            mapper = MapperUtils.CreateMapper<ReadinessMappingProfile>();
            SetupRepository();
        }

        [Fact]
        public async Task Handle_Should_Return()
        {
            var readiness = GetUserReadiness();
            var handler = new GetReadinessQueryHandler(repository, mapper);
            var query = new GetReadinessQueryDto("user_test_id");

            var result = await handler.Handle(query, default);

            Assert.Equal(readiness.ReadinessStatus, result.ReadinessStatus);
            Assert.Equivalent(readiness.ReadinessRanges, result.ReadinessRanges);
            await repository
                .Received()
                .Get(Arg.Is("user_test_id"));
        }

        private void SetupRepository()
        {
            var toReturn = GetUserReadiness();

            repository.Get(Arg.Any<string>()).Returns(toReturn);
        }

        private UserReadiness GetUserReadiness()
        {
            return new UserReadiness()
            {
                Id = "test_id",
                UserId = "user_test_id",
                ReadinessStatus = ReadinessStatus.Ready,
                ReadinessRanges = new ReadinessRange[]
                {
                    new ReadinessRange(true, TimeSpan.FromHours(15), TimeSpan.FromHours(20), DayOfWeek.Tuesday),
                    new ReadinessRange(true, TimeSpan.FromHours(8), TimeSpan.FromHours(12), DayOfWeek.Thursday),
                    new ReadinessRange(false, TimeSpan.FromHours(8), TimeSpan.FromHours(12), DayOfWeek.Friday),
                }
            };
        }
    }
}
