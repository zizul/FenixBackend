using Application.Services.Location.DTOs;
using Application.Services.Location.DTOs.Common;
using AutoMapper;
using Domain.Entities.Location;

namespace Application.Services.Location.Mappers
{
    internal class DeviceLocationMappingProfile : Profile
    {
        public DeviceLocationMappingProfile()
        {
            CreateMap<DeviceLocationDto, DeviceLocation>();

            CreateMap<UpdateLocationCommandDto, DeviceLocation>()
                .IncludeMembers(src => src.DeviceLocation)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DeviceId));
        }
    }
}
