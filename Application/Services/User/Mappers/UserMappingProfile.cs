using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using AutoMapper;
using Domain.Entities.User;

namespace Application.Services.User.Mappers
{
    internal class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserDto, BasicUser>();
            CreateMap<BasicUser, UserDto>();

            CreateAddUserMap();
            CreateUpdateUserMap();
        }

        private void CreateAddUserMap()
        {
            CreateMap<AddUserCommandDto, BasicUser>()
               .ForMember(result => result.IdentityId, conf => conf.MapFrom(src => src.UserData.IdentityId))
               .IncludeMembers(src => src.UserData);
        }

        private void CreateUpdateUserMap()
        {
            CreateMap<UpdateUserCommandDto, BasicUser>()
               .ForMember(result => result.IdentityId, conf => conf.MapFrom(src => src.UserData.IdentityId))
               .IncludeMembers(src => src.UserData);
        }
    }
}
