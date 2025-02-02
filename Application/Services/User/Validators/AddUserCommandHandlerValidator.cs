using Application.Services.User.DTOs;
using FluentValidation;

namespace Application.Services.User.Validators
{
    internal class AddUserCommandValidator : AbstractValidator<AddUserCommandDto>
    {
        public AddUserCommandValidator()
        {
            RuleFor(x => x.UserData).SetValidator(new UserDataValidator());
        }
    }
}
