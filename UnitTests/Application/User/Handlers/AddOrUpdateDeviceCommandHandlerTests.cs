using Application.Services.User.Commands;
using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using Application.Services.User.Mappers;
using AutoMapper;
using Domain.Entities.User;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.User.Handlers
{
    public class AddOrUpdateDeviceCommandHandlerTests
    {
        private readonly IDeviceRepository repositoryMock;
        private readonly IMapper mapper;


        public AddOrUpdateDeviceCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IDeviceRepository>();
            mapper = MapperUtils.CreateMapper<DeviceMappingProfile>();
            SetupRepository();
        }

        [Fact]
        public async Task Handle_Should_AddOrUpdate()
        {
            var handler = new AddOrUpdateDeviceCommandHandler(repositoryMock, mapper);
            var device = GetDeviceDto();
            var command = new AddOrUpdateDeviceCommandDto("test user", device);

            var added = await handler.Handle(command, default);

            Assert.Equivalent(device, added);
            await repositoryMock
                .Received()
                .AddOrUpdate(Arg.Is<Device>(x => x.DeviceId == device.DeviceId));
        }

        private void SetupRepository()
        {
            var device = GetDeviceDto();
            var toReturn = new Device()
            {
                DeviceId = device.DeviceId,
                DeviceModel = device.DeviceModel,
                FirebaseToken = device.FirebaseToken
            };

            repositoryMock.AddOrUpdate(Arg.Any<Device>()).Returns(toReturn);
        }

        private DeviceDto GetDeviceDto()
        {
            return new DeviceDto
            {
                DeviceId = "test",
                FirebaseToken = "test token",
                DeviceModel = "test model"
            };
        }
    }
}
