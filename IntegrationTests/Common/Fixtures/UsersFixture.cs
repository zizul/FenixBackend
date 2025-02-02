using Domain.Entities.Readiness;
using Domain.Entities.User;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures
{
    public class UsersFixture : DatabaseFixture
    {
        public const string ReadOnlyIdentityId = "id_1";
        public const string AddedEmail = "test@test.com";
        public const string AddedPhone = "123456789";
        public const string AddedUsername = "added-username";

        public const string IdentityIdToUpdate1 = "id_2";
        public const string IdentityIdToUpdate2 = "id_3";
        public const string IdentityIdToDelete = "id_4";

        public const string AddedDeviceId = "test_device_id";
        public readonly static string[] DevicesIdToDelete = new string[] { "d1", "d2", "d3" };


        public UsersFixture()
        {
        }

        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await InitCollections();
            await InitAccounts();
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task InitCollections()
        {
            await CreateCollection(GlobalCollections.USERS);
            await CreateCollection(GlobalCollections.USER_DEVICES);
            await CreateCollection(GlobalCollections.USER_READINESS);
        }

        private async Task InitAccounts()
        {
            var userToRead = await AddAccount(ReadOnlyIdentityId, AddedDeviceId, AddedEmail, AddedPhone, AddedUsername);
            var userToDelete = await AddAccount(IdentityIdToDelete, "d2", "delete@t.com", "111222333", "d-username");
            await AddAccount(IdentityIdToUpdate1, "d-update1", "update@t1.com", "156789123", "u1-username");
            await AddAccount(IdentityIdToUpdate2, "d-update2", "update@t2.com", "256789123", "u2-username");

            await AddDevice(AddedDeviceId, userToRead.Id);

            foreach (var id in DevicesIdToDelete)
            {
                await AddDevice(id, userToDelete.Id);
            }
            await AddReadiness(userToDelete.Id);
        }

        private async Task<BasicUser> AddAccount(
            string identityId, string activeDeviceId, string email, string phone, string username)
        {
            var account = new BasicUser()
            {
                FirstName = "test",
                LastName = "test",
                MobileNumber = phone,
                IdentityId = identityId,
                Username = username,
                Email = email,
                ActiveDeviceId = activeDeviceId,
                Role = UserRoles.User,
                IsBanned = false,
                IsMobileNumberVerified = true,
                IsEmailVerified = true,
            };

            var created = await CreateDocument(GlobalCollections.USERS, account);
            return created;
        }

        private async Task AddDevice(string deviceId, string userRef)
        {
            var device = new Device()
            {
                DeviceId = deviceId,
                FirebaseToken = "test token",
                DeviceModel = "test model",
                UserId = userRef,
            };

            await CreateDocument(GlobalCollections.USER_DEVICES, device);
        }

        private async Task AddReadiness(string userId)
        {
            var readiness = new UserReadiness()
            {
                ReadinessStatus = ReadinessStatus.Ready,
                UserId = userId,
                ReadinessRanges = new ReadinessRange[0],
            };

            await CreateDocument(GlobalCollections.USER_READINESS, readiness);
        }

        internal async Task<bool> AreDevicesExist(string[] devicesIds)
        {
            foreach (var id in devicesIds)
            {
                bool isExists = await IsExists(
                    $"FOR d IN {GlobalCollections.USER_DEVICES} " +
                    $"FILTER d.device_id == '{id}' " +
                    $"RETURN d");

                if (!isExists)
                {
                    return false;
                }
            }

            return true;
        }

        internal async Task<bool> IsReadinessExists(string userId)
        {
            bool isExists = await IsExists(
                $"FOR ur IN {GlobalCollections.USER_READINESS} " +
                $"FILTER ur.user_ref == '{userId}' " +
                $"RETURN ur");

            if (!isExists)
            {
                return false;
            }

            return true;
        }

        internal async Task<bool> IsActiveDeviceIdIsSet(string deviceId, string identityId)
        {
            return await IsExists(
                    $"FOR u IN {GlobalCollections.USERS} " +
                    $"FILTER (u.identity_id == '{identityId}' && u.active_device_id == '{deviceId}') " +
                    $"RETURN u");

        }
    }
}