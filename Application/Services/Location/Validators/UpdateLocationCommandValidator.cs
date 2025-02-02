using Application.Services.Location.DTOs;
using Application.Services.Map.PointsOfInterest.Validators;
using FluentValidation;

namespace Application.Services.Location.Validators
{
    internal class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommandDto>
    {
        public UpdateLocationCommandValidator()
        {
            RuleFor(x => x.DeviceId).NotEmpty();
            RuleFor(x => x.DeviceLocation.Coordinates).SetValidator(new CoordinatesValidator());
        }
    }
}
