using System.Text.Json.Serialization;

namespace Presentation.Configuration.Serialization
{
    public static class ControllersConfiguration
    {
        public static void ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }
    }
}
