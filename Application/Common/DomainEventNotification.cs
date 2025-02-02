using Domain.Entities;
using MediatR;

namespace Application.Common
{
    /// <summary>
    /// Wrapper around DomainEvent, to not add MediatR dependency to the Domain layer
    /// </summary>
    public class DomainEventNotification<TDomainEvent> : INotification
        where TDomainEvent : IDomainEvent
    {
        public TDomainEvent DomainEvent { get; }


        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
