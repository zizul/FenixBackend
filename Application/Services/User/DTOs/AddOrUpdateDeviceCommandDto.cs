using Application.Services.User.DTOs.Common;
using MediatR;

namespace Application.Services.User.DTOs
{
    public class AddOrUpdateDeviceCommandDto : IRequest<DeviceDto>
    {
        public string UserId { get; }
        public DeviceDto Device { get; }


        public AddOrUpdateDeviceCommandDto(string userId, DeviceDto device)
        {
            UserId = userId;
            Device = device;
        }
    }
}
