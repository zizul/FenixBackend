using Application.Map.PointsOfInterest.Commands;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using Application.Services.Map.PointsOfInterest.Mappers;
using AutoMapper;
using Domain.Entities.Map;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class AddAedPoiCommandHandlerTests
    {
        private readonly IAedRepository repositoryMock;
        private readonly IMapper mapper;


        public AddAedPoiCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IAedRepository>();
            mapper = MapperUtils.CreateMapper<PoiMappingProfile>();
        }

        [Fact]
        public async Task Handle_Should_ReturnAddedResult()
        {
            SetupRepository();
            var handler = new AddAedPoiCommandHandler(repositoryMock, mapper);
            var command = new AddAedPoiCommandDto(new AedDto
            {
                Coordinates = new Coordinates(10.5, 5.25),
                Description = "test",
            });

            var result = await handler.Handle(command, default);

            Assert.Equal(123, result.Id);
            Assert.Equivalent(command.Aed, result.Data);
            await repositoryMock
                .Received()
                .Add(Arg.Is<Aed>(x => x.Id == 0));
        }

        private void SetupRepository()
        {
            repositoryMock.Add(Arg.Any<Aed>())
                .Returns(new Aed()
                {
                    Id = 123,
                    Coordinates = new Coordinates(10.5, 5.25),
                    Description = "test"
                });
        }
    }
}
