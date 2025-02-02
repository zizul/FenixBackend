using Newtonsoft.Json;

namespace IntegrationTests.Common.Utils
{
    internal static class ControllersTestsUtils
    {
        /// <summary>
        /// Used because it provides deserialization using added default converters,
        /// when ReadFromJsonAsync doesn't use any converters by default
        /// </summary>
        internal static async Task<T> GetFromResponse<T>(HttpResponseMessage response)
        {
            string jsonContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(jsonContent);
            return result;
        }
    }
}