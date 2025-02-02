using Application.Services.Readiness.Commands;
using Application.Services.Readiness.Contracts;
using Application.Services.Readiness.DTOs;
using Application.Services.Readiness.DTOs.Common;
using Application.Services.Readiness.Mappers;
using AutoMapper;
using Domain.Entities.Readiness;
using Domain.Enums;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.Readiness.Handlers
{
    public class UpdateReadinessCommandHandlerTests
    {
        private readonly IReadinessRepository repository;
        private readonly IMapper mapper;


        public UpdateReadinessCommandHandlerTests()
        {
            repository = Substitute.For<IReadinessRepository>();
            mapper = MapperUtils.CreateMapper<ReadinessMappingProfile>();
        }

        [Fact]
        public async Task Handle_Should_AddOrUpdate()
        {
            var readinessDto = GetUserReadinessDto();
            var handler = new UpdateReadinessCommandHandler(repository, mapper);
            var query = new UpdateReadinessCommandDto("user_test_id", readinessDto);

            await handler.Handle(query, default);

            await repository
                .Received()
                .Update(
                Arg.Is("user_test_id"), Arg.Is<UserReadiness>(x =>
                        x.ReadinessStatus == readinessDto.ReadinessStatus &&
                        x.ReadinessRanges.Length == readinessDto.ReadinessRanges.Length));
        }

        private UserReadinessDataDto GetUserReadinessDto()
        {
            return new UserReadinessDataDto()
            {
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
