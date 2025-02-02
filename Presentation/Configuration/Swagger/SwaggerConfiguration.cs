using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Configuration.Swagger
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerOptionsConfiguration>();
            services.AddSwaggerGen(options =>
            {
                options.ConfigureSwaggerSecurityOptions();

                options.CustomSchemaIds(SchemaIdStrategy);

                options.OperationFilter<SwaggerDefaultValues>();
                options.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("00:00:00")
                });

                options.UseAllOfToExtendReferenceSchemas();
            });

            return services;
        }

        private static string SchemaIdStrategy(Type currentClass)
        {
            string returnedValue = currentClass.Name;
            if (returnedValue.ToLower().EndsWith("dto"))
            {
                returnedValue = returnedValue.Remove(returnedValue.Length - 3);
            }
            if (returnedValue.ToLower().EndsWith("dtos"))
            {
                returnedValue = returnedValue.Remove(returnedValue.Length - 4);
            }
            return returnedValue;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseSwagger();
                app.UseConfiguredSwaggerUI();
            }

            return app;
        }

        private static IApplicationBuilder UseConfiguredSwaggerUI(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(options =>
            {
                var descriptions = ((WebApplication)app).DescribeApiVersions();

                // Build a swagger endpoint for each discovered API version
                foreach (var description in descriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
            });
            return app;
        }
    }
}
