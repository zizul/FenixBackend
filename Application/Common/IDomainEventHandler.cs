using Domain.Entities;
using MediatR;

namespace Application.Common
{
    /// <summary>
    /// Adapter for interface readability 
    /// INotificationHandler<DomainEventNotification<SomeEvent>> -> IDomainEventHandler<SomeEvent>
    /// </summary>
    public interface IDomainEventHandler<T> : INotificationHandler<DomainEventNotification<T>>
        where T : IDomainEvent
    {
    }
}
