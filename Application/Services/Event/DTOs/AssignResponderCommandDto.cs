using MediatR;

namespace Application.Services.Event.DTOs
{
    public class AssignResponderCommandDto : IRequest
    {
        public string EventId { get; }
        public string IdentityId { get; }


        public AssignResponderCommandDto(string eventId, string identityId)
        {
            EventId = eventId;
            IdentityId = identityId;
        }
    }
}
