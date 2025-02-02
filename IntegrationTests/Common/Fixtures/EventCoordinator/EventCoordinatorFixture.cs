using ArangoDBNetStandard.DocumentApi.Models;
using Domain.Entities.Event;
using Domain.Entities.Readiness;
using Domain.Entities.User;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures.EventCoordinator
{
    public class EventCoordinatorFixture : DatabaseFixture
    {
        public static Coordinates LocationA { get; } = new Coordinates(50, 50);
        public static Coordinates LocationB { get; } = new Coordinates(-50, -50);
        public static Coordinates LocationC { get; } = new Coordinates(0, 0);

        public List<BasicUser> DbReporters { get; set; }
        public Dictionary<Coordinates, List<BasicUser>> DbResponders { get; set; }
        public Dictionary<Coordinates, List<BasicUser>> DbAvailableResponders { get; set; }


        public EventCoordinatorFixture()
        {
            DbResponders = new Dictionary<Coordinates, List<BasicUser>>()
            {
                { LocationA, new List<BasicUser>() },
                { LocationB, new List<BasicUser>() },
                { LocationC, new List<BasicUser>() },
            };

            DbAvailableResponders = new Dictionary<Coordinates, List<BasicUser>>()
            {
                { LocationA, new List<BasicUser>() },
                { LocationB, new List<BasicUser>() },
                { LocationC, new List<BasicUser>() },
            };

            DbReporters = new List<BasicUser>();
        }

        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await InitEventCollections();

            await InitData();
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
            await CreateCollection(GlobalCollections.USER_READINESS);
        }

        protected virtual async Task InitData()
        {
            await InitUsers();
            await InitReadiness();
        }

        private async Task InitUsers()
        {
            var defaultUser = await CreateUser(TestAuthHandler.DefaultUserId, UserRoles.User);

            var user1 = await CreateUser("0", UserRoles.User);
            var user2 = await CreateUser("1", UserRoles.User);
            DbReporters.Add(user1);
            DbReporters.Add(user2);

            // location A available (6)
            await AddResponders(LocationA, 1);
            await AddResponders(LocationA, 2, "123", ResponderStatusType.Rejected);
            await AddResponders(LocationA, 1, "456", ResponderStatusType.Completed);
            await AddResponders(LocationA, 2, "456", ResponderStatusType.Rejected);

            // location A assigned (9)
            await AddResponders(LocationA, 2, "123", ResponderStatusType.Pending);
            await AddResponders(LocationA, 3, "123", ResponderStatusType.Accepted);
            await AddResponders(LocationA, 1, "123", ResponderStatusType.Arrived);
            await AddResponders(LocationA, 3, "456", ResponderStatusType.Pending);

            // location B available (6)
            await AddResponders(LocationB, 1);
            await AddResponders(LocationB, 2, "789", ResponderStatusType.Completed);
            await AddResponders(LocationB, 3, "789", ResponderStatusType.Incompleted);

            // location C available (1)
            await AddResponders(LocationC, 1);

            // location C assigned (5)
            await AddResponders(LocationC, 2, "0123", ResponderStatusType.Accepted);
            await AddResponders(LocationC, 3, "0123", ResponderStatusType.Arrived);
        }

        protected async Task AddResponders(
            Coordinates coordinates,
            int count,
            string? eventId = null,
            ResponderStatusType? status = null)
        {
            var addedUsers = new List<BasicUser>();
            for (int i = 0; i < count; i++)
            {
                string id = Guid.NewGuid().ToString();
                var user = await CreateUser(id, UserRoles.Responder);
                await CreateDevice(user.Id, user.ActiveDeviceId!, GetCoordinatesNearby(coordinates));

                await TryAddResponderToDb(coordinates, user, status, eventId);

                addedUsers.Add(user);
            }

            DbResponders[coordinates].AddRange(addedUsers);
        }

        protected async Task<BasicUser> CreateUser(string id, UserRoles role)
        {
            var user = new BasicUser
            {
                Username = $"test-{id}",
                IdentityId = $"identity-{id}",
                Role = role,
                MobileNumber = "123-456-789",
                ActiveDeviceId = $"{id}-device",
                IsEmailVerified = true,
                IsMobileNumberVerified = true,
            };

            var result = await CreateDocument(GlobalCollections.USERS, user);
            return result;
        }

        private async Task<Device> CreateDevice(string userId, string deviceId, Coordinates coordinates)
        {
            var device = new Device
            {
                UserId = $"{userId}",
                DeviceModel = $"Model",
                FirebaseToken = $"Token",
                DeviceId = deviceId,
                Coordinates = coordinates,
            };

            var result = await CreateDocument(GlobalCollections.USER_DEVICES, device);
            return result;
        }

        private async Task TryAddResponderToDb(
            Coordinates coordinates, BasicUser user, ResponderStatusType? status, string? eventId)
        {
            if (!status.HasValue)
            {
                DbAvailableResponders[coordinates].Add(user);
                return;
            }

            var responder = await CreateResponder(
                user.Id, user.IdentityId, eventId!, status.Value, GetCoordinatesNearby(coordinates));

            if (!responder.IsActiveOnCall())
            {
                DbAvailableResponders[coordinates].Add(user);
            }
        }

        private Coordinates GetCoordinatesNearby(Coordinates coordinates)
        {
            // in range of 5km (search radius)
            return new Coordinates(
                coordinates.Longitude - 0.0001,
                coordinates.Latitude + 0.0001);
        }

        private async Task<Responder> CreateResponder(
            string userId, string identityId, string eventId, ResponderStatusType status, Coordinates coordinates)
        {
            var responder = new Responder
            (
                eventId,
                identityId,
                status,
                coordinates: coordinates,
                userId: userId
            );

            var result = await CreateDocument(GlobalCollections.EVENT_RESPONDERS, responder);
            return result;
        }

        private async Task InitReadiness()
        {
            // full time readiness
            foreach (var responders in DbResponders.Values)
            {
                foreach (var responder in responders)
                {
                    await AddReadinessInDb(responder.Id, ReadinessStatus.Ready, new ReadinessRange[0]);
                }
            }
        }

        protected async Task AddReadinessInDb(string userId, ReadinessStatus readinessStatus, ReadinessRange[] ranges)
        {
            var readiness = new UserReadiness()
            {
                UserId = userId,
                ReadinessStatus = readinessStatus,
                ReadinessRanges = ranges
            };

            var result = await CreateDocument(GlobalCollections.USER_READINESS, readiness);
        }

        /// <summary>
        /// Used to simulate time passing
        /// </summary>
        internal async Task ExpireEventCreatedAt(string eventId)
        {
            var document = context.Client.Document;

            var pastDate = DateTime.UtcNow.AddHours(-5);
            object update = new { created_date = pastDate };

            await document.PatchDocumentAsync<object, object>(
                GlobalCollections.EVENTS,
                eventId,
                update,
                new PatchDocumentQuery()
                {
                    ReturnNew = true
                });
        }
    }
}