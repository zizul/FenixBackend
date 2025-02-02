using Application.Services.Event.Contracts;
using Application.Services.User.Contracts;
using Domain.Entities.Event;
using Domain.Entities.User;
using Domain.Enums;
using Infrastructure.Persistance.Core;
using Newtonsoft.Json;

namespace Infrastructure.Persistance.Repositories
{
    internal class ReportedEventsRepository : IReportedEventsRepository
    {
        private readonly IDocumentRepository<ReportedEvent> eventRepository;
        private readonly IDocumentRepository<Responder> responderRepository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDocumentRepository<object> reporterRepository;
        private readonly ITransaction transaction;


        public ReportedEventsRepository(
            IDocumentRepository<ReportedEvent> eventRepository,
            IDocumentRepository<Responder> responderRepository,
            IDeviceRepository deviceRepository,
            IDocumentRepository<object> reporterRepository,
            ITransaction transaction)
        {
            this.eventRepository = eventRepository;
            this.responderRepository = responderRepository;
            this.deviceRepository = deviceRepository;
            this.reporterRepository = reporterRepository;
            this.transaction = transaction;
        }

        public async Task<string> GetRef(string identityId)
        {
            string keyRef = await GetUserProp(
                "doc.identity_id == @value", "_key", identityId);
            return keyRef;
        }

        public async Task<bool> IsUserReporter(ReportedEvent reportedEvent, string identityId)
        {
            string reporterIdentityId = await GetUserProp(
                "doc._key == @value", "identity_id", reportedEvent.Reporter.UserId);
            return reporterIdentityId == identityId;
        }

        public async Task<List<ReportedEvent>> GetReportedEvents(string identityId, List<EventStatusType> eventStatusTypes)
        {
            string userKey = await GetRef(identityId);

            var vars = new Dictionary<string, object>()
            {
                { "userKey", userKey }
            };

            var query =
                $"FOR event in {GlobalCollections.EVENTS} " +
                    $"FILTER event.reporter_ref == @userKey " +
                    GetEventStatusFilter(eventStatusTypes) +
                    $"LET responders = ({GetEventRespondersQuery("event._key")})" +
                $"RETURN MERGE(event, {{responders: responders}})";

            List<ReportedEvent> events = (await eventRepository.Execute(query, vars)).ToList();

            return events;
        }

        private string GetEventStatusFilter(List<EventStatusType> eventStatusFilters)
        {
            if (eventStatusFilters == null || eventStatusFilters.Count == 0)
            {
                return "";
            }
            return $"FILTER event.status in [{string.Join(',', eventStatusFilters.ToArray().Select(x => $"\"{x}\""))}]";
        }

        public async Task<List<ReportedEvent>> GetAssignedEvents(string identityId, List<EventStatusType> eventStatusTypes)
        {
            string userKey = await GetRef(identityId);

            var vars = new Dictionary<string, object>()
            {
                { "userKey", userKey }
            };

            var query =
                $"FOR event in {GlobalCollections.EVENTS} " +
                    $"FOR event_responder in {GlobalCollections.EVENT_RESPONDERS} " +
                    $"FILTER event_responder.event_ref == event._key " +
                    $"FILTER event_responder.responder_ref == @userKey " +
                    GetEventStatusFilter(eventStatusTypes) +
                    $"LET responders = ({GetEventRespondersQuery("event._key")})" + 
                $"RETURN MERGE(event, {{responders: responders}})";

            List<ReportedEvent> events = (await eventRepository.Execute(query, vars)).ToList();

            return events;
        }

        public async Task<ReportedEvent> Get(string id)
        {
            var reportedEvent = await eventRepository.Get(id, GlobalCollections.EVENTS);
            reportedEvent.Responders = await GetResponders(id);

            string mobileNumber = await GetUserProp(
                "doc._key == @value", "mobile_number", reportedEvent.Reporter.UserId);
            reportedEvent.Reporter.MobileNumber = mobileNumber;

            string identityId = await GetUserProp(
                "doc._key == @value", "identity_id", reportedEvent.Reporter.UserId);
            reportedEvent.Reporter.IdentityId = identityId;

            return reportedEvent;
        }

        public async Task<ReportedEvent> Add(ReportedEvent reportedEvent, string identityId)
        {
            // map identity id -> ref (FK)
            string keyRef = await GetRef(identityId);

            reportedEvent.Reporter = new Reporter()
            {
                UserId = keyRef
            };

            var addedEvent = await eventRepository.Add(reportedEvent, GlobalCollections.EVENTS);
            return addedEvent;
        }

        public Task<ReportedEvent> Update(string id, Action<ReportedEvent> updateEntity)
        {
            return Update(id, (reportedEvent) =>
            {
                updateEntity(reportedEvent);
                return Task.CompletedTask;
            });
        }

        public async Task<ReportedEvent> Update(string id, Func<ReportedEvent, Task> updateEntity)
        {
            var write = new string[]
            {
                GlobalCollections.EVENTS,
                GlobalCollections.EVENT_RESPONDERS
            };

            var updatedEvent = await transaction.Transact(async () =>
            {
                var reportedEvent = await Get(id);

                await updateEntity(reportedEvent);

                return await UpdateInDb(reportedEvent);
            }, exclusiveCollections: write);

            return updatedEvent;
        }

        public async Task Delete(string id)
        {
            await eventRepository.Delete(id, GlobalCollections.EVENTS);
        }

        public string GetEventRespondersQuery(string eventId)
        {
            var query =
                    $"FOR r IN {GlobalCollections.EVENT_RESPONDERS} " +
                    $"FILTER r.event_ref == {eventId} " +
                        // extract location from user devices
                        $"FOR u IN {GlobalCollections.USERS} FILTER u._key == r.responder_ref " +
                        $"FOR d IN {GlobalCollections.USER_DEVICES} FILTER u.active_device_id == d.device_id " +
                    $"RETURN merge(" +
                        $"[r, " +
                        $"{{location: d.location}}, " +
                        $"{{identity_id: u.identity_id}}, " +
                        $"{{name: u.first_name}}," +
                        $"{{surname: u.last_name}}," +
                        $"{{avatar_url: u.avatar_url}}])";
            return query;
        }

        private async Task<List<Responder>> GetResponders(string eventId)
        {
            var query = GetEventRespondersQuery("@id");

            var vars = new Dictionary<string, object>()
            {
                { "id", eventId }
            };

            return (await responderRepository.Execute(query, vars)).ToList();
        }

        private async Task<string> GetUserProp(string filter, string prop, string value)
        {
            var query =
                $"FOR doc IN {GlobalCollections.USERS} " +
                $"FILTER {filter} " +
                $"RETURN doc.{prop}";
            var vars = new Dictionary<string, object>()
            {
                { "value", value }
            };
            string result = (string)((await reporterRepository.Execute(query, vars)).SingleOrDefault());
            return result;
        }

        private async Task<ReportedEvent> UpdateInDb(ReportedEvent reportedEvent)
        {
            var responders = reportedEvent.Responders;
            foreach (var responder in responders)
            {
                await UpdateResponder(responder, reportedEvent.Id);
            }

            return await eventRepository.Update(reportedEvent.Id, GlobalCollections.EVENTS, reportedEvent);
        }

        private async Task UpdateResponder(Responder responder, string eventId)
        {
            var userRef = await GetRef(responder.IdentityId);
            responder.UserId = userRef;

            var responderJson = JsonConvert.SerializeObject(responder);

            var vars = new Dictionary<string, object>()
            {
                { "event_ref", eventId },
                { "responder_ref", userRef },
            };
            var query =
                $"UPSERT {{ event_ref: @event_ref, responder_ref: @responder_ref }} " +
                    $"INSERT {responderJson} " +
                    $"UPDATE {responderJson} " +
                    $"IN {GlobalCollections.EVENT_RESPONDERS}";

            await responderRepository.Execute(query, vars);
        }
    }
}
