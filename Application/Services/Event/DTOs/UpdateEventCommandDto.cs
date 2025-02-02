using Application.Services.Event.DTOs.Common;
using Domain.Enums;
using MediatR;

namespace Application.Services.Event.DTOs
{
    public class UpdateEventCommandDto : IRequest
    {
        public string EventId { get; set; }
        public EventStatusType Status { get; }


        public UpdateEventCommandDto(string eventId, EventStatusType status)
        {
            EventId = eventId;
            Status = status;
        }
    }
}
