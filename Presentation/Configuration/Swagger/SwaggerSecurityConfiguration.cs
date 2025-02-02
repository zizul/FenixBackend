using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Configuration.Swagger
{
    public static class SwaggerSecurityConfiguration
    {
        public static SwaggerGenOptions ConfigureSwaggerSecurityOptions(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", GetSecurityDefinition());
            options.AddSecurityRequirement(GetSecurityRequirement());
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

            return options;
        }

        private static OpenApiSecurityScheme GetSecurityDefinition()
        {
            return new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            };
        }

        private static OpenApiSecurityRequirement GetSecurityRequirement()
        {
            return new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            };
        }
    }
}
