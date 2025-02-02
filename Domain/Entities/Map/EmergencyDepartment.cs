using Domain.ValueObjects;

namespace Domain.Entities.Map
{
    public class EmergencyDepartment : PointOfInterest
    {
        public string DepartmentName { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; }
    }
}