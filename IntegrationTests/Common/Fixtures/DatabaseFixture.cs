using ArangoDBNetStandard;
using ArangoDBNetStandard.CollectionApi.Models;
using ArangoDBNetStandard.DatabaseApi.Models;
using ArangoDBNetStandard.DocumentApi.Models;
using ArangoDBNetStandard.Transport.Http;
using Infrastructure.Persistance.Core.Arango;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Common.Fixtures
{
    public class DatabaseFixture : IAsyncLifetime
    {
        internal HttpClient Client { get; }
        internal FenixWebApplicationFactory Application { get; }

        protected readonly IArangoDbClientContext context;
        protected readonly string databaseName;
        private readonly ArangoDBClient adminClient;


        public DatabaseFixture()
        {
            databaseName = GenerateDbName();

            Application = new FenixWebApplicationFactory(databaseName);
            Client = Application.CreateClient();
            context = Application.Services.GetRequiredService<IArangoDbClientContext>();

            var dbOptions = Application.Services.GetRequiredService<IOptions<ArangoDbOptions>>().Value;
            dbOptions.DbName = databaseName;
            adminClient = GetAdminDbClient(dbOptions.HostUrl);
        }

        private string GenerateDbName()
        {
            return $"test-db-{Guid.NewGuid()}";
        }

        private ArangoDBClient GetAdminDbClient(string hostUrl)
        {
            var transport = HttpApiTransport.UsingBasicAuth(
                new Uri(hostUrl), "_system", "root", "test");
            var adminClient = new ArangoDBClient(transport);
            return adminClient;
        }

        // setup
        public virtual async Task InitializeAsync()
        {
            var db = adminClient.Database;

            var databases = await db.GetDatabasesAsync();
            if (databases.Result.Contains(databaseName))
            {
                await db.DeleteDatabaseAsync(databaseName);
            }

            await db.PostDatabaseAsync(new PostDatabaseBody()
            {
                Name = databaseName,
                Options = new PostDatabaseOptions(),
                Users = new List<DatabaseUser>() { new DatabaseUser() { Username = "test", Passwd = "test" } }
            });
        }

        // teardown
        public virtual async Task DisposeAsync()
        {
            var db = adminClient.Database;
            //await db.DeleteDatabaseAsync(databaseName);
        }

        /// <summary>
        /// Creates new collection with given name
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public async Task CreateCollection(string collectionName)
        {
            await context.Client.Collection.PostCollectionAsync(new PostCollectionBody() { Name = collectionName });
        }

        /// <summary>
        /// Creates new document in collection with given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<T> CreateDocument<T>(string collectionName, T obj)
        {
            var result = await context.Client.Document.PostDocumentAsync(
                    collectionName, obj, new PostDocumentsQuery()
                    {
                        ReturnNew = true
                    });
            return result.New;
        }

        public async Task<bool> IsExists(string query)
        {
            var found = (await context.Client.Cursor.PostCursorAsync<object>(query)).Result;
            if (found.Any())
            {
                return true;
            }

            return false;
        }
    }
}