using Domain.Enums;

namespace Domain.Entities.User
{
    public class BasicUser
    {
        public string Id { get; set; }
        public string IdentityId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public UserRoles Role { get; set; }
        public bool IsBanned { get; set; } = false;
        public string? ActiveDeviceId { get; set; }
        public List<Device> Devices { get; set; } = new List<Device>();
    }
}
