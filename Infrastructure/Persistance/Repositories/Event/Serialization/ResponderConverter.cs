using Domain.Entities.Event;
using Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Event.Serialization
{
    internal class ResponderConverter : JsonConverter<Responder>
    {
        public override Responder? ReadJson(JsonReader reader, Type objectType, Responder? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new Responder();
            entity.UserId = json.responder_ref;
            entity.EventId = json.event_ref;
            entity.Status = json.status;
            entity.ETA = json.eta;
            if (json.transport_type != null && json.transport_type != "")
            {
                entity.Transport = json.transport_type;
            }
            if (json.location != null)
            {
                entity.Coordinates = new Coordinates(
                    (double)json.location.longitude, (double)json.location.latitude);
            }

            if (json.timeline != null)
            { 
                var timelineConverter = new ResponderTimelineEntryConverter();
                foreach (var timelineEntry in json.timeline)
                {
                    var timelineEntryReader = new JTokenReader(timelineEntry);
                    var timelineEntity = timelineConverter.ReadJson(timelineEntryReader, typeof(ResponderTimelineEntry), null, false, serializer);
                    entity.Timeline.AddEntry(timelineEntity);
                }
            }

            AddReadonlyProps(json, entity);

            return entity;
        }

        private void AddReadonlyProps(dynamic json, Responder entity)
        {
            if (json.identity_id != null)
            {
                entity.IdentityId = json.identity_id;
            }
            if (json.name != null)
            {
                entity.Name = json.name;
                if (json.surname != null)
                {
                    entity.Name += " " + json.surname;
                }
            }
            if (json.avatar_url != null)
            {
                entity.AvatarUrl = json.avatar_url;
            }
        }

        public override void WriteJson(JsonWriter writer, Responder? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();
            json.responder_ref = value.UserId;
            json.event_ref = value.EventId;
            json.status = value.Status.ToString();
            json.eta = value.ETA;
            json.transport_type = value.Transport.ToString();
            if (value.Coordinates != null)
            {
                json.location = new JObject
                {
                    { "longitude", value.Coordinates.Longitude },
                    { "latitude", value.Coordinates.Latitude }
                };
            }

            if (value.Timeline != null)
            {
                var timelineConverter = new ResponderTimelineEntryConverter();
                json.timeline = new JArray();
                foreach (var entry in value.Timeline.Entries)
                {
                    JObject timelinEntry = timelineConverter.GetJObject(entry);
                    json.timeline.Add(timelinEntry);
                }
            }

            json.WriteTo(writer);
        }
    }
}
