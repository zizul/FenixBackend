using MediatR;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class DeleteAedPoiCommandDto : IRequest
    {
        public string Id { get; }


        public DeleteAedPoiCommandDto(string id)
        {
            Id = id;
        }
    }
}
