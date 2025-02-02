using Application.Map.PointsOfInterest.Commands;
using Domain.Entities.Map;
using NSubstitute;
using AutoMapper;
using UnitTests.Utils;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using Domain.ValueObjects;
using Application.Services.Map.PointsOfInterest.Mappers;
using Domain.Enums;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class UpdateAedPoiCommandHandlerTests
    {
        private readonly IAedRepository repositoryMock;
        private readonly IMapper mapper;


        public UpdateAedPoiCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IAedRepository>();
            mapper = MapperUtils.CreateMapper<PoiMappingProfile>();
        }

        [Fact]
        public async Task Handle_Should_ReturnUpdatedResult()
        {
            SetupRepository();
            var handler = new UpdateAedPoiCommandHandler(repositoryMock, mapper);
            var command = new UpdateAedPoiCommandDto("123", new AedDto
            {
                Coordinates = new Coordinates(10.5, 5.25),
                Description = "test",
                Access = AedAccessType.Public,
                Address = new Address("test street"),
                Operator = "test operator",
            });

            var result = await handler.Handle(command, default);

            Assert.Equal(123, result.Id);
            Assert.Equivalent(command.Aed, result.Data);
            await repositoryMock
                .Received()
                .Update(Arg.Is<string>(m => m == "123"), Arg.Is<Aed>(m => m.Id == 0));
        }

        private void SetupRepository()
        {
            repositoryMock.Update(Arg.Any<string>(), Arg.Any<Aed>())
                .Returns(new Aed()
                {
                    Id = 123,
                    Coordinates = new Coordinates(10.5, 5.25),
                    Description = "test",
                    Access = AedAccessType.Public,
                    Address = new Address("test street"),
                    Operator = "test operator",
                });
        }
    }
}
