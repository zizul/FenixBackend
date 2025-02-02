using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using AutoMapper;
using Domain.Entities.Event;

namespace Application.Services.Map.PointsOfInterest.Mappers
{
    internal class ReportedEventMappingProfile : Profile
    {
        public ReportedEventMappingProfile()
        {
            CreateMap<ReportedEventDto, ReportedEvent>();
            CreateMap<ReportedEvent, ReportedEventDto>();

            CreateMap<ResponderInputDto, Responder>();
            CreateMap<Responder, ResponderInputDto>();

            CreateEventResultMapping();
            CreateReportEventMapping();
        }

        private void CreateEventResultMapping()
        {
            CreateMap<Responder, ResponderResultDto>()
                .ForMember(dest => dest.Timeline, opt => opt.MapFrom(e => e.Timeline.Entries));

            CreateMap<Reporter, ReporterResultDto>();

            CreateMap<ReportedEvent, ReportedEventResultDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(e => e.Id))
               .ForMember(dest => dest.Data, opt => opt.MapFrom(e => e))
               .ForMember(dest => dest.Responders, opt => opt.MapFrom(e => e.Responders));
        }

        private void CreateReportEventMapping()
        {
            CreateMap<ReportEventCommandDto, ReportedEvent>()
               .IncludeMembers(src => src.ReportedEvent)
               .ForMember(dest => dest.Reporter, opt => opt.MapFrom(
                   src => new Reporter { UserId = src.IdentityId }));
        }
    }
}
