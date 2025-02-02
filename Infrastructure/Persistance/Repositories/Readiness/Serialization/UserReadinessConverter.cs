using Domain.Entities.Readiness;
using Domain.Enums;
using Domain.ValueObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Readiness.Serialization
{
    internal class UserReadinessConverter : JsonConverter<UserReadiness>
    {
        public const string USER_REF = "user_ref";
        public const string READINESS_STATUS = "readiness_status";
        public const string RANGES = "ranges";
        public const string ENABLED = "enabled";
        public const string START_TIME = "start_time";
        public const string END_TIME = "end_time";
        public const string DAY = "day";


        public override UserReadiness? ReadJson(JsonReader reader, Type objectType, UserReadiness? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            dynamic json = JObject.Load(reader);

            var entity = new UserReadiness();
            entity.Id = json._key;
            entity.UserId = json[USER_REF];
            entity.ReadinessStatus = GetReadinessStatus((string?)json[READINESS_STATUS]);

            var ranges = new List<ReadinessRange>();
            foreach (var rangeJson in json[RANGES])
            {
                ranges.Add(new ReadinessRange(
                    (bool)rangeJson[ENABLED],
                    (TimeSpan)rangeJson[START_TIME],
                    (TimeSpan)rangeJson[END_TIME],
                    (DayOfWeek)rangeJson[DAY]));
            }
            entity.ReadinessRanges = ranges.ToArray();

            return entity;
        }

        public override void WriteJson(JsonWriter writer, UserReadiness? value, JsonSerializer serializer)
        {
            dynamic json = new JObject();
            json[USER_REF] = value.UserId;
            json[READINESS_STATUS] = GetReadinessStatus(value.ReadinessStatus);

            var rangesJson = new JArray();
            foreach (var range in value.ReadinessRanges)
            {
                rangesJson.Add(new JObject
                {
                    { ENABLED, range.IsEnabled },
                    { START_TIME, range.AvailableFrom },
                    { END_TIME, range.AvailableTo },
                    { DAY, range.Day.ToString() },
                });
            }
            json[RANGES] = rangesJson;

            json.WriteTo(writer);
        }

        private ReadinessStatus GetReadinessStatus(string? property)
        {
            if (property == null)
            {
                return ReadinessStatus.NotReady;
            }

            if (property == "ready")
            {
                return ReadinessStatus.Ready;
            }
            else if (property == "not_ready" || property == "notready")
            {
                return ReadinessStatus.NotReady;
            }
            else if (property == "byschedule" || property == "by_schedule")
            {
                return ReadinessStatus.BySchedule;
            }

            return ReadinessStatus.NotReady;
        }

        private string? GetReadinessStatus(ReadinessStatus status)
        {
            if (status == ReadinessStatus.Ready)
            {
                return "ready";
            }
            else if (status == ReadinessStatus.NotReady)
            {
                return "notready";
            }
            else if (status == ReadinessStatus.BySchedule)
            {
                return "byschedule";
            }

            throw new JsonSerializationException("Unknown ReadinessStatus: " + status);
        }
    }
}
