using Application.Common;
using Application.Services.Event.Contracts;
using Application.Services.User.Contracts;
using Domain.Entities.Event;
using Infrastructure.Coordinator.Common;
using Infrastructure.Persistance.Repositories.User;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Coordinator
{
    internal class EventCoordinatorService : IEventCoordinatorService
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        // scoped services
        private IEventCoordinatorRepository eventRepository;
        private IDeviceRepository deviceRepository;
        private IDomainEventConsumer eventsConsumer;
        private IRespondersNotifier notifier;


        public EventCoordinatorService(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task TryFindAndAssignRespondersToEvent(string eventId, double radiusInKm)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                SetServices(scope.ServiceProvider);

                var reportedEvent = await TryUpdateEventState(eventId);

                var userIds = await GetAvailableRespondersIdsNearby(reportedEvent, radiusInKm);
                if (!userIds.Any())
                {
                    return;
                }

                await AssignResponders(eventId, radiusInKm);
            }
        }

        private void SetServices(IServiceProvider provider)
        {
            eventRepository = provider.GetService<IEventCoordinatorRepository>()!;
            deviceRepository = provider.GetService<IDeviceRepository>()!;
            eventsConsumer = provider.GetService<IDomainEventConsumer>()!;
            notifier = provider.GetService<IRespondersNotifier>()!;
        }

        private async Task<ReportedEvent> TryUpdateEventState(string eventId)
        {
            Action<ReportedEvent> updateEntity = (reportedEvent) => 
                {
                    reportedEvent.UpdateEventStatus();
                    eventsConsumer.Consume(reportedEvent.DomainEvents);
                };

            var updated = await eventRepository.UpdateEvent(eventId, updateEntity);

            return updated;
        }

        private async Task<List<string>> GetAvailableRespondersIdsNearby(ReportedEvent reportedEvent, double radiusInKm)
        {
            var users = await eventRepository.GetAvailableResponders(reportedEvent, radiusInKm);
            return users.Select(x => x.IdentityId).ToList();
        }

        private async Task AssignResponders(string eventId, double radiusInKm)
        {
            var userIds = new List<string>();

            Func<ReportedEvent, Task> updateEntity = async (reportedEvent) =>
            {
                userIds = await GetAvailableRespondersIdsNearby(reportedEvent, radiusInKm);
                foreach (var userId in userIds)
                {
                    var device = await deviceRepository.GetUserActiveDevice(userId);
                    reportedEvent.AssignResponder(userId, device?.Coordinates);
                }

                eventsConsumer.Consume(reportedEvent.DomainEvents);
            };
            var currentEvent = await eventRepository.UpdateEvent(eventId, updateEntity);

            await NotifyResponders(userIds, currentEvent);
        }

        private async Task NotifyResponders(List<string> identityIds, ReportedEvent reportedEvent)
        {
            var tokens = await eventRepository.GetRespondersFirebaseTokens(identityIds);
            await notifier.Notify(tokens.ToArray(), reportedEvent);
        }
    }
}
