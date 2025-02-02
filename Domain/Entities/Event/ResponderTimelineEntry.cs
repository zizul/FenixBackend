using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Event
{
    public class ResponderTimelineEntry : ICloneable
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ResponderStatusType? Status { get; set; }

        public TransportType? Transport { get; set; }
        public DateTime? ETA { get; set; }
        public Coordinates? Coordinates { get; set; }

        public ResponderTimelineEntry()
        {
        }

        public ResponderTimelineEntry(ResponderStatusType status, TransportType? transportType, DateTime? eta, Coordinates? coordinates)
        {
            Status = status;
            Transport = transportType;
            ETA = eta;
            Coordinates = coordinates;
        }

        public ResponderTimelineEntry(ResponderTimelineEntry entry)
        {
            Status = entry.Status;
            Transport = entry.Transport;
            ETA = entry.ETA;
            Coordinates = entry.Coordinates;
        }

        public ResponderTimelineEntry Clone()
        {
            return new ResponderTimelineEntry(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
