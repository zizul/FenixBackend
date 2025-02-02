using Application.Services.Event.Contracts;
using Domain.Entities.Event;
using Domain.Entities.User;
using Domain.Enums;
using Infrastructure.Persistance.Core;

namespace Infrastructure.Coordinator.Common
{
    internal class EventCoordinatorRepository : IEventCoordinatorRepository
    {
        private IReportedEventsRepository eventRepository;
        private IDocumentRepository<BasicUser> userRepository;
        private IDocumentRepository<object> deviceRepository;


        public EventCoordinatorRepository(
            IReportedEventsRepository eventRepository, 
            IDocumentRepository<BasicUser> userRepository,
            IDocumentRepository<object> deviceRepository)
        {
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
            this.deviceRepository = deviceRepository;
        }

        public async Task<ReportedEvent> UpdateEvent(string eventId, Action<ReportedEvent> updateEntity)
        {
            return await eventRepository.Update(eventId, updateEntity);
        }

        public async Task<ReportedEvent> UpdateEvent(string eventId, Func<ReportedEvent, Task> updateEntity)
        {
            return await eventRepository.Update(eventId, updateEntity);
        }

        public async Task<List<BasicUser>> GetAvailableResponders(ReportedEvent reportedEvent, double radiusInKm)
        {
            var vars = new Dictionary<string, object>()
            {
                { "coordinates", new double[2]
                    {
                        reportedEvent.Coordinates.Longitude,
                        reportedEvent.Coordinates.Latitude
                    }
                },
                { "radius", radiusInKm * 1_000 },
                { "eventId", reportedEvent.Id }
            };

            var availableRespondersQuery = GetAvailableRespondersQuery();

            var users = (await userRepository.Execute(availableRespondersQuery, vars)).ToList();
            return users;
        }

        private string GetAvailableRespondersQuery()
        {
            var timeNow = DateTime.UtcNow;

            return
                $"FOR u in {GlobalCollections.USERS} " +
                $"FILTER u.role == '{UserRoles.Responder}' " +
                $"{ResponderSearchHelper.IsResponderNearbyEventFilter("@coordinates", "@radius", "u")} " +
                $"{ResponderSearchHelper.IsResponderOnDutyFilter("u._key", timeNow.DayOfWeek, timeNow.TimeOfDay)} " +
                $"{ResponderSearchHelper.IsResponderNotAssignedToEventsFilter("@eventId", "u")} " +
                $"{ResponderSearchHelper.IsResponderNotCreatorOfEventFilter("@eventId", "u")} " +
                $"RETURN u";
        }

        public async Task<List<string>> GetRespondersFirebaseTokens(List<string> identityIds)
        {
            var vars = new Dictionary<string, object>()
            {
                { "identity_ids", identityIds.ToArray() }
            };

            var query =
                $"FOR identity_id IN @identity_ids " +
                $"FOR user in {GlobalCollections.USERS} " +
                $"FILTER user.identity_id == identity_id " +
                $"FOR d IN {GlobalCollections.USER_DEVICES} " +
                $"FILTER user._key == d.user_ref " +
                $"FILTER user.active_device_id == d.device_id " +
                $"RETURN d.firebase_token";

            List<string> tokens = (await deviceRepository.Execute(query, vars))
                .Select(x => x.ToString()!)
                .ToList();

            return tokens;
        }
    }
}
