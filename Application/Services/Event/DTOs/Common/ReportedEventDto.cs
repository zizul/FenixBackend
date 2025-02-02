using Domain.ValueObjects;

namespace Application.Services.Event.DTOs.Common
{
    public class ReportedEventDto
    {
        public Coordinates Coordinates { get; set; }
        public string? EventType { get; set; }
        public int? InjuredCount { get; set; }
        public Address? Address { get; set; }
        public string? Description { get; set; }
    }
}
