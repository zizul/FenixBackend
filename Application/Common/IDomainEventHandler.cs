using Domain.Entities;
using MediatR;

namespace Application.Common
{
    /// <summary>
    /// Adapter for interface readability:
    ///   INotificationHandler{DomainEventNotification{SomeEvent}} → IDomainEventHandler{SomeEvent}
    /// 
    /// Constrains T to IDomainEvent, preventing accidental registration
    /// of handlers for non-domain-event types.
    /// </summary>
    public interface IDomainEventHandler<T> : INotificationHandler<DomainEventNotification<T>>
        where T : IDomainEvent
    {
    }
}
