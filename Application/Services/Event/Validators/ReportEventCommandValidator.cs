using Application.Services.Event.DTOs;
using Application.Services.Map.PointsOfInterest.Validators;
using FluentValidation;

namespace Application.Services.Event.Validators
{
    internal class ReportEventCommandValidator : AbstractValidator<ReportEventCommandDto>
    {
        public ReportEventCommandValidator()
        {
            RuleFor(x => x.ReportedEvent.Coordinates).SetValidator(new CoordinatesValidator());
        }
    }
}
