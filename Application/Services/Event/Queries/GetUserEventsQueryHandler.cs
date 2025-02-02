using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using AutoMapper;
using MediatR;

namespace Application.Services.Event.Queries
{
    public class GetUserEventsQueryHandler : IRequestHandler<GetUserEventsQueryDto, GetUserEventsResultDto>
    {
        private readonly IReportedEventsRepository repository;
        private readonly IMapper mapper;


        public GetUserEventsQueryHandler(IReportedEventsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<GetUserEventsResultDto> Handle(GetUserEventsQueryDto request, CancellationToken cancellationToken)
        {
            var result = new GetUserEventsResultDto();

            result.ReportedEvents = mapper.Map<List<ReportedEventResultDto>>(await repository.GetReportedEvents(request.IdentityId, request.EventStatusTypes));
            result.ReportedEvents.ForEach(x => HandleReportedEvent(x));

            result.AssignedEvents = new List<ReportedEventResultDto>();

            if (request.IsResponderRole)
            {
                result.AssignedEvents.AddRange(mapper.Map<List<ReportedEventResultDto>>(await repository.GetAssignedEvents(request.IdentityId, request.EventStatusTypes)));
            }

            return result;
        }

        private ReportedEventResultDto HandleReportedEvent(ReportedEventResultDto result)
        {
            result.Responders.ForEach(x =>
            {
                x.Transport = null;
                x.Coordinates = null;
            });
            return result;
        }
    }
}
