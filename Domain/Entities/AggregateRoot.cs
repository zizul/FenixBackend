namespace Domain.Entities
{
    /// <summary>
    /// Base class for aggregate roots in the domain model.
    /// Aggregates are the consistency boundary — domain events are only raised
    /// within aggregate methods to guarantee invariants are upheld before the event is emitted.
    /// 
    /// Design notes:
    ///   - DomainEvents uses IReadOnlyList to prevent external Add/Remove
    ///   - RegisterDomainEvent is protected — only the aggregate itself can raise events
    ///   - ClearDomainEvents is public — the infrastructure layer calls it after dispatch
    ///   - Struct domain events are boxed when stored as IDomainEvent; this is acceptable
    ///     because aggregates typically raise 1-2 events per operation (negligible allocation)
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> domainEvents = new();

        public IReadOnlyList<IDomainEvent> DomainEvents => domainEvents;

        /// <summary>
        /// Registers a domain event to be dispatched after the aggregate is persisted.
        /// Must only be called AFTER all invariants are validated — an event represents
        /// a fact that has already happened within the aggregate boundary.
        /// </summary>
        protected void RegisterDomainEvent(IDomainEvent domainEvent)
        {
            domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Clears all pending domain events after dispatch.
        /// Called by infrastructure after Consume() to prevent duplicate dispatch
        /// if the same aggregate instance is reused within a request scope.
        /// </summary>
        public void ClearDomainEvents()
        {
            domainEvents.Clear();
        }
    }
}
