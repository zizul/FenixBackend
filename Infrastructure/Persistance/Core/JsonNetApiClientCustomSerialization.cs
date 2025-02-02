using ArangoDBNetStandard.Serialization;
using Infrastructure.Persistance.Repositories.User.Serialization;
using Infrastructure.Persistance.Repositories.Event.Serialization;
using Infrastructure.Persistance.Repositories.Serialization;
using Newtonsoft.Json;
using Infrastructure.Persistance.Repositories.Location.Serialization;
using Infrastructure.Persistance.Repositories.Readiness.Serialization;

namespace Infrastructure.Persistance.Core
{
    /// <summary>
    /// Custom class to add additional Json converters
    /// </summary>
    public class JsonNetApiClientCustomSerialization : JsonNetApiClientSerialization
    {
        public override T DeserializeFromStream<T>(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                return default;
            }

            using StreamReader reader = new StreamReader(stream);
            using JsonTextReader reader2 = new JsonTextReader(reader);
            return JsonSerializer.CreateDefault().Deserialize<T>(reader2);
        }

        public static List<JsonConverter> GetConverters()
        {
            var converters = new List<JsonConverter>();
            AddMapContextConverters(converters);
            AddEventContextConverters(converters);
            AddUserContextConverters(converters);
            AddLocationContextConverters(converters);
            AddReadinessContextConverters(converters);

            return converters;
        }

        private static void AddMapContextConverters(List<JsonConverter> converters)
        {
            converters.Add(new AedConverter());
            converters.Add(new EmergencyDepartmentConverter());
        }

        private static void AddEventContextConverters(List<JsonConverter> converters)
        {
            converters.Add(new ReportedEventConverter());
            converters.Add(new ResponderConverter());
        }

        private static void AddUserContextConverters(List<JsonConverter> converters)
        {
            converters.Add(new UserConverter());
            converters.Add(new DeviceConverter());
        }

        private static void AddLocationContextConverters(List<JsonConverter> converters)
        {
            converters.Add(new DeviceLocationConverter());
        }

        private static void AddReadinessContextConverters(List<JsonConverter> converters)
        {
            converters.Add(new UserReadinessConverter());
        }
    }
}
