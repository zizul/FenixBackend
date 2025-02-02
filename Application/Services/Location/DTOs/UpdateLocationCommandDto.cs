using Application.Services.Location.DTOs.Common;
using Domain.ValueObjects;
using MediatR;

namespace Application.Services.Location.DTOs
{
    public class UpdateLocationCommandDto : IRequest
    {
        public string DeviceId { get; set; }
        public DeviceLocationDto DeviceLocation { get; set; }
        public bool IsTransaction { get; set; }

        public UpdateLocationCommandDto(string deviceId, DeviceLocationDto deviceLocation, bool isTransaction = false)
        {
            DeviceId = deviceId;
            DeviceLocation = deviceLocation;
            IsTransaction = isTransaction;
        }
    }
}
