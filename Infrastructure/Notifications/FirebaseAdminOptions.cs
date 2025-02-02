using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Infrastructure.Notifications
{
    internal class FirebaseAdminOptions
    {
        [JsonProperty("type")]
        [ConfigurationKeyName("type")]
        public string Type { get; set; }

        [JsonProperty("project_id")]
        [ConfigurationKeyName("project_id")]
        public string ProjectId { get; set; }

        [JsonProperty("private_key_id")]
        [ConfigurationKeyName("private_key_id")]
        public string PrivateKeyId { get; set; }

        [JsonProperty("private_key")]
        [ConfigurationKeyName("private_key")]
        public string PrivateKey { get; set; }

        [JsonProperty("client_email")]
        [ConfigurationKeyName("client_email")]
        public string ClientEmail { get; set; }

        [JsonProperty("client_id")]
        [ConfigurationKeyName("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("auth_uri")]
        [ConfigurationKeyName("auth_uri")]
        public string AuthUri { get; set; }

        [JsonProperty("token_uri")]
        [ConfigurationKeyName("token_uri")]
        public string TokenUri { get; set; }

        [JsonProperty("auth_provider_x509_cert_url")]
        [ConfigurationKeyName("auth_provider_x509_cert_url")]
        public string AuthProviderX509CertUrl { get; set; }

        [JsonProperty("client_x509_cert_url")]
        [ConfigurationKeyName("client_x509_cert_url")]
        public string ClientX509CertUrl { get; set; }

        [JsonProperty("universe_domain")]
        [ConfigurationKeyName("universe_domain")]
        public string UniverseDomain { get; set; }
    }
}
