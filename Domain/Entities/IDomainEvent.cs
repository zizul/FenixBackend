namespace Domain.Entities
{
    /// <summary>
    /// Marker interface for domain events raised by aggregates.
    /// All implementations should be immutable (readonly record structs) to:
    ///   - prevent mutation after being raised (DDD invariant safety)
    ///   - avoid heap allocations (struct semantics)
    ///   - get value equality for free (record equality)
    /// 
    /// OccurredAt provides a monotonic timestamp for event ordering and auditing.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
