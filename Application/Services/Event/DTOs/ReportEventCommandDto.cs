using Application.Services.Event.DTOs.Common;
using MediatR;

namespace Application.Services.Event.DTOs
{
    public class ReportEventCommandDto : IRequest<ReportedEventResultDto>
    {
        public string IdentityId { get; set; }
        public ReportedEventDto ReportedEvent { get; }


        public ReportEventCommandDto(string identityId, ReportedEventDto reportedEvent)
        {
            IdentityId = identityId;
            ReportedEvent = reportedEvent;
        }
    }
}
