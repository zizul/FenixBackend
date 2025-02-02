using Application.Services.Event.DTOs.Common;
using MediatR;

namespace Application.Services.Event.DTOs
{
    public class UpdateResponderStatusCommandDto : IRequest
    {
        public string EventId { get; }
        public string IdentityId { get; }
        public ResponderInputDto ResponderData { get; }


        public UpdateResponderStatusCommandDto(
            string eventId, string identityId, ResponderInputDto responderData)
        {
            EventId = eventId;
            IdentityId = identityId;
            ResponderData = responderData;
        }
    }
}
