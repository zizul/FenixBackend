
namespace Application.Services.Event.Contracts
{
    /// <summary>
    /// Responsible for responders searching and sending notifications via Firebase.
    /// </summary>
    public interface IEventCoordinatorService
    {
        /// <summary>
        /// Searches for available responders near the event and assigns them.
        /// </summary>
        /// <returns>
        /// true if the event still needs responders (consumer should continue searching),
        /// false if the event is resolved or no longer in a searchable state.
        /// </returns>
        Task<bool> TryFindAndAssignRespondersToEvent(string eventId, double radiusInKm);
    }
}
