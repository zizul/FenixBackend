using Domain.Entities.Map;
using Domain.ValueObjects;
using Application.Map.PointsOfInterest.Queries;
using Domain.Enums;
using NSubstitute;
using AutoMapper;
using UnitTests.Utils;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.Queries;
using Application.Services.Map.PointsOfInterest.Mappers;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class GetMedicalPoiQueryHandlerTests
    {
        private readonly IPoiRepository repositoryMock;
        private readonly IMapper mapper;


        public GetMedicalPoiQueryHandlerTests()
        {
            repositoryMock = Substitute.For<IPoiRepository>();
            mapper = MapperUtils.CreateMapper<PoiMappingProfile>();
        }

        [Fact]
        public async Task Handle_Should_ReturnAllPoints()
        {
            SetupRepository();
            var handler = new GetMedicalPoiQueryHandler(repositoryMock, mapper);
            var command = new GetMedicalPoiQueryDto(
                new Coordinates(10.5, 5.25), 1_000, new List<PoiFilter>(), false);

            var result = await handler.Handle(command, default);

            Assert.True(result.Aeds.Count > 0);
            Assert.True(result.Sors.Count > 0);
            Assert.True(result.Niswols.Count > 0);
        }

        [Fact]
        public async Task Handle_Should_ReturnFilteredPoints()
        {
            SetupRepository();
            var handler = new GetMedicalPoiQueryHandler(repositoryMock, mapper);
            var command = new GetMedicalPoiQueryDto(
                new Coordinates(10.5, 5.25), 
                1_000,
                new List<PoiFilter>()
                {
                    new PoiFilter(PointOfInterestType.AED, false),
                    new PoiFilter(PointOfInterestType.SOR, false),
                },
                false);

            var result = await handler.Handle(command, default);

            Assert.True(result.Aeds.Count == 0);
            Assert.True(result.Sors.Count == 0);
            Assert.True(result.Niswols.Count > 0);
        }

        private void SetupRepository()
        {
            repositoryMock.GetAeds(Arg.Any<Coordinates>(), Arg.Any<double?>(), Arg.Any<bool>())
                .Returns(new List<Aed>()
                {
                    new Aed()
                    {
                        Id = 123,
                        Coordinates = new Coordinates(10.5, 5.25),
                        Description = "test",
                    }
                });

            repositoryMock.GetSors(Arg.Any<Coordinates>(), Arg.Any<double?>(), Arg.Any<bool>())
                .Returns(new List<EmergencyDepartment>()
                {
                    new EmergencyDepartment()
                    {
                        Id = 456,
                        Coordinates = new Coordinates(10.5, 5.25),
                        Phone = "123 456 789",
                    }
                });

            repositoryMock.GetNiswols(Arg.Any<Coordinates>(), Arg.Any<double?>(), Arg.Any<bool>())
                .Returns(new List<EmergencyDepartment>()
                {
                    new EmergencyDepartment()
                    {
                        Id = 789,
                        Coordinates = new Coordinates(10.5, 5.25),
                        Phone = "123 456 789"
                    }
                });
        }
    }
}
