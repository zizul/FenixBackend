using Application.Services.Map.PointsOfInterest.DTOs.Common;

namespace Application.Services.Map.PointsOfInterest.DTOs
{
    public class GetMedicalPoiResultDto
    {
        public List<AedResultDto> Aeds { get; set; }
        public List<EmergencyDepartmentResultDto> Sors { get; set; }
        public List<EmergencyDepartmentResultDto> Niswols { get; set; }
    }
}
