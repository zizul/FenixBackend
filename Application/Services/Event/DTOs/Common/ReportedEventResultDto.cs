using Domain.Enums;

namespace Application.Services.Event.DTOs.Common
{
    public class ReportedEventResultDto
    {
        public string Id { get; set; }
        public EventStatusType Status { get; set; }
        public List<ResponderResultDto> Responders { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ClosedAt { get; set; }

        // Data for AMR only
        public ReportedEventDto? Data { get; set; }
        public ReporterResultDto Reporter { get; set; }
    }
}
