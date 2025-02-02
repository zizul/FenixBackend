using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using AutoMapper;
using Domain.Entities.User;

namespace Application.Services.User.Mappers
{
    internal class DeviceMappingProfile : Profile
    {
        public DeviceMappingProfile()
        {
            CreateMap<DeviceDto, Device>();
            CreateMap<Device, DeviceDto>();

            CreateMap<AddOrUpdateDeviceCommandDto, Device>()
                .IncludeMembers(src => src.Device);
        }
    }
}
