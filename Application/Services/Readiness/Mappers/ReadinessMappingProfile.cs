using Application.Services.Readiness.DTOs;
using Application.Services.Readiness.DTOs.Common;
using AutoMapper;
using Domain.Entities.Readiness;

namespace Application.Services.Readiness.Mappers
{
    internal class ReadinessMappingProfile : Profile
    {
        public ReadinessMappingProfile()
        {
            CreateMap<UserReadinessDataDto, UserReadiness>();
            CreateMap<UserReadiness, UserReadinessDataDto>();

            CreateMap<UpdateReadinessCommandDto, UserReadiness>()
               .IncludeMembers(src => src.ReadinessData);
        }
    }
}
