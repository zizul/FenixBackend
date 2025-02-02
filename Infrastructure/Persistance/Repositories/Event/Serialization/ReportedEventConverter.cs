using Domain.Entities.Event;
using Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Event.Serialization
{
    internal class ReportedEventConverter : JsonConverter<ReportedEvent>
    {
        public override ReportedEvent? ReadJson(JsonReader reader, Type objectType, ReportedEvent? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new ReportedEvent();
            entity.Id = json._key;
            entity.Status = json.status;
            entity.Reporter = new Reporter() { UserId = json.reporter_ref };
            entity.Coordinates = new Coordinates(
                (double)json.location.longitude, (double)json.location.latitude);
            entity.CreatedAt = json.created_date;
            if ((DateTime?)json.completed_date != null)
            {
                entity.ClosedAt = json.completed_date;
            }

            if (json.address != null) 
            {
                dynamic address = json.address;
                entity.Address = new Address(
                    (string)address.street, 
                    (string?)address.house, 
                    (string?)address.flat, 
                    (string?)address.floor, 
                    (string?)address.postal_code);
            }
            entity.EventType = json.event_type;
            entity.InjuredCount = json.injured_count;
            entity.Description = json.description;

            if (json.responders != null)
            {
                var responderConverter = new ResponderConverter();
                foreach (var responder in json.responders)
                {
                    var responderReader = new JTokenReader(responder);
                    var responderEntity = responderConverter.ReadJson(responderReader, typeof(Responder), null, false, serializer);
                    entity.Responders.Add(responderEntity);
                }
            }

            return entity;
        }

        public override void WriteJson(JsonWriter writer, ReportedEvent? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();
            json.status = value!.Status.ToString();
            json.reporter_ref = value.Reporter.UserId;
            json.location = new JObject
            {
                { "longitude", value.Coordinates.Longitude },
                { "latitude", value.Coordinates.Latitude }
            };
            json.completed_date = GetDateTimeOrNull(value.ClosedAt);
            json.created_date = value.CreatedAt;

            if (value.Address != null)
            {
                json.address = new JObject
                {
                    { "street", value.Address.Street },
                    { "house", value.Address.House },
                    { "flat", value.Address.Flat },
                    { "floor", value.Address.Floor },
                    { "postal_code", value.Address.PostalCode },
                };
            }
            json.event_type = value.EventType;
            json.injured_count = value.InjuredCount;
            json.description = value.Description;

            json.WriteTo(writer);
        }

        private object? GetDateTimeOrNull(DateTime date) 
        {
            if (IsDateTimeAssigned(date))
            {
                return date;
            }
            else
            {
                return null;
            }
        }

        private bool IsDateTimeAssigned(DateTime date)
        {
            return date != null && date != DateTime.MinValue;
        }

        private object? GetDateTimeOrNull(DateTime? date) 
        {
            if (IsDateTimeAssigned(date))
            {
                return date;
            }
            else
            {
                return null;
            }
        }

        private bool IsDateTimeAssigned(DateTime? date)
        {
            return date != null && date != DateTime.MinValue;
        }
    }
}
