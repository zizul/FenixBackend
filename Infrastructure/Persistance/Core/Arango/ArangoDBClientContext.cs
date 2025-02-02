using ArangoDBNetStandard;
using ArangoDBNetStandard.Serialization;
using ArangoDBNetStandard.Transport.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistance.Core.Arango
{
    public class ArangoDBClientContext : IArangoDbClientContext
    {
        public IArangoDBClient Client { get; }


        public ArangoDBClientContext(IOptions<ArangoDbOptions> options)
        {
            var serialization = GetApiClientSerialization();
            Client = new ArangoDBClient(GetTransport(options, options.Value.DbName), serialization);
        }

        private HttpApiTransport GetTransport(IOptions<ArangoDbOptions> options, string dbName)
        {
            return HttpApiTransport.UsingBasicAuth(
               new Uri(options.Value.HostUrl), dbName, options.Value.Username, options.Value.Password);
        }

        private IApiClientSerialization GetApiClientSerialization()
        {
            return new JsonNetApiClientCustomSerialization();
        }
    }
}
