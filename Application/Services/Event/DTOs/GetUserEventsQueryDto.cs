using Domain.Enums;
using MediatR;

namespace Application.Services.Event.DTOs
{
    public class GetUserEventsQueryDto : IRequest<GetUserEventsResultDto>
    {
        public string IdentityId { get; }
        public bool IsResponderRole { get; }
        public List<EventStatusType> EventStatusTypes { get; }


        public GetUserEventsQueryDto(string userId, bool isResponderRole, List<EventStatusType> eventStatusTypes)
        {
            IdentityId = userId;
            IsResponderRole = isResponderRole;
            EventStatusTypes = eventStatusTypes;
        }
    }
}
