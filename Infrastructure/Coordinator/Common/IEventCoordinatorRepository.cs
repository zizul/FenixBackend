using Domain.Entities.Event;
using Domain.Entities.User;

namespace Infrastructure.Coordinator.Common
{
    internal interface IEventCoordinatorRepository
    {
        Task<ReportedEvent> UpdateEvent(string eventId, Action<ReportedEvent> updateEntity);
        Task<ReportedEvent> UpdateEvent(string eventId, Func<ReportedEvent, Task> updateEntity);
        Task<List<BasicUser>> GetAvailableResponders(ReportedEvent reportedEvent, double radiusInKm);
        Task<List<string>> GetRespondersFirebaseTokens(List<string> identityIds);
    }
}
