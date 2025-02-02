using Domain.ValueObjects;

namespace Application.Services.Location.DTOs.Common
{
    public class DeviceLocationDto
    {
        public Coordinates Coordinates { get; set; }

        public DeviceLocationDto(Coordinates coordinates)
        {
            Coordinates = coordinates;
        }
    }
}
