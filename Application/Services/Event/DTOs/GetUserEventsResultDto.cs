using Application.Services.Event.DTOs.Common;

namespace Application.Services.Event.DTOs
{
    public class GetUserEventsResultDto
    {
        public List<ReportedEventResultDto> ReportedEvents { get; set; }
        public List<ReportedEventResultDto> AssignedEvents { get; set; }
    }
}
