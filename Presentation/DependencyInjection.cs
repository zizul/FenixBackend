using Presentation.Configuration.Versioning;

namespace Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.ConfigureApiVersioning();
            return services;
        }
    }
}
