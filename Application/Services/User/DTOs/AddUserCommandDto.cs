using Application.Services.User.DTOs.Common;
using MediatR;

namespace Application.Services.User.DTOs
{
    public class AddUserCommandDto : IRequest<UserDto>
    {
        public UserDto UserData { get; }


        public AddUserCommandDto(UserDto user)
        {
            UserData = user;
        }
    }
}
