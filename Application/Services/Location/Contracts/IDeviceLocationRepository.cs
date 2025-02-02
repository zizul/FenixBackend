using Domain.Entities.Location;

namespace Application.Services.Location.Contracts
{
    public interface IDeviceLocationRepository
    {
        Task<DeviceLocation> Update(DeviceLocation device, bool isTransaction);
    }
}
