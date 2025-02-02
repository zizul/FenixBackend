using Application.Services.Readiness.DTOs.Common;
using MediatR;

namespace Application.Services.Readiness.DTOs
{
    public class GetReadinessQueryDto : IRequest<UserReadinessDataDto>
    {
        public string IdentityId { get; }


        public GetReadinessQueryDto(string identityId)
        {
            IdentityId = identityId;
        }
    }
}
