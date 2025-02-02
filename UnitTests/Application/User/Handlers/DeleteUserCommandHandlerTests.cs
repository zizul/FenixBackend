using Application.Services.User.Commands;
using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.Mappers;
using AutoMapper;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.User.Handlers
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly IUserRepository repositoryMock;


        public DeleteUserCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IUserRepository>();
        }

        [Fact]
        public async Task Handle_Should_DeleteAccount()
        {
            var handler = new DeleteUserCommandHandler(repositoryMock);
            var command = new DeleteUserCommandDto("test username");

            await handler.Handle(command, default);

            await repositoryMock
                .Received()
                .Delete(Arg.Is(command.UserId));
        }
    }
}
