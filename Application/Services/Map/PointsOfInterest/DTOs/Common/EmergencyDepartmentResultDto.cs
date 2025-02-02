using Domain.ValueObjects;

namespace Application.Services.Map.PointsOfInterest.DTOs.Common
{
    public class EmergencyDepartmentResultDto
    {
        public int Id { get; set; }
        public Coordinates Coordinates { get; set; }
        public string DepartmentName { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; }
    }
}
