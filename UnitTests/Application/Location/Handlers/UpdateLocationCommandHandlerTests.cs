using Application.Services.Location.Commands;
using Application.Services.Location.Contracts;
using Application.Services.Location.DTOs;
using Application.Services.Location.DTOs.Common;
using Application.Services.Location.Mappers;
using AutoMapper;
using Domain.Entities.Location;
using Domain.ValueObjects;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.Location.Handlers
{
    public class UpdateLocationCommandHandlerTests
    {
        private UpdateLocationCommandHandler testedHandler;
        private readonly IDeviceLocationRepository repositoryMock;
        private readonly IMapper mapper;


        public UpdateLocationCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IDeviceLocationRepository>();
            mapper = MapperUtils.CreateMapper<DeviceLocationMappingProfile>();
            testedHandler = new UpdateLocationCommandHandler(repositoryMock, mapper);
        }

        [Fact]
        public async Task Handle_Should_UpdateDevice()
        {
            // Assign
            string deviceId = "123";
            Coordinates coordinates = new Coordinates(2.5, 2.5);

            var command = new UpdateLocationCommandDto(deviceId, new DeviceLocationDto(coordinates));

            // Act
            await testedHandler.Handle(command, default);

            // Assert
            await repositoryMock
                .Received()
                .Update(Arg.Is<DeviceLocation>(x =>
                    x.Id == deviceId &&
                    x.Coordinates == coordinates
                    ), false);
        }
    }
}