using Domain.Entities.User;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.User.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.User
{
    public class DeviceConverterTests
    {
        private readonly DeviceConverter converter;


        public DeviceConverterTests()
        {
            converter = new DeviceConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            // Assign
            var device = GetEntity();

            // Act
            var serializedDeviceJson = JsonConvert.SerializeObject(device, new JsonConverter[] { converter });

            string serializeDeviceJsonArangoFormat = ConverterUtils.AssignArangoKeyProperty(123, serializedDeviceJson);

            var deserializedDeviceObject = JsonConvert.DeserializeObject<Device>(
                serializeDeviceJsonArangoFormat,
                new JsonConverter[] { converter });

            // Assert
            Assert.Equivalent(device, deserializedDeviceObject);
        }

        private Device GetEntity()
        {
            return new Device()
            {
                Id = "123",
                DeviceModel = "test device",
                DeviceId = "123",
                FirebaseToken = "456",
                UserId = "12345",
                Coordinates = new Coordinates(52.333, 32.668)
            };
        }
    }
}
