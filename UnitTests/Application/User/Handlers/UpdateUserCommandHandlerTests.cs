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
    public class UpdateUserCommandHandlerTests
    {
        private readonly IUserRepository repositoryMock;
        private readonly IMapper mapper;


        public UpdateUserCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IUserRepository>();
            mapper = MapperUtils.CreateMapper<UserMappingProfile>();
            SetupRepository();
        }

        [Fact]
        public async Task Handle_Should_UpdateAccount()
        {
            var handler = new UpdateUserCommandHandler(repositoryMock, mapper);
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
            var command = new UpdateUserCommandDto("test username", user);

            var updated = await handler.Handle(command, default);

            Assert.Equivalent(user, updated);
            await repositoryMock
                .Received()
                .Update(
                    Arg.Is<string>(x => x == command.UserId), 
                    Arg.Is<BasicUser>(x => x.IdentityId == command.UserData.IdentityId));
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

            repositoryMock.Update("test username", Arg.Any<BasicUser>()).Returns(userToReturn);
        }
    }
}
