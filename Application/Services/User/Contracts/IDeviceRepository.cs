using Domain.Entities.User;

namespace Application.Services.User.Contracts
{
    public interface IDeviceRepository
    {
        Task<Device> AddOrUpdate(Device device);
        Task<Device?> TryGetDevice(string deviceId);
        Task<Device?> GetUserActiveDevice(string identityId);
    }
}
