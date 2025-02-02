using Application.Services.User.DTOs.Common;
using MediatR;

namespace Application.Services.User.DTOs
{
    public class UpdateUserCommandDto : IRequest<UserDto>
    {
        public string UserId { get; }
        public UserDto UserData { get; }


        public UpdateUserCommandDto(string userId, UserDto userData)
        {
            UserId = userId;
            UserData = userData;
        }
    }
}
