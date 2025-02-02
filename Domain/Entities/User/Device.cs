using Domain.ValueObjects;

namespace Domain.Entities.User
{
    public class Device
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string FirebaseToken { get; set; }
        public string DeviceModel { get; set; }
        public Coordinates? Coordinates { get; set; }
    }
}
