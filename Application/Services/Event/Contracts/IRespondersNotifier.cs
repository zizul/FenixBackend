using Domain.Entities.Event;

namespace Application.Services.Event.Contracts
{
    public interface IRespondersNotifier
    {
        Task Notify(string[] firebaseTokens, ReportedEvent reportedEvent);
    }
}
