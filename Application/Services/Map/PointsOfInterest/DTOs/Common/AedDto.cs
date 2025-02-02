using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Services.Map.PointsOfInterest.DTOs.Common
{
    public class AedDto
    {
        public Coordinates Coordinates { get; set; }
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
    }
}
