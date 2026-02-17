using Application.Behaviors;
using Application.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]
namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblies(
                    new Assembly[] {
                        typeof(Application.DependencyInjection).Assembly
                });
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(
                typeof(Application.DependencyInjection).Assembly, includeInternalTypes: true);

            services.AddAutoMapper(
                typeof(Application.DependencyInjection).Assembly);

            AddDomainEventsDispatcher(services);

            return services;
        }

        private static void AddDomainEventsDispatcher(IServiceCollection services)
        {
            services.AddScoped<IDomainEventConsumer, DomainEventDispatcher>();
        }
    }
}
