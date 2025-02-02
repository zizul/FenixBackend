using Application.Common;
using Application.Exceptions;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainExceptions;
using MediatR;

namespace Application.Services.Event.Commands
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommandDto>
    {
        private readonly IReportedEventsRepository repository;
        private readonly IDomainEventConsumer eventsConsumer;


        public UpdateEventCommandHandler(
            IReportedEventsRepository repository, 
            IDomainEventConsumer eventsConsumer)
        {
            this.repository = repository;
            this.eventsConsumer = eventsConsumer;
        }

        public async Task Handle(UpdateEventCommandDto request, CancellationToken cancellationToken)
        {
            Action<ReportedEvent> updateEntity = (reportedEvent) =>
            {
                UpdateEvent(reportedEvent, request);
                eventsConsumer.Consume(reportedEvent.DomainEvents);
            };

            var updated = await repository.Update(request.EventId, updateEntity);
        }

        private void UpdateEvent(ReportedEvent reportedEvent, UpdateEventCommandDto request)
        {
            try
            {
                reportedEvent.ChangeEventStatus(request.Status);
            }
            catch (ForbiddenEventStatusDomainException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (EventIsClosedDomainException e)
            {
                throw new ResourceConflictException(e.Message);
            }
        }
    }
}