using Domain.Entities.User;
using Domain.Enums;
using Infrastructure.Persistance.Repositories.User.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.User
{
    public class UserConverterTests
    {
        private readonly UserConverter converter;


        public UserConverterTests()
        {
            converter = new UserConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var entity = GetEntity();

            var serialized = JsonConvert.SerializeObject(entity, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, entity, converter);
        }

        private BasicUser GetEntity()
        {
            return new BasicUser()
            {
                Id = "123",
                FirstName = "test name",
                LastName = "test surname",
                Role = UserRoles.User,
                Email = "test@mail",
                Username = "test username",
                MobileNumber = "123456789",
                IdentityId = "test id",
                IsEmailVerified = true,
                IsMobileNumberVerified = true,
            };
        }
    }
}
