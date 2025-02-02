using Application.Services.Map.PointsOfInterest.DTOs.Common;
using MediatR;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class AddAedPoiCommandDto : IRequest<AedResultDto>
    {
        public AedDto Aed { get; }


        public AddAedPoiCommandDto(AedDto aed)
        {
            Aed = aed;
        }
    }
}
