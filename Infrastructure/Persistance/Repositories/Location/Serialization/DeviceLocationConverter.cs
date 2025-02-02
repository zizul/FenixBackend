using Domain.ValueObjects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Domain.Entities.Location;

namespace Infrastructure.Persistance.Repositories.Location.Serialization
{
    internal class DeviceLocationConverter : JsonConverter<DeviceLocation>
    {
        public override DeviceLocation? ReadJson(JsonReader reader, Type objectType, DeviceLocation? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new DeviceLocation();
            entity.Id = json._key;
            entity.Coordinates = new Coordinates(
               (double)json.location.longitude, (double)json.location.latitude);

            return entity;
        }

        public override void WriteJson(JsonWriter writer, DeviceLocation? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();

            json.location = new JObject
            {
                { "longitude", value.Coordinates.Longitude },
                { "latitude", value.Coordinates.Latitude }
            };

            json.WriteTo(writer);
        }
    }
}
