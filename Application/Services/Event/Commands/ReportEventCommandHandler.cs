using Application.Common;
using Application.Services.Event.Contracts;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using AutoMapper;
using Domain.Entities.Event;
using MediatR;

namespace Application.Services.Event.Commands
{
    public sealed class ReportEventCommandHandler : IRequestHandler<ReportEventCommandDto, ReportedEventResultDto>
    {
        private readonly IReportedEventsRepository repository;
        private readonly IMapper mapper;
        private readonly IDomainEventConsumer eventsConsumer;


        public ReportEventCommandHandler(
            IReportedEventsRepository repository, 
            IMapper mapper, 
            IDomainEventConsumer eventsConsumer)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.eventsConsumer = eventsConsumer;
        }

        public async Task<ReportedEventResultDto> Handle(ReportEventCommandDto request, CancellationToken cancellationToken)
        {
            var reportedEvent = mapper.Map<ReportedEvent>(request);

            var addedEvent = await repository.Add(reportedEvent, request.IdentityId);
            addedEvent.ReportEvent();

            // Domain events dispatched after successful persistence (awaited for reliability).
            // Publishes SearchRespondersCommand to RabbitMQ via EventReportedDomainEventHandler.
            await eventsConsumer.Consume(addedEvent.DomainEvents);
            addedEvent.ClearDomainEvents();

            var result = mapper.Map<ReportedEventResultDto>(addedEvent);
            return result;
        }
    }
}
