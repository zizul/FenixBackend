using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class UpdateAedPhotoCommandDto : IRequest<string>
    {
        public string AedId { get; }
        public IFormFile File { get; }


        public UpdateAedPhotoCommandDto(string aedId, IFormFile file)
        {
            AedId = aedId;
            File = file;
        }
    }
}