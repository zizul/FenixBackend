using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using MediatR;

namespace Application.Services.User.Commands
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandDto>
    {
        private readonly IUserRepository repository;


        public DeleteUserCommandHandler(IUserRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(DeleteUserCommandDto request, CancellationToken cancellationToken)
        {
            await repository.Delete(request.UserId);
        }
    }
}