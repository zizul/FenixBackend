using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using AutoMapper;
using Domain.Entities.Map;

namespace Application.Services.Map.PointsOfInterest.Mappers
{
    internal class PoiMappingProfile : Profile
    {
        public PoiMappingProfile()
        {
            CreateMap<AedDto, Aed>();
            CreateMap<Aed, AedDto>();

            CreateMap<EmergencyDepartmentResultDto, EmergencyDepartment>();
            CreateMap<EmergencyDepartment, EmergencyDepartmentResultDto>();

            CreateAddAedPoiMapping();
            UpdateAedPoiMapping();
        }

        private void CreateAddAedPoiMapping()
        {
            CreateMap<AddAedPoiCommandDto, Aed>()
               // use existing mapper for nested source property
               .IncludeMembers(src => src.Aed);

            CreateMap<Aed, AedResultDto>()
               // use existing mapper for nested target property
               .ForMember(result => result.Id, conf => conf.MapFrom(aed => aed.Id))
               .ForMember(result => result.Data, conf => conf.MapFrom(aed => aed));
        }

        private void UpdateAedPoiMapping()
        {
            CreateMap<UpdateAedPoiCommandDto, Aed>()
               .ForMember(result => result.Id, conf => conf.Ignore())
               // use existing mapper for nested source property
               .IncludeMembers(src => src.Aed);
        }
    }
}
