using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using AutoMapper;
using MediatR;

namespace Application.Services.Event.Queries
{
    public class GetReportedEventByIdQueryHandler : IRequestHandler<GetReportedEventByIdQueryDto, ReportedEventResultDto>
    {
        private readonly IReportedEventsRepository repository;
        private readonly IMapper mapper;


        public GetReportedEventByIdQueryHandler(IReportedEventsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<ReportedEventResultDto> Handle(GetReportedEventByIdQueryDto request, CancellationToken cancellationToken)
        {
            var reportedEvent = await repository.Get(request.Id);
            var result = mapper.Map<ReportedEventResultDto>(reportedEvent);

            if (await repository.IsUserReporter(reportedEvent, request.IdentityId)) 
            {
                return HandleForReporter(result);
            }
            else if (request.IsResponderRole)
            {
                return HandleForResponder(result);
            }

            throw new ArgumentException("Wrong user role or user id");
        }

        // AMZ
        private ReportedEventResultDto HandleForReporter(ReportedEventResultDto result)
        {
            result.Reporter = null;
            result.Data = null;
            result.Responders.ForEach(x =>
            {
                x.Transport = null;
                x.Coordinates = null;
            });
            return result;
        }

        // AMR
        private ReportedEventResultDto HandleForResponder(ReportedEventResultDto result)
        {
            return result;
        }
    }
}