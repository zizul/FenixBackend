using Application.Services.Map.PointsOfInterest.DTOs;
using FluentValidation;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class GetMedicalPoiQueryValidator : AbstractValidator<GetMedicalPoiQueryDto>
    {
        public GetMedicalPoiQueryValidator()
        {
            RuleFor(x => x.Coordinates).SetValidator(new CoordinatesValidator());
        }
    }
}
