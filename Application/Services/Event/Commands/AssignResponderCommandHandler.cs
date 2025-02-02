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
    public class AssignResponderCommandHandler : IRequestHandler<AssignResponderCommandDto>
    {
        private readonly IReportedEventsRepository eventRepository;
        private readonly IDeviceRepository deviceRepository;

        public AssignResponderCommandHandler(IReportedEventsRepository eventRepository, IDeviceRepository deviceRepository)
        {
            this.eventRepository = eventRepository;
            this.deviceRepository = deviceRepository;
        }

        public async Task Handle(AssignResponderCommandDto request, CancellationToken cancellationToken)
        {
            var device = await deviceRepository.GetUserActiveDevice(request.IdentityId);

            Action<ReportedEvent> updateEntity = (reportedEvent) =>
            {
                AssignResponder(reportedEvent, request.IdentityId, device);
            };

            await eventRepository.Update(request.EventId, updateEntity);
        }

        private void AssignResponder(ReportedEvent reportedEvent, string responderId, Device? device)
        {
            try
            {
                reportedEvent.AssignResponder(responderId, device?.Coordinates);
            }
            catch (ResponderAlreadyAssignedDomainException e)
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