
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Identity
{
    internal class OidcOptions
    {
        [ConfigurationKeyName("realm")]
        public string Realm { get; set; }

        [ConfigurationKeyName("auth-server-url")]
        public string AuthServerUrl { get; set; }

        [ConfigurationKeyName("audience")]
        public string Audience { get; set; }

        public string RealmFullUrl { get => $"{AuthServerUrl}realms/{Realm}"; }
    }
}
