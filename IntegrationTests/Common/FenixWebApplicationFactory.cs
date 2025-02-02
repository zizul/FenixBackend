using Asp.Versioning.ApiExplorer;
using Infrastructure.Notifications;
using Infrastructure.Persistance.Core;
using Infrastructure.Persistance.Core.Arango;
using IntegrationTests.Common.Stubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace IntegrationTests.Common
{
    internal class FenixWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string BaseUrl => "localhost";

        private readonly string databaseName;


        public FenixWebApplicationFactory(string databaseName) 
        {
            this.databaseName = databaseName;
        }

        public string GetRoute(string endpointRoute)
        {
            var version = this.Services.GetRequiredService<IApiVersionDescriptionProvider>()
                .ApiVersionDescriptions
                .First(x => !x.IsDeprecated)
                .ApiVersion
                .MajorVersion;
            string resultRoute = endpointRoute.Replace("{version:apiVersion}", version.ToString());
            return resultRoute;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(IOptions<ArangoDbOptions>));

                var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                ConfigureTestDb(services, config);

                FakeAuthenticationAttributes(services);

                StubFirebaseInstance(services, config);

                StubObjectStorageInstance(services);
            });
        }

        private void ConfigureTestDb(IServiceCollection services, IConfiguration config) 
        {
            var dbOptions = config.GetSection("ArangoDbIntegrationTest").Get<ArangoDbOptions>();
            dbOptions.DbName = databaseName;
            var options = Options.Create(dbOptions);
            services.AddSingleton(options!);
        }

        private void FakeAuthenticationAttributes(IServiceCollection services)
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.SchemeName, options => { });
        }

        private void StubFirebaseInstance(IServiceCollection services, IConfiguration config)
        {
            services.RemoveAll(typeof(IAppNotifier));
            services.AddSingleton<IAppNotifier, AppNotifierStub>();

            // try get credentials from secret
            // integration tests are running in Development env
            var section = config.GetSection("FirebaseAdmin");
            if (section.Exists())
            {
                services.Configure<FirebaseAdminOptions>(section);
            }
            else
            {
                // integration tests are running in Staging env (during the pipeline)
                // try getting credentials from environment variable instead
                var env = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
                if (string.IsNullOrWhiteSpace(env))
                {
                    throw new ArgumentNullException("FIREBASE_CONFIG", "FIREBASE_CONFIG environment variable is null or empty.");
                }

                var decodedEnv = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(env));
                var options = Options.Create(JsonConvert.DeserializeObject<FirebaseAdminOptions>(decodedEnv)!);
                services.AddSingleton(options);
            }
        }

        private void StubObjectStorageInstance(IServiceCollection services)
        {
            services.RemoveAll(typeof(IObjectStorage));
            services.AddSingleton<IObjectStorage, ObjectStorageStub>();
        }
    }
}