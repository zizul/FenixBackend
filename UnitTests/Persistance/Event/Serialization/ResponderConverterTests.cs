using Domain.Entities.Event;
using Domain.Enums;
using Infrastructure.Persistance.Repositories.Event.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Event.Serialization
{
    public class ResponderConverterTests
    {
        private readonly ResponderConverter converter;


        public ResponderConverterTests()
        {
            converter = new ResponderConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var entity = GetEntity();

            var serialized = JsonConvert.SerializeObject(entity, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, entity, converter);
        }

        private Responder GetEntity()
        {
            return new Responder
            (
                eventId: "123",
                identityId: null,
                status: ResponderStatusType.Pending,
                transport: TransportType.Scooter,
                eta: DateTime.UtcNow,
                userId: "456"
            );
        }
    }
}
