using Domain.Entities.Map;
using Domain.ValueObjects;

namespace Application.Services.Map.PointsOfInterest.Contracts
{
    public interface IPoiRepository
    {
        Task<List<Aed>> GetAeds(Coordinates Coordinates, double? rangeInKm, bool isSortByDistance);
        Task<List<EmergencyDepartment>> GetNiswols(Coordinates Coordinates, double? rangeInKm, bool isSortByDistance);
        Task<List<EmergencyDepartment>> GetSors(Coordinates Coordinates, double? rangeInKm, bool isSortByDistance);
    }
}
