using Domain.Entities;
using MediatR;

namespace Application.Common
{
    /// <summary>
    /// Lightweight wrapper that bridges Domain events to MediatR notifications
    /// without polluting the Domain layer with MediatR dependency.
    /// 
    /// Readonly record struct: zero-allocation wrapper (stack-allocated).
    /// Value equality provided automatically by record — two notifications
    /// wrapping equal domain events are themselves equal.
    /// </summary>
    public readonly record struct DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
        where TDomainEvent : IDomainEvent;
}
