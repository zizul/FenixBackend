using Asp.Versioning;
using Presentation.Common;

namespace Presentation.Configuration.Versioning
{
    public static class ApiVersioningConfiguration
    {
        public static void ConfigureApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioningConfiguration()
                    .AddApiExplorerConfiguration();
        }

        private static IApiVersioningBuilder AddApiVersioningConfiguration(this IServiceCollection services)
        {
            return services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = ApiVersions.Default;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
            });
        }

        private static IApiVersioningBuilder AddApiExplorerConfiguration(this IApiVersioningBuilder services)
        {
            return services.AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }
    }
}
