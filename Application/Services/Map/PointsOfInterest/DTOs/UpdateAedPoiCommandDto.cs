using Application.Services.Map.PointsOfInterest.DTOs.Common;
using MediatR;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class UpdateAedPoiCommandDto : IRequest<AedResultDto>
    {
        public string Id { get; }
        public AedDto Aed { get; }


        public UpdateAedPoiCommandDto(string id, AedDto aed)
        {
            Id = id;
            Aed = aed;
        }
    }
}
