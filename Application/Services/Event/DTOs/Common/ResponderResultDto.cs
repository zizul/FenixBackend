using Domain.Common;
using Domain.Entities.Event;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Services.Event.DTOs.Common
{
    public class ResponderResultDto
    {
        public string IdentityId { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public ResponderStatusType Status { get; set; }
        public DateTime? ETA { get; set; }

        // Data for AMR only
        public TransportType? Transport { get; set; }
        public Coordinates? Coordinates { get; set; }
        public List<ResponderTimelineEntry> Timeline { get; set; }  
    }
}
