using MediatR;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class DeleteAedPhotoCommandDto : IRequest
    {
        public string AedId { get; }


        public DeleteAedPhotoCommandDto(string aedId)
        {
            AedId = aedId;
        }
    }
}