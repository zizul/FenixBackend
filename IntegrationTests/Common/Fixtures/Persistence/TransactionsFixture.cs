using ArangoDBNetStandard.CollectionApi.Models;
using Infrastructure.Persistance.Core;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Common.Fixtures.Persistence
{
    public class TransactionsFixture : DatabaseFixture
    {
        internal const string TEST_COLLECTION_1 = "test_collection1";
        internal const string TEST_COLLECTION_2 = "test_collection2";

        internal static string[] TEST_COLLECTIONS = new string[] { TEST_COLLECTION_1, TEST_COLLECTION_2 };


        public TransactionsFixture()
        {
        }

        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await InitCollections();
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task InitCollections()
        {
            var collection = context.Client.Collection;
            await collection.PostCollectionAsync(new PostCollectionBody() { Name = TEST_COLLECTION_1 });
            await collection.PostCollectionAsync(new PostCollectionBody() { Name = TEST_COLLECTION_2 });
        }

        internal ITransaction GetTransaction(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<ITransaction>();
        }

        internal IDocumentRepository<object> GetRepository(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<IDocumentRepository<object>>();
        }
    }
}