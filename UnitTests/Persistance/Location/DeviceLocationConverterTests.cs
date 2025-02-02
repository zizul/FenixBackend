using Domain.Entities.Location;
using Domain.Entities.User;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Location.Serialization;
using Infrastructure.Persistance.Repositories.User.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Location
{
    public class DeviceLocationConverterTests
    {
        private readonly DeviceLocationConverter converter;


        public DeviceLocationConverterTests()
        {
            converter = new DeviceLocationConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var entity = GetEntity();

            var serialized = JsonConvert.SerializeObject(entity, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, entity, converter);
        }

        private DeviceLocation GetEntity()
        {
            return new DeviceLocation()
            {
                Id = "123",
                Coordinates = new Coordinates(52.333, 32.668)
            };
        }
    }
}
