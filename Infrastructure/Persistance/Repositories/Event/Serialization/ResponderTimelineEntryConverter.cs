using Domain.Entities.Event;
using Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Event.Serialization
{
    public class ResponderTimelineEntryConverter : JsonConverter<ResponderTimelineEntry>
    {
        public override ResponderTimelineEntry? ReadJson(JsonReader reader, Type objectType, ResponderTimelineEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new ResponderTimelineEntry();
            entity.CreatedAt = json.created_date;
            entity.Status = json.status;
            if (json.transport_type != null && json.transport_type != "")
            {
                entity.Transport = json.transport_type;
            }
            if (json.eta != null)
            {
                entity.ETA = json.eta;
            }
            if (json.location != null)
            {
                entity.Coordinates = new Coordinates(
                                       (double)json.location.longitude, (double)json.location.latitude);
            }

            return entity;
        }

        public override void WriteJson(JsonWriter writer, ResponderTimelineEntry? value, JsonSerializer serializer)
        {
            dynamic json = GetJObject(value);
            json.WriteTo(writer);
        }

        internal JObject GetJObject(ResponderTimelineEntry? value)
        {
            dynamic json = new JObject();
            json.created_date = value.CreatedAt;
            json.status = value.Status.ToString();
            json.transport_type = value.Transport.ToString();
            json.eta = value.ETA;
            if (value.Coordinates != null)
            {
                json.location = new JObject
                {
                    { "longitude", value.Coordinates.Longitude },
                    { "latitude", value.Coordinates.Latitude }
                };
            }
            return json;
        }

    }
}
