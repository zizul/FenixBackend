using Application.Services.Map.PointsOfInterest.DTOs;
using FluentValidation;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class AddAedPoiCommandValidator : AbstractValidator<AddAedPoiCommandDto>
    {
        public AddAedPoiCommandValidator()
        {
            RuleFor(x => x.Aed).NotEmpty();
            RuleFor(x => x.Aed.Coordinates).SetValidator(new CoordinatesValidator());
        }
    }
}
