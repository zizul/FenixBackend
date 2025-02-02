using Domain.Entities.Event;
using Domain.Entities.User;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures
{
    public class ReportedEventFixture : DatabaseFixture
    {
        public Coordinates ReadOnlyLocation { get; } = new Coordinates(25, 15);
        public Coordinates WriteOnlyLocation { get; } = new Coordinates(1.5, 1.5);
        public Coordinates CancelLocation { get; } = new Coordinates(5, 5);
        public Coordinates LocationOutOfRange { get; } = new Coordinates(-220.5, 120);

        public Dictionary<Coordinates, List<ReportedEvent>> DbEvents { get; set; } = new();
        public Dictionary<BasicUser, List<ReportedEvent>> DbUserEvents { get; set; } = new();
        public Dictionary<Coordinates, List<BasicUser>> DbUsers { get; set; } = new();
        public Dictionary<Coordinates, List<BasicUser>> DbResponders { get; set; } = new();
        public ReportedEvent ReadOnlyReportedEvent { get; set; }
        public BasicUser ReadOnlyReporter { get; set; }
        public ReportedEvent[] ReportedEventsToCancel { get; set; }
        public BasicUser Reporter2 { get; set; }
        public string ResponderIdentityId = "2134-8478-3678";


        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await InitEventCollections();
            await InitDocuments();
        }

        private async Task InitDocuments()
        {
            var defaultUser = await CreateUser(TestAuthHandler.DefaultUserId, null);

            await AddEvents(ReadOnlyLocation, EventStatusType.Pending, 3);
            ReadOnlyReportedEvent = DbEvents[ReadOnlyLocation].Last();
            ReadOnlyReporter = DbUsers[ReadOnlyLocation].Last();

            await AddEvent(ReadOnlyLocation, EventStatusType.Completed, ReadOnlyReporter);
            await AddEvent(ReadOnlyLocation, EventStatusType.Accepted, ReadOnlyReporter);
            await AddEvent(ReadOnlyLocation, EventStatusType.Cancelled, ReadOnlyReporter);

            await AddEvents(CancelLocation, EventStatusType.Pending, 2);
            await AddEvents(CancelLocation, EventStatusType.Cancelled, 1);

            await AddEvents(WriteOnlyLocation, EventStatusType.Pending, 2);

            var reportedEvent = DbEvents[WriteOnlyLocation].Last();
            await AddNewResponder(reportedEvent, "123");

            // add events and responder for GETEventsAmr_Should_ReturnOkCode200 test
            await AddEvents(ReadOnlyLocation, EventStatusType.Pending, 1);
            var event1 = DbEvents[ReadOnlyLocation].Last();
            Reporter2 = DbUsers[ReadOnlyLocation].Last();

            await AddEvent(ReadOnlyLocation, EventStatusType.Completed, Reporter2);
            var event2 = DbEvents[ReadOnlyLocation].Last();

            await AddEvent(ReadOnlyLocation, EventStatusType.Accepted, Reporter2);
            var event3 = DbEvents[ReadOnlyLocation].Last();

            await AddEvent(ReadOnlyLocation, EventStatusType.Cancelled, Reporter2);
            var event4 = DbEvents[ReadOnlyLocation].Last();

            await AddNewResponder(event1, ResponderIdentityId);
            await AddResponder(event2, event1.Responders[0].UserId);
            await AddResponder(event3, event1.Responders[0].UserId);
            await AddResponder(event4, event1.Responders[0].UserId);
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task InitEventCollections()
        {
            await CreateCollection(GlobalCollections.EVENTS);
            await CreateCollection(GlobalCollections.EVENT_RESPONDERS);
            await CreateCollection(GlobalCollections.USER_DEVICES);
            await CreateCollection(GlobalCollections.USERS);
        }

        private async Task AddEvent(Coordinates coordinates, EventStatusType eventStatus, BasicUser user)
        {
            var result = await CreateEvent(coordinates, eventStatus, user);

            //add result to DbEvents
            if (DbEvents.ContainsKey(coordinates))
            {
                DbEvents[coordinates].Add(result);
            }
            else
            {
                DbEvents.Add(coordinates, new List<ReportedEvent>() { result });
            }

            //add result to DbUserEvents
            if (DbUserEvents.ContainsKey(user))
            {
                DbUserEvents[user].Add(result);
            }
            else
            {
                DbUserEvents.Add(user, new List<ReportedEvent>() { result });
            }
        }

        private async Task AddEvents(Coordinates coordinates, EventStatusType eventStatus, int count)
        {
            var users = new List<BasicUser>();
            var reportedEvents = new List<ReportedEvent>();
            for (int i = 0; i < count; i++)
            {
                var userId = Guid.NewGuid().ToString();
                var deviceId = Guid.NewGuid().ToString();
                var user = await CreateUser(userId, deviceId);
                await CreateDevice(deviceId, user.Id, coordinates);
                users.Add(user);

                var result = await CreateEvent(coordinates, eventStatus, user);
                reportedEvents.Add(result);

                //add event to DbUserEvents
                if (DbUserEvents.ContainsKey(user))
                {
                    DbUserEvents[user].Add(result);
                }
                else
                {
                    DbUserEvents.Add(user, new List<ReportedEvent>() { result });
                }
            }

            if (DbEvents.ContainsKey(coordinates))
            {
                DbEvents[coordinates].AddRange(reportedEvents);
            }
            else
            {
                DbEvents.Add(coordinates, reportedEvents);
            }

            if (DbUsers.ContainsKey(coordinates))
            {
                DbUsers[coordinates].AddRange(users);
            }
            else
            {
                DbUsers.Add(coordinates, users);
            }


        }

        private async Task<ReportedEvent> CreateEvent(Coordinates coordinates, EventStatusType status, BasicUser user)
        {
            var reportedEvent = new ReportedEvent()
            {
                Coordinates = coordinates,
                Description = "test",
                Reporter = new Reporter() { UserId = user.Id, IdentityId = user.IdentityId,  MobileNumber = "123-456-789" },
                Status = status,
            };

            var result = await CreateDocument(GlobalCollections.EVENTS, reportedEvent);
            result.Reporter.IdentityId = reportedEvent.Reporter.IdentityId;
            result.Reporter.MobileNumber = reportedEvent.Reporter.MobileNumber;
            return result;
        }

        private async Task AddNewResponder(ReportedEvent reportedEvent, string userId)
        {
            var deviceId = Guid.NewGuid().ToString();
            var user = await CreateUser(userId, deviceId);
            await CreateDevice(deviceId, user.Id, reportedEvent.Coordinates);
            var responderRef = user.Id;

            var responder = new Responder
            (
                eventId: reportedEvent.Id,
                identityId: user.IdentityId,
                status: ResponderStatusType.Pending,
                userId: responderRef
            );

            var result = await CreateDocument(GlobalCollections.EVENT_RESPONDERS, responder);
            result.IdentityId = user.IdentityId;

            reportedEvent.Responders.Add(result);
        }

        private async Task AddResponder(ReportedEvent reportedEvent, string userId)
        {
            var responder = new Responder
            (
                eventId: reportedEvent.Id,
                identityId: null,
                status: ResponderStatusType.Pending,
                userId: userId
            );

            var result = await CreateDocument(GlobalCollections.EVENT_RESPONDERS, responder);

            reportedEvent.Responders.Add(result);
        }

        private async Task<BasicUser> CreateUser(string userId, string activeDeviceId)
        {
            var user = new BasicUser()
            {
                IdentityId = userId,
                MobileNumber = "123-456-789",
                ActiveDeviceId = activeDeviceId,
                IsEmailVerified = true,
                IsMobileNumberVerified = true,
            };

            return await CreateDocument(GlobalCollections.USERS, user);
        }

        private async Task CreateDevice(string deviceId, string userRef, Coordinates coordinates)
        {
            var device = new Device()
            {
                DeviceId = deviceId,
                FirebaseToken = "test token",
                DeviceModel = "test model",
                UserId = userRef,
                Coordinates = coordinates,
            };

            await CreateDocument(GlobalCollections.USER_DEVICES, device);
        }
    }
}