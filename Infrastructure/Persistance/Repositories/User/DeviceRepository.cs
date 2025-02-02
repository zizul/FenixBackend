using Application.Exceptions;
using Application.Services.User.Contracts;
using Domain.Entities.User;
using Infrastructure.Persistance.Core;

namespace Infrastructure.Persistance.Repositories.User
{
    internal class DeviceRepository : IDeviceRepository
    {
        private readonly IDocumentRepository<object> userRepository;
        private readonly IDocumentRepository<Device> devicesRepository;
        private readonly ITransaction transaction;


        public DeviceRepository(
            IDocumentRepository<object> userRepository,
            IDocumentRepository<Device> devicesRepository,
            ITransaction transaction)
        {
            this.userRepository = userRepository;
            this.devicesRepository = devicesRepository;
            this.transaction = transaction;
        }

        public async Task<Device> AddOrUpdate(Device device)
        {
            var write = new string[] 
            { 
                GlobalCollections.USERS,
                GlobalCollections.USER_DEVICES 
            };

            var addedOrUpdated = await transaction.Transact(async () =>
            {
                return await AddOrUpdateInDb(device);
            }, exclusiveCollections: write);

            return addedOrUpdated;
        }

        public async Task<Device> AddOrUpdateInDb(Device device)
        {
            var userRef = await GetUserProperty(device.UserId, "_key");
            device.UserId = userRef;

            var resource = await AddOrUpdateDevice(device);
            await SetUserActiveDeviceId(userRef, resource);

            return resource;
        }

        private async Task<string> GetUserProperty(string identityId, string propertyName)
        {
            var bindingVars = new Dictionary<string, object>()
            {
                { "identity_id", identityId }
            };

            try
            {
                string userProperty = (string)((await userRepository.Execute(
                    $"FOR u IN {GlobalCollections.USERS} " +
                    $"FILTER u.identity_id == @identity_id " +
                    $"RETURN u.{propertyName}",
                    bindingVars))
                    .Single());

                return userProperty;
            }
            catch (InvalidOperationException e)
            {
                throw ResourceNotFoundException.WithId<BasicUser>(identityId);
            }
        }

        public async Task<Device?> GetUserActiveDevice(string identityId)
        {
            string activeDeviceId = await GetUserProperty(identityId, "active_device_id");
            return await TryGetDevice(activeDeviceId);
        }

        private async Task<Device> AddOrUpdateDevice(Device device)
        {
            var fromDb = await TryGetDevice(device.DeviceId);
            if (fromDb != null)
            {
                return await devicesRepository.Update(fromDb.Id, GlobalCollections.USER_DEVICES, device);
            }
            else
            {
                return await devicesRepository.Add(device, GlobalCollections.USER_DEVICES);
            }
        }

        public async Task<Device?> TryGetDevice(string deviceId)
        {
            var bindingVars = new Dictionary<string, object>()
            {
                { "device_id", deviceId }
            };

            var foundDevices = await devicesRepository.Execute(
                $"FOR d IN {GlobalCollections.USER_DEVICES} " +
                $"FILTER d.device_id == @device_id " +
                $"RETURN d",
                bindingVars);

            if (foundDevices.Any())
            {
                return foundDevices.Single();
            }

            return null;
        }

        private async Task SetUserActiveDeviceId(string userRef, Device device)
        {
            await userRepository.Update(
                userRef, GlobalCollections.USERS, new { active_device_id = device.DeviceId });
        }
    }
}
