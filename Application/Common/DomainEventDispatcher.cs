using Domain.Entities;
using MediatR;

namespace Application.Common
{
    public class DomainEventDispatcher : IDomainEventConsumer
    {
        private readonly IMediator mediator;


        public DomainEventDispatcher(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public void Consume(IReadOnlyList<IDomainEvent> changes)
        {
            foreach (var change in changes)
            {
                var domainEventNotification = CreateDomainEventNotification((dynamic)change);

                mediator.Publish(domainEventNotification);
            }
        }

        private static DomainEventNotification<TDomainEvent> CreateDomainEventNotification<TDomainEvent>(TDomainEvent domainEvent)
            where TDomainEvent : IDomainEvent
        {
            return new DomainEventNotification<TDomainEvent>(domainEvent);
        }
    }
}
