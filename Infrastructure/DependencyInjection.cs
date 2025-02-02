using Application.Services.Event.Contracts;
using Application.Services.Location.Contracts;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Readiness.Contracts;
using Application.Services.User.Contracts;
using Infrastructure.Coordinator;
using Infrastructure.Coordinator.Common;
using Infrastructure.Identity;
using Infrastructure.Notifications;
using Infrastructure.Persistance.Core;
using Infrastructure.Persistance.Core.Arango;
using Infrastructure.Persistance.Core.Ceph;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance.Repositories.Location;
using Infrastructure.Persistance.Repositories.Map;
using Infrastructure.Persistance.Repositories.Readiness;
using Infrastructure.Persistance.Repositories.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("IntegrationTests")]
namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
        {
            AddPersistence(services, configuration);
            AddIdentity(services, configuration);
            AddNotifications(services, configuration);
            return services;
        }

        private static void AddPersistence(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddScoped(typeof(IDocumentRepository<>), typeof(ArangoDbRepository<>));
            services.AddScoped<ITransaction, ArangoDbTransaction>();
            AddArangoDb(services, configuration);
            AddRepositories(services);
            AddObjectStorage(services, configuration);
        }

        private static void AddArangoDb(IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<ArangoDbOptions>(configuration.GetSection("ArangoDb"));
            services.AddSingleton<IArangoDbClientContext, ArangoDBClientContext>();
        }

        private static void AddRepositories(IServiceCollection services)
        {
            services.AddTransient<IPoiRepository, PoiRepository>();
            services.AddTransient<IAedRepository, AedRepository>();
            services.AddTransient<IReportedEventsRepository, ReportedEventsRepository>();
            services.AddTransient<IEventCoordinatorService, EventCoordinatorService>();
            services.AddScoped<IEventCoordinatorRepository, EventCoordinatorRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IDeviceRepository, DeviceRepository>();
            services.AddTransient<IDeviceLocationRepository, DeviceLocationRepository>();
            services.AddTransient<IReadinessRepository, ReadinessRepository>();
            AddConverters();
        }

        private static void AddObjectStorage(IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<ObjectStorageS3Options>(configuration.GetSection("ObjectStorageS3"));
            services.AddSingleton<IObjectStorageS3Context, ObjectStorageS3Context>();
            services.AddScoped<IObjectStorage, CephObjectStorageS3>();
        }

        private static void AddConverters()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = JsonNetApiClientCustomSerialization.GetConverters()
            };
        }

        private static void AddIdentity(IServiceCollection services, ConfigurationManager configuration)
        {
            var oidcOptions = configuration.GetSection("Oidc").Get<OidcOptions>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = oidcOptions.RealmFullUrl;
                options.Audience = oidcOptions.Audience;
                if (IsDevelopmentOrStaging())
                {
                    options.RequireHttpsMetadata = false; // Disable HTTPS requirement
                }
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = oidcOptions.RealmFullUrl,
                    ValidAudience = oidcOptions.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                };
            });

            services.AddAuthorization();
        }

        private static void AddNotifications(IServiceCollection services, ConfigurationManager configuration)
        {
            AddFirebaseAdminOptions(services, configuration);

            services.AddSingleton<IAppNotifier, FirebaseNotifier>();
            services.AddTransient<IRespondersNotifier, RespondersNotifier>();
        }

        private static void AddFirebaseAdminOptions(IServiceCollection services, ConfigurationManager configuration)
        {
            if (IsDevelopment())
            {
                // configure from secret
                services.Configure<FirebaseAdminOptions>(configuration.GetSection("FirebaseAdmin"));
            }
            else if (IsStagingOrProduction())
            {
                // configure from environment variable
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

        private static bool IsDevelopmentOrStaging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == "Development" || environment == "Staging";
        }

        private static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == "Development";
        }

        private static bool IsStagingOrProduction()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == "Staging" || environment == "Production";
        }
    }
}
