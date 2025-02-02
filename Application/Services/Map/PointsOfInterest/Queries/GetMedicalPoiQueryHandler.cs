using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using Application.Services.Map.PointsOfInterest.Queries;
using AutoMapper;
using Domain.Entities.Map;
using Domain.Enums;
using MediatR;

namespace Application.Map.PointsOfInterest.Queries
{
    public class GetMedicalPoiQueryHandler : IRequestHandler<GetMedicalPoiQueryDto, GetMedicalPoiResultDto>
    {
        private readonly IPoiRepository poiRepository;
        private readonly IMapper mapper;


        public GetMedicalPoiQueryHandler(IPoiRepository poiRepository, IMapper mapper)
        {
            this.poiRepository = poiRepository;
            this.mapper = mapper;
        }

        public async Task<GetMedicalPoiResultDto> Handle(GetMedicalPoiQueryDto request, CancellationToken cancellationToken)
        {
            var resultAeds = new List<Aed>();
            var resultSors = new List<EmergencyDepartment>();
            var resultNiswols = new List<EmergencyDepartment>();

            if (PoiFilter.IsIncludeFilter(request.Filters, PointOfInterestType.AED))
            {
                resultAeds = await poiRepository.GetAeds(request.Coordinates, request.RangeInKm, request.IsSortByDistance);
            }
            if (PoiFilter.IsIncludeFilter(request.Filters, PointOfInterestType.SOR))
            {
                resultSors = await poiRepository.GetSors(request.Coordinates, request.RangeInKm, request.IsSortByDistance);
            }
            if (PoiFilter.IsIncludeFilter(request.Filters, PointOfInterestType.NISWOL))
            {
                resultNiswols = await poiRepository.GetNiswols(request.Coordinates, request.RangeInKm, request.IsSortByDistance);
            }

            return GetResultDto(resultAeds, resultSors, resultNiswols);
        }

        private GetMedicalPoiResultDto GetResultDto(
            List<Aed> resultAeds, 
            List<EmergencyDepartment> resultSors,
            List<EmergencyDepartment> resultNiswols)
        {
            return new GetMedicalPoiResultDto()
            {
                Aeds = mapper.Map<List<AedResultDto>>(resultAeds),
                Sors = mapper.Map<List<EmergencyDepartmentResultDto>>(resultSors),
                Niswols = mapper.Map<List<EmergencyDepartmentResultDto>>(resultNiswols),
            };
        }
    }
}