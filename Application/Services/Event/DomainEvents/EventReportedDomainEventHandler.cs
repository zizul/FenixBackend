using Domain.Entities.Event.DomainEvents;
using Application.Common;
using Application.Services.Event.Worker;
using Application.Services.Event.Contracts;

namespace Application.Services.Event.DomainEvents
{
    public class EventReportedDomainEventHandler : IDomainEventHandler<EventReportedDomainEvent>
    {
        private readonly IWorkerManager worker;
        private readonly IEventCoordinatorService coordinator;


        public EventReportedDomainEventHandler(
            IWorkerManager worker, 
            IEventCoordinatorService coordinator) 
        {
            this.worker = worker;
            this.coordinator = coordinator;
        }

        public Task Handle(DomainEventNotification<EventReportedDomainEvent> notification, CancellationToken cancellationToken)
        {
            var id = notification.DomainEvent.Id;

            Func<CancellationToken, Task> search = async (token) => { await SearchForResponders(token, id); };

            worker.AddLoopJob(notification.DomainEvent.Id, search);
            return Task.CompletedTask;
        }

        private async Task SearchForResponders(CancellationToken token, string id)
        {
            double searchRadius = 5;
            int searchDelayInMs = 500;

            while (!token.IsCancellationRequested)
            {
                await coordinator.TryFindAndAssignRespondersToEvent(id, searchRadius);
                await Task.Delay(searchDelayInMs, token);
            }
        }
    }
}
