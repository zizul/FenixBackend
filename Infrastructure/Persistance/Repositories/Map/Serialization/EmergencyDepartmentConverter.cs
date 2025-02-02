using Domain.Entities.Map;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Serialization
{
    internal class EmergencyDepartmentConverter : JsonConverter<EmergencyDepartment>
    {
        public const string NAZWA_SWD = "nazwa_swd";
        public const string TELEFON_REJ = "telefon_rej";
        public const string ADR_LOK_ULICA = "adr_lok_ulica";
        public const string ADR_LOK_NR_DOMU = "adr_lok_nr_domu";
        public const string ADR_LOK_NR_LOKALU = "adr_lok_nr_lokalu";
        public const string ADR_LOK_KOD_POCZT = "adr_lok_kod_poczt";
        public const string ADR_LOK_MIEJSC = "adr_lok_miejsc";


        public override EmergencyDepartment? ReadJson(JsonReader reader, Type objectType, EmergencyDepartment? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new EmergencyDepartment();
            entity.Id = json._key;

            entity.Coordinates = SerializationUtils.ReadCoordinatesFromGeoJson(json);

            var propertiesJson = json[SerializationUtils.PROPERTIES];
            entity.DepartmentName = propertiesJson[NAZWA_SWD];

            entity.Address = new Address(
                (string)propertiesJson[ADR_LOK_ULICA],
                (string?)propertiesJson[ADR_LOK_NR_DOMU],
                (string?)propertiesJson[ADR_LOK_NR_LOKALU],
                null,
                (string?)propertiesJson[ADR_LOK_KOD_POCZT],
                (string?)propertiesJson[ADR_LOK_MIEJSC]);

            entity.Phone = propertiesJson[TELEFON_REJ];

            return entity;
        }

        public override void WriteJson(JsonWriter writer, EmergencyDepartment? value, JsonSerializer serializer)
        {
            dynamic geometryJson = new JObject();
            SerializationUtils.WriteCoordinatesInGeoJson(geometryJson, value.Coordinates);

            dynamic propertiesJson = new JObject();

            propertiesJson[NAZWA_SWD] = value.DepartmentName;

            propertiesJson[ADR_LOK_ULICA] = value.Address.Street;
            propertiesJson[ADR_LOK_NR_DOMU] = value.Address.House;
            propertiesJson[ADR_LOK_NR_LOKALU] = value.Address.Flat;
            propertiesJson[ADR_LOK_KOD_POCZT] = value.Address.PostalCode;
            propertiesJson[ADR_LOK_MIEJSC] = value.Address.Town;

            propertiesJson[TELEFON_REJ] = value.Phone;

            dynamic json = SerializationUtils.CreateGeoJson(geometryJson, propertiesJson);
            json.WriteTo(writer);
        }
    }
}
