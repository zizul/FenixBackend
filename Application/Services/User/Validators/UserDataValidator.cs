using Application.Services.User.DTOs.Common;
using FluentValidation;

namespace Application.Services.User.Validators
{
    internal class UserDataValidator : AbstractValidator<UserDto>
    {
        public UserDataValidator()
        {
            RuleFor(x => x.Email).NotNull().EmailAddress();
            // regular expression for checking polish phone number
            RuleFor(x => x.MobileNumber).NotNull().Matches(@"(?<!\w)(\(?(\+|00)?48\)?)?[ -]?\d{3}[ -]?\d{3}[ -]?\d{3}(?!\w)");
            RuleFor(x => x.Role).NotNull().IsInEnum();
        }
    }
}
