using Application.Common;
using Application.Services.Event.Contracts;
using Application.Services.User.Contracts;
using Domain.Entities.Event;
using Domain.Enums;
using Infrastructure.Coordinator.Common;
using Infrastructure.Persistance.Repositories.User;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Coordinator
{
    /// <summary>
    /// Orchestrates responder search, assignment, and notification for reported events.
    /// Creates its own service scope because it runs outside the HTTP request pipeline
    /// (invoked by the RabbitMQ consumer in a MassTransit scope).
    /// Sealed to prevent subclassing and unintended scope/lifetime behavior.
    /// </summary>
    internal sealed class EventCoordinatorService : IEventCoordinatorService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        private IEventCoordinatorRepository eventRepository = null!;
        private IDeviceRepository deviceRepository = null!;
        private IDomainEventConsumer eventsConsumer = null!;
        private IRespondersNotifier notifier = null!;


        public EventCoordinatorService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc />
        public async Task<bool> TryFindAndAssignRespondersToEvent(string eventId, double radiusInKm)
        {
            using var scope = serviceScopeFactory.CreateScope();
            SetServices(scope.ServiceProvider);

            var reportedEvent = await TryUpdateEventState(eventId);

            // Event is no longer pending — stop searching for responders
            if (reportedEvent.Status != EventStatusType.Pending)
                return false;

            var userIds = await GetAvailableRespondersIdsNearby(reportedEvent, radiusInKm)
;

            // No available responders found yet — signal consumer to continue searching
            if (userIds.Count == 0)
                return true;

            await AssignResponders(eventId, radiusInKm);
            return true;
        }

        private void SetServices(IServiceProvider provider)
        {
            // GetRequiredService throws early on misconfiguration (fail-fast vs silent null)
            eventRepository = provider.GetRequiredService<IEventCoordinatorRepository>();
            deviceRepository = provider.GetRequiredService<IDeviceRepository>();
            eventsConsumer = provider.GetRequiredService<IDomainEventConsumer>();
            notifier = provider.GetRequiredService<IRespondersNotifier>();
        }

        private async Task<ReportedEvent> TryUpdateEventState(string eventId)
        {
            var updated = await eventRepository.UpdateEvent(eventId, async (reportedEvent) =>
            {
                reportedEvent.UpdateEventStatus();

                // Await domain event dispatch — fixes previous fire-and-forget bug
                await eventsConsumer.Consume(reportedEvent.DomainEvents);
                reportedEvent.ClearDomainEvents();
            });

            return updated;
        }

        private async Task<List<string>> GetAvailableRespondersIdsNearby(ReportedEvent reportedEvent, double radiusInKm)
        {
            var users = await eventRepository.GetAvailableResponders(reportedEvent, radiusInKm)
;
            return users.Select(x => x.IdentityId).ToList();
        }

        private async Task AssignResponders(string eventId, double radiusInKm)
        {
            var userIds = new List<string>();

            Func<ReportedEvent, Task> updateEntity = async (reportedEvent) =>
            {
                userIds = await GetAvailableRespondersIdsNearby(reportedEvent, radiusInKm)
;

                // Index-based loop avoids enumerator allocation on List<string>
                for (var i = 0; i < userIds.Count; i++)
                {
                    var device = await deviceRepository.GetUserActiveDevice(userIds[i])
;
                    reportedEvent.AssignResponder(userIds[i], device?.Coordinates);
                }

                // Await domain event dispatch — fixes previous fire-and-forget bug
                await eventsConsumer.Consume(reportedEvent.DomainEvents);
                reportedEvent.ClearDomainEvents();
            };

            var currentEvent = await eventRepository.UpdateEvent(eventId, updateEntity)
;

            await NotifyResponders(userIds, currentEvent);
        }

        private async Task NotifyResponders(List<string> identityIds, ReportedEvent reportedEvent)
        {
            var tokens = await eventRepository.GetRespondersFirebaseTokens(identityIds)
;
            await notifier.Notify(tokens.ToArray(), reportedEvent);
        }
    }
}
