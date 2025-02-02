using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.DocumentApi.Models;

namespace IntegrationTests.Common.Fixtures.Persistence
{
    public class DocumentRepositoryFixture : DatabaseFixture
    {
        internal const string TEST_COLLECTION_1 = "test_collection1";
        internal const string TEST_COLLECTION_2 = "test_collection2";

        internal string DocumentKeyToUpdate { get; private set; }
        internal string DocumentKeyToDelete { get; private set; }
        internal List<string> DocumentKeysToFind { get; private set; }


        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await InitCollections();
            await InitDocuments();
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

        private async Task InitDocuments()
        {
            DocumentKeyToUpdate = await AddAnonymousDocument(
                new { test1 = "test value", test2 = 0 }, 
                TEST_COLLECTION_1);

            DocumentKeyToDelete = await AddAnonymousDocument(
                new { test1 = "test value", test2 = 0 }, 
                TEST_COLLECTION_1);

            DocumentKeysToFind = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                var key = await AddAnonymousDocument(
                    new { test_id = i.ToString(), test2 = "test value" },
                    TEST_COLLECTION_2);
                DocumentKeysToFind.Add(key);
            }
        }

        private async Task<string> AddAnonymousDocument(object documentToAdd, string collection)
        {
            var document = context.Client.Document;
            var result = await document.PostDocumentAsync(
                collection, documentToAdd, new PostDocumentsQuery()
                {
                    ReturnNew = true
                });
            return result._key;
        }
    }
}