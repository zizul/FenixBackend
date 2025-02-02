using ArangoDBNetStandard;

namespace Infrastructure.Persistance.Core.Arango
{
    public interface IArangoDbClientContext
    {
        public IArangoDBClient Client { get; }
    }
}
