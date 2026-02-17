using Application.Common;
using Application.Exceptions;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainExceptions;
using MediatR;

namespace Application.Services.Event.Commands
{
    public sealed class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommandDto>
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
            // Func overload enables awaiting domain event dispatch inside the callback.
            // Events are dispatched within the repository's update scope, ensuring handler
            // completion before the operation returns.
            await repository.Update(request.EventId, async (reportedEvent) =>
            {
                UpdateEvent(reportedEvent, request);
                await eventsConsumer.Consume(reportedEvent.DomainEvents);
                reportedEvent.ClearDomainEvents();
            });
        }

        private static void UpdateEvent(ReportedEvent reportedEvent, UpdateEventCommandDto request)
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
