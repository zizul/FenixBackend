using Domain.Entities.User;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace UnitTests.Utils
{
    internal static class ConverterUtils
    {
        public static void AssertIsSerializedInGeoJson(string serialized)
        {
            dynamic geoJson = JsonConvert.DeserializeObject(serialized)!;
            Assert.NotNull(geoJson.type);
            Assert.NotNull(geoJson.geometry);
            Assert.NotNull(geoJson.properties);
        }
        
        public static void AssertReadWriteAreSame<T>(string serialized, T originalObj, JsonConverter converter)
        {
            // emulation of read record from db (_key attribute is required)
            string serializedAsArango = AssignArangoKeyProperty(123, serialized);

            var deserialized = JsonConvert.DeserializeObject<T>(
                    serializedAsArango,
                    new JsonConverter[] { converter });
            Assert.Equivalent(originalObj, deserialized);
        }

        public static string AssignArangoKeyProperty(int id, string serialized)
        {
            dynamic obj = JsonConvert.DeserializeObject(serialized)!;
            obj._key = id.ToString();
            return JsonConvert.SerializeObject(obj);
        }
    }
}
