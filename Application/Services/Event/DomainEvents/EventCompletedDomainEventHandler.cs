using Domain.Entities.Event.DomainEvents;
using Application.Common;
using Application.Services.Event.Worker;

namespace Application.Services.Event.DomainEvents
{
    public class EventCompletedDomainEventHandler : IDomainEventHandler<EventCompletedDomainEvent>
    {
        private readonly IWorkerManager worker;


        public EventCompletedDomainEventHandler(IWorkerManager worker) 
        {
            this.worker = worker;
        }

        public Task Handle(DomainEventNotification<EventCompletedDomainEvent> notification, CancellationToken cancellationToken)
        {
            worker.CancelRunningJob(notification.DomainEvent.Id);
            return Task.CompletedTask;
        }
    }
}
