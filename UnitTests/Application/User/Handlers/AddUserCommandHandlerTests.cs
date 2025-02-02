using Application.Services.User.Commands;
using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using Application.Services.User.Mappers;
using AutoMapper;
using Domain.Entities.User;
using NSubstitute;
using UnitTests.Utils;

namespace UnitTests.Application.User.Handlers
{
    public class AddUserCommandHandlerTests
    {
        private readonly IUserRepository repositoryMock;
        private readonly IMapper mapper;


        public AddUserCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IUserRepository>();
            mapper = MapperUtils.CreateMapper<UserMappingProfile>();
            SetupRepository();
        }

        [Fact]
        public async Task Handle_Should_AddAccount()
        {
            var handler = new AddUserCommandHandler(repositoryMock, mapper);
            var user = new UserDto
            {
                Username = "test username",
                IdentityId = "test id",
                FirstName = "test",
                MobileNumber = "123 456 789",
                LastName = "test",
                IsEmailVerified = true,
                IsMobileNumberVerified = true,
            };
            var command = new AddUserCommandDto(user);

            var added = await handler.Handle(command, default);

            Assert.Equivalent(user, added);
            await repositoryMock
                .Received()
                .Add(Arg.Is<BasicUser>(x => x.IdentityId == command.UserData.IdentityId));
        }

        private void SetupRepository()
        {
            var userToReturn = new BasicUser()
            {
                Id = "15",
                Username = "test username",
                IdentityId = "test id",
                FirstName = "test",
                MobileNumber = "123 456 789",
                LastName = "test",
                IsEmailVerified = true,
                IsMobileNumberVerified = true,
            };

            repositoryMock.Add(Arg.Any<BasicUser>()).Returns(userToReturn);
        }
    }
}
