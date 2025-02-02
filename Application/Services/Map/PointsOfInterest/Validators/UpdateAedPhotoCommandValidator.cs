using Application.Services.Map.PointsOfInterest.DTOs;
using FluentValidation;

namespace Application.Services.Map.PointsOfInterest.Validators
{
    internal class UpdateAedPhotoCommandValidator : AbstractValidator<UpdateAedPhotoCommandDto>
    {
        public UpdateAedPhotoCommandValidator()
        {
            RuleFor(x => x.File).SetValidator(new ImageValidator());
        }
    }
}
