using Domain.Entities.Map;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Map.Serialization
{
    public class EmergencyDepartmentConverterTests
    {
        private readonly EmergencyDepartmentConverter converter;


        public EmergencyDepartmentConverterTests()
        {
            converter = new EmergencyDepartmentConverter();
        }

        [Fact]
        public void Converter_Should_WriteInGeoJsonFormat()
        {
            var department = GetEntity();

            var serialized = JsonConvert.SerializeObject(department, new JsonConverter[] { converter });

            ConverterUtils.AssertIsSerializedInGeoJson(serialized);
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var department = GetEntity();

            var serialized = JsonConvert.SerializeObject(department, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, department, converter);
        }

        private EmergencyDepartment GetEntity()
        {
            return new EmergencyDepartment()
            {
                Id = 123,
                Address = new Address(street: "test street", house: "12", postalCode: "12 345", town: "test town"),
                DepartmentName = "test name",
                Coordinates = new Coordinates(10.5, 5.25),
                Phone = "123 456 789"
            };
        }
    }
}
