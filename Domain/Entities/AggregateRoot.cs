
namespace Domain.Entities
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> domainEvents = new();

        /// <summary>
        /// Registered domain events pending dispatch. Read-only view to prevent external mutation.
        /// </summary>
        public IReadOnlyList<IDomainEvent> DomainEvents => domainEvents;

        protected void RegisterDomainEvent(IDomainEvent domainEvent)
        {
            domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears all pending domain events after they have been dispatched.
        /// Prevents duplicate dispatch if Consume is called more than once on the same instance.
        /// </summary>
        public void ClearDomainEvents()
        {
            domainEvents.Clear();
        }
    }
}