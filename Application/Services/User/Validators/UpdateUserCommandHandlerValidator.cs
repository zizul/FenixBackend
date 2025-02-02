using Application.Services.User.DTOs;
using FluentValidation;

namespace Application.Services.User.Validators
{
    internal class UpdateUserCommandHandlerValidator : AbstractValidator<UpdateUserCommandDto>
    {
        public UpdateUserCommandHandlerValidator()
        {
            RuleFor(x => x.UserData).SetValidator(new UserDataValidator());
        }
    }
}
