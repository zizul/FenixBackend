using Application.Services.User.DTOs.Common;
using Application.Services.User.Validators;
using Domain.Enums;

namespace UnitTests.Application.User.Validators
{
    public class UserDataValidatorTests
    {
        private readonly UserDataValidator validator;


        public UserDataValidatorTests()
        {
            validator = new UserDataValidator();
        }

        [Theory]
        [InlineData("test@gmail.com", "+48-123-456-789", UserRoles.User)]
        [InlineData("test@gmail.com", "123456789", UserRoles.Responder)]
        public void Validate_Should_Valid(string email, string phone, UserRoles role)
        {
            var userData = GetUserData(email, phone, role);

            var result = validator.Validate(userData);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test.com")]
        public void Validate_Should_FailOnEmail(string email)
        {
            var userData = GetUserData(email, "+48-123456789", UserRoles.User);

            var result = validator.Validate(userData);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("asfasfasf")]
        public void Validate_Should_FailOnPhoneNumber(string phone)
        {
            var userData = GetUserData("test@gmail", phone, UserRoles.User);

            var result = validator.Validate(userData);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData((UserRoles)999)]
        public void Validate_Should_FailOnRole(UserRoles role)
        {
            var userData = GetUserData("test@gmail", "+48-123456789", role);

            var result = validator.Validate(userData);

            Assert.False(result.IsValid);
        }

        private UserDto GetUserData(string email, string phone, UserRoles role)
        {
            return new UserDto()
            {
                IdentityId = "123",
                FirstName = "test firstname",
                LastName = "test lastname",
                Email = email,
                IsEmailVerified = true,
                IsMobileNumberVerified = false,
                MobileNumber = phone,
                Role = role,
                Username = "test username",
            };
        }
    }
}
