using Domain.Entities.Readiness;
using Domain.Entities.User;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures
{
    public class ReadinessFixture : DatabaseFixture
    {
        public BasicUser ReadOnlyUser { get; set; }
        public BasicUser WriteOnlyUser { get; set; }
        public UserReadiness ReadOnlyReadiness { get; set; }
        public BasicUser UserWithoutReadiness { get; set; }


        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await InitCollections();
            await InitData();
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task InitCollections()
        {
            await CreateCollection(GlobalCollections.USERS);
            await CreateCollection(GlobalCollections.USER_READINESS);
        }

        private async Task InitData()
        {
            ReadOnlyUser = await CreateUser("id1", UserRoles.Responder);
            ReadOnlyReadiness = await CreateReadiness(
                    ReadOnlyUser.Id,
                    ReadinessStatus.Ready,
                    new ReadinessRange[]
                    {
                        new ReadinessRange(true, TimeSpan.FromHours(8), TimeSpan.FromHours(12), DayOfWeek.Monday)
                    });

            UserWithoutReadiness = await CreateUser("id2", UserRoles.Responder);

            WriteOnlyUser = await CreateUser("id3", UserRoles.Responder);
        }

        private async Task<BasicUser> CreateUser(string id, UserRoles role)
        {
            var user = new BasicUser
            {
                Username = $"test-{id}",
                IdentityId = $"identity-{id}",
                Role = role,
                MobileNumber = "123-456-789",
                ActiveDeviceId = $"{id}-device"
            };

            var result = await CreateDocument(GlobalCollections.USERS, user);
            return result;
        }

        private async Task<UserReadiness> CreateReadiness(string userId, ReadinessStatus readinessStatus, ReadinessRange[] ranges)
        {
            var readiness = new UserReadiness
            {
                UserId = userId,
                ReadinessStatus = readinessStatus,
                ReadinessRanges = ranges,
            };

            var result = await CreateDocument(GlobalCollections.USER_READINESS, readiness);
            return result;
        }
    }
}