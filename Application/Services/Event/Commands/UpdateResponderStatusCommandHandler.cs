using Application.Common;
using Application.Exceptions;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.User.Contracts;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainExceptions;
using Domain.Entities.User;
using MediatR;

namespace Application.Services.Event.Commands
{
    public class UpdateResponderStatusCommandHandler : IRequestHandler<UpdateResponderStatusCommandDto>
    {
        private readonly IReportedEventsRepository repository;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDomainEventConsumer eventsConsumer;


        public UpdateResponderStatusCommandHandler(
            IReportedEventsRepository repository, IDomainEventConsumer eventsConsumer, IDeviceRepository deviceRepository)
        {
            this.repository = repository;
            this.eventsConsumer = eventsConsumer;
            this.deviceRepository = deviceRepository;
        }

        public async Task Handle(UpdateResponderStatusCommandDto request, CancellationToken cancellationToken)
        {
            var device = await deviceRepository.GetUserActiveDevice(request.IdentityId);

            Action<ReportedEvent> updateEntity = (reportedEvent) =>
            {
                UpdateResponder(reportedEvent, request, device);
                eventsConsumer.Consume(reportedEvent.DomainEvents);
            };

            await repository.Update(request.EventId, updateEntity);
        }

        private void UpdateResponder(ReportedEvent reportedEvent, UpdateResponderStatusCommandDto request, Device? device)
        {
            try
            {
                reportedEvent.UpdateResponder(
                    request.IdentityId,
                    request.ResponderData.Status,
                    request.ResponderData.Transport,
                    request.ResponderData.ETA,
                    device?.Coordinates);
            }
            catch (ResponderNotRelatedToEventDomainException e)
            {
                throw new ResourceNotFoundException(e.Message);
            }
            catch (ResponderInvalidStateDomainException e)
            {
                throw new ResourceConflictException(e.Message);
            }
            catch (EventIsClosedDomainException e)
            {
                throw new ResourceConflictException(e.Message);
            }
        }
    }
}