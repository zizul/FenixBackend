using Domain.Enums;

namespace Application.Services.User.DTOs.Common
{
    public class UserDto
    {
        public string IdentityId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public bool IsMobileNumberVerified { get; set; }
        public UserRoles Role { get; set; }
        public string? ActiveDeviceId { get; set; }
    }
}
