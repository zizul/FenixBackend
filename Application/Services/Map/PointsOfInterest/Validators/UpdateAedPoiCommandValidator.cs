using Application.Services.Map.PointsOfInterest.DTOs;
using FluentValidation;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class UpdateAedPoiCommandValidator : AbstractValidator<UpdateAedPoiCommandDto>
    {
        public UpdateAedPoiCommandValidator()
        {
            RuleFor(x => x.Aed).NotEmpty();
            RuleFor(x => x.Aed.Coordinates).SetValidator(new CoordinatesValidator());
        }
    }
}
