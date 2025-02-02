using Domain.ValueObjects;

namespace Domain.Entities.Location
{
    public class DeviceLocation
    {
        public string Id { get; set; }
        public Coordinates Coordinates { get; set; }
    }
}
