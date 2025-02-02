using Application.Services.Map.PointsOfInterest.Queries;
using Domain.ValueObjects;
using MediatR;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class GetMedicalPoiQueryDto : IRequest<GetMedicalPoiResultDto>
    {
        public Coordinates Coordinates { get; }
        public double? RangeInKm { get; }
        public List<PoiFilter> Filters { get; }
        public bool IsSortByDistance { get; }


        public GetMedicalPoiQueryDto(
            Coordinates coordinates, 
            double? rangeInKm, 
            List<PoiFilter> filters,
            bool isSortByDistance)
        {
            Coordinates = coordinates;
            RangeInKm = rangeInKm;
            Filters = filters;
            IsSortByDistance = isSortByDistance;
        }
    }
}
