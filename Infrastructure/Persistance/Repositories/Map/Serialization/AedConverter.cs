using Domain.Entities.Map;
using Domain.Enums;
using Infrastructure.Persistance.Repositories.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Amazon.S3.Util.S3EventNotification;

namespace Infrastructure.Persistance.Repositories.Serialization
{
    internal class AedConverter : JsonConverter<Aed>
    {
        public const string OSM_ID = "osm_id";
        public const string DEFIBRILLATOR_LOCATION = "defibrillator:location";
        public const string DESCRIPTION = "description";
        public const string INDOOR = "indoor";
        public const string OPENING_HOURS = "opening_hours";
        public const string PHONE = "phone";
        public const string OPERATOR = "operator";
        public const string ACCESS = "access";
        public const string PHOTO_URL = "photo_url";
        public const string LEVEL = "level";


        public override Aed? ReadJson(JsonReader reader, Type objectType, Aed? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new Aed();
            entity.Id = json._key;

            entity.Coordinates = SerializationUtils.ReadCoordinatesFromGeoJson(json);

            var propertiesJson = json[SerializationUtils.PROPERTIES];
            entity.Location = propertiesJson[DEFIBRILLATOR_LOCATION];
            entity.Description = propertiesJson[DESCRIPTION];
            entity.InDoor = GetInDoor((string?)propertiesJson[INDOOR]);
            entity.OpeningHours = propertiesJson[OPENING_HOURS];
            entity.Phone = propertiesJson[PHONE];
            entity.Operator = propertiesJson[OPERATOR];
            entity.Access = GetAccessType((string?)propertiesJson[ACCESS]);
            entity.PhotoUrl = (string?)propertiesJson[PHOTO_URL];
            entity.Level = propertiesJson[LEVEL];

            if (propertiesJson[SerializationUtils.ADDRESS] != null)
            {
                entity.Address = SerializationUtils.ReadAddress(propertiesJson);
            }

            if (propertiesJson[SerializationUtils.AVAILABILITY] != null)
            {
                entity.Availability = SerializationUtils.ReadAvailability(propertiesJson);
            }

            return entity;
        }

        public override void WriteJson(JsonWriter writer, Aed? value, JsonSerializer serializer)
        {
            dynamic geometryJson = new JObject();
            SerializationUtils.WriteCoordinatesInGeoJson(geometryJson, value.Coordinates);

            dynamic propertiesJson = new JObject();

            propertiesJson[OSM_ID] = null;
            propertiesJson[DEFIBRILLATOR_LOCATION] = value.Location;
            propertiesJson[DESCRIPTION] = value.Description;
            propertiesJson[INDOOR] = GetInDoorCode(value.InDoor);
            propertiesJson[OPENING_HOURS] = value.OpeningHours;
            propertiesJson[PHONE] = value.Phone;
            propertiesJson[OPERATOR] = value.Operator;
            propertiesJson[LEVEL] = value.Level;

            propertiesJson[ACCESS] = GetAccessCode(value.Access);
            propertiesJson[PHOTO_URL] = value.PhotoUrl;

            if (value.Address != null)
            {
                SerializationUtils.WriteAddress(propertiesJson, value.Address);
            }

            if (value.Availability != null)
            {
                SerializationUtils.WriteAvailability(propertiesJson, value.Availability);
            }

            dynamic json = SerializationUtils.CreateGeoJson(geometryJson, propertiesJson);

            json.WriteTo(writer);
        }

        /// <summary>
        /// https://wiki.openstreetmap.org/wiki/Tag:emergency%3Ddefibrillator
        /// https://wiki.openstreetmap.org/wiki/Key:access
        /// </summary>
        private AedAccessType GetAccessType(string? property)
        {
            if (property == null)
            {
                return AedAccessType.Unknown;
            }

            if (property == "yes")
            {
                return AedAccessType.Public;
            }
            else if (property == "customers" || property == "permissive")
            {
                return AedAccessType.WorkHours;
            }
            else if (property == "permit")
            {
                return AedAccessType.OwnerConsent;
            }
            else if (property == "no" || property == "private")
            {
                return AedAccessType.NotAvailable;
            }
            return AedAccessType.Unknown;
        }

        private string? GetAccessCode(AedAccessType type)
        {
            if (type == AedAccessType.Unknown)
            {
                return null;
            }
            else if (type == AedAccessType.Public)
            {
                return "yes";
            }
            else if (type == AedAccessType.WorkHours)
            {
                return "customers";
            }
            else if (type == AedAccessType.OwnerConsent)
            {
                return "permit";
            }
            else if (type == AedAccessType.NotAvailable)
            {
                return "no";
            }

            throw new JsonSerializationException("Unknown AedAccessType");
        }

        private bool? GetInDoor(string? inDoor)
        {
            if (inDoor == null || (string?)inDoor == null)
            {
                return null;
            }

            if (inDoor == "yes")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string? GetInDoorCode(bool? inDoor)
        {
            if (!inDoor.HasValue)
            {
                return null;
            }

            if (inDoor.Value)
            {
                return "yes";
            }
            else
            {
                return "no";
            }
        }
    }
}
