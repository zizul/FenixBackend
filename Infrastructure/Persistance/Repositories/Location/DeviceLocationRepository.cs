using Application.Exceptions;
using Application.Services.Location.Contracts;
using Domain.Entities.Location;
using Infrastructure.Persistance.Core;
using Newtonsoft.Json;

namespace Infrastructure.Persistance.Repositories.Location
{
    internal class DeviceLocationRepository : IDeviceLocationRepository
    {
        private readonly IDocumentRepository<DeviceLocation> deviceRepository;
        private readonly ITransaction transaction;


        public DeviceLocationRepository(IDocumentRepository<DeviceLocation> deviceRepository, ITransaction transaction)
        {
            this.deviceRepository = deviceRepository;
            this.transaction = transaction;
        }

        public async Task<DeviceLocation> Update(DeviceLocation device, bool isTransaction)
        {
            var json = JsonConvert.SerializeObject(device);

            var bindingParams = new Dictionary<string, object>() {
                { "device_id", device.Id }
            };

            Task<IEnumerable<DeviceLocation>> updateTask = deviceRepository.Execute(
                    $"FOR d IN {GlobalCollections.USER_DEVICES} " +
                    $"FILTER d.device_id == @device_id " +
                    $"UPDATE d WITH {json} IN {GlobalCollections.USER_DEVICES} " +
                    $"RETURN NEW",
                    bindingParams);

            IEnumerable<DeviceLocation> results;
            if (isTransaction)
            {
                // update with transaction
                var collectionToLock = new string[]
                {
                    GlobalCollections.USER_DEVICES
                };

                results = await transaction.Transact(async () =>
                {
                    return await updateTask;
                }, exclusiveCollections: collectionToLock);
            }
            else
            {
                // update without transaction
                results = await updateTask;
            }

            if (!results.Any())
            {
                throw ResourceNotFoundException.WithId<DeviceLocation>(device.Id);
            }

            return device;
        }
    }
}
