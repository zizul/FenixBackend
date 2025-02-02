using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Map
{
    public class Aed : PointOfInterest
    {
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool? InDoor { get; set; }
        public string? Level { get; set; }
        public string? Phone { get; set; }
        public string? OpeningHours { get; set; }
        public string? Operator { get; set; }
        public AedAccessType Access { get; set; }
        public Address? Address { get; set; }
        public Availability? Availability { get; set; }
        public string? PhotoUrl { get; set; }
    }
}