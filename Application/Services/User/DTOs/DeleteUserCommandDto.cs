using MediatR;

namespace Application.Services.User.DTOs
{
    public class DeleteUserCommandDto : IRequest
    {
        public string UserId { get; }


        public DeleteUserCommandDto(string userId)
        {
            UserId = userId;
        }
    }
}
