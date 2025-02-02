using Domain.Entities.Event;
using Domain.Enums;
using MediatR;

namespace Application.Services.Event.Contracts
{
    public interface IReportedEventsRepository
    {
        Task<string> GetRef(string identityId);
        Task<bool> IsUserReporter(ReportedEvent reportedEvent, string identityId);
        Task<ReportedEvent> Get(string id);
        Task<List<ReportedEvent>> GetReportedEvents(string identityId, List<EventStatusType> eventStatusTypes);
        Task<List<ReportedEvent>> GetAssignedEvents(string identityId, List<EventStatusType> eventStatusTypes);
        Task<ReportedEvent> Add(ReportedEvent reportedEvent, string identityId);
        Task<ReportedEvent> Update(string id, Action<ReportedEvent> updateEntity);
        Task<ReportedEvent> Update(string id, Func<ReportedEvent, Task> updateEntity);
        Task Delete(string id);
    }
}
