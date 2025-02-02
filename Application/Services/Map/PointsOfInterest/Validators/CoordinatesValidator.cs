using Domain.ValueObjects;
using FluentValidation;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class CoordinatesValidator : AbstractValidator<Coordinates>
    {
        public CoordinatesValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .Must(InCorrectRange)
                .WithMessage("Invalid coordinates. Value is out of range [-180, 180]");
        }

        private bool InCorrectRange(Coordinates x)
        {
            return x.Longitude > -180 && x.Longitude < 180 &&
                   x.Latitude > -180 && x.Latitude < 180;
        }
    }
}
