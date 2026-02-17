using Domain.Enums;

namespace Domain.Entities.Event.DomainEvents
{
    /// <summary>
    /// Raised when an event reaches a terminal state (Completed or Cancelled).
    /// Named "Closed" rather than "Completed" because it covers both outcomes —
    /// the FinalStatus discriminator tells handlers which terminal state was reached.
    /// Readonly record struct: immutable, stack-allocated, value equality by default.
    /// </summary>
    public readonly record struct EventClosedDomainEvent(string EventId, EventStatusType FinalStatus) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
