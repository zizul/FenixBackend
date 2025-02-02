using Domain.Entities.Readiness;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Readiness.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Event.Serialization
{
    public class UserReadinessConverterTests
    {
        private readonly UserReadinessConverter converter;


        public UserReadinessConverterTests()
        {
            converter = new UserReadinessConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var entity = GetEntity();

            var serialized = JsonConvert.SerializeObject(entity, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, entity, converter);
        }

        private UserReadiness GetEntity()
        {
            return new UserReadiness()
            {
                Id = "123",
                UserId = "user_test_id",
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
