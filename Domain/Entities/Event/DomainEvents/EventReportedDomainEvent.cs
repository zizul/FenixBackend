namespace Domain.Entities.Event.DomainEvents
{
    /// <summary>
    /// Raised when a new emergency event is reported.
    /// Readonly record struct: immutable, stack-allocated, value equality by default.
    /// </summary>
    public readonly record struct EventReportedDomainEvent(string EventId) : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
