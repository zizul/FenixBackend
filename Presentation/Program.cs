using Presentation;
using Infrastructure;
using Application;
using NLog.Web;
using NLog;
using Presentation.Configuration.Swagger;
using Presentation.Middleware;
using System.Runtime.CompilerServices;
using Presentation.Configuration.Serialization;

[assembly: InternalsVisibleTo("IntegrationTests")]

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services
        .AddApplicationAddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddPresentation(builder.Configuration);

    builder.Services.ConfigureControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();

    //configure logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwaggerConfiguration(app.Environment);

    if (app.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

#pragma warning disable CA1050
public partial class Program
{
}
#pragma warning restore CA1050
