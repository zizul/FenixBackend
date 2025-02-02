using Application.Services.Event.DTOs.Common;
using MediatR;

namespace Application.Services.Event.DTOs
{
    public class GetReportedEventByIdQueryDto : IRequest<ReportedEventResultDto>
    {
        public string IdentityId { get; }
        public string Id { get; }
        public bool IsResponderRole { get; }


        public GetReportedEventByIdQueryDto(string id, string userId, bool isResponderRole)
        {
            Id = id;
            IdentityId = userId;
            IsResponderRole = isResponderRole;
        }
    }
}
