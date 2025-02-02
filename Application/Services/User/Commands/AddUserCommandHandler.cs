using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using AutoMapper;
using Domain.Entities.User;
using MediatR;

namespace Application.Services.User.Commands
{
    public class AddUserCommandHandler : IRequestHandler<AddUserCommandDto, UserDto>
    {
        private readonly IUserRepository repository;
        private readonly IMapper mapper;


        public AddUserCommandHandler(IUserRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<UserDto> Handle(AddUserCommandDto request, CancellationToken cancellationToken)
        {
            var account = mapper.Map<BasicUser>(request.UserData);

            var added = await repository.Add(account);

            var addedDto = mapper.Map<UserDto>(added);
            return addedDto;
        }
    }
}