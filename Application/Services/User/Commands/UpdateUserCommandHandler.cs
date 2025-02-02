using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using AutoMapper;
using Domain.Entities.User;
using MediatR;

namespace Application.Services.User.Commands
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommandDto, UserDto>
    {
        private readonly IUserRepository repository;
        private readonly IMapper mapper;


        public UpdateUserCommandHandler(IUserRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<UserDto> Handle(UpdateUserCommandDto request, CancellationToken cancellationToken)
        {
            var account = mapper.Map<BasicUser>(request);

            var updated = await repository.Update(request.UserId, account);

            var updatedDto = mapper.Map<UserDto>(updated);
            return updatedDto;
        }
    }
}