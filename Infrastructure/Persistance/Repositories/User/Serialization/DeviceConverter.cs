using Domain.ValueObjects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Domain.Entities.User;

namespace Infrastructure.Persistance.Repositories.User.Serialization
{
    internal class DeviceConverter : JsonConverter<Device>
    {
        public override Device? ReadJson(JsonReader reader, Type objectType, Device? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new Device();
            entity.Id = json._key;
            entity.UserId = json.user_ref;
            entity.DeviceId = json.device_id;
            entity.FirebaseToken = json.firebase_token;
            entity.DeviceModel = json.device_model;
            if (json.location != null)
            {
                entity.Coordinates = new Coordinates(
                    (double)json.location.longitude, (double)json.location.latitude);
            }

            return entity;
        }

        public override void WriteJson(JsonWriter writer, Device? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();
            if (value.UserId != null)
            {
                json.user_ref = value.UserId;
            }
            if (value.DeviceId != null)
            {
                json.device_id = value.DeviceId;
            }
            if (value.DeviceModel != null)
            {
                json.device_model = value.DeviceModel;
            }
            if (value.FirebaseToken != null)
            {
                json.firebase_token = value.FirebaseToken;
            }            

            if (value.Coordinates != null)
            {
                json.location = new JObject
                {
                    { "longitude", value.Coordinates.Longitude },
                    { "latitude", value.Coordinates.Latitude }
                };
            }

            json.WriteTo(writer);
        }
    }
}
