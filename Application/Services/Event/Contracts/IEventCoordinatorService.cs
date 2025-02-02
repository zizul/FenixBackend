
namespace Application.Services.Event.Contracts
{
    /// <summary>
    /// Responsible for responders searching and sending notifications via Firebase
    /// </summary>
    public interface IEventCoordinatorService
    {
        Task TryFindAndAssignRespondersToEvent(string eventId, double radiusInKm);
    }
}
