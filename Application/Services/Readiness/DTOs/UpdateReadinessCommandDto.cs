using Application.Services.Readiness.DTOs.Common;
using MediatR;

namespace Application.Services.Readiness.DTOs
{
    public class UpdateReadinessCommandDto : IRequest
    {
        public string IdentityId { get; }
        public UserReadinessDataDto ReadinessData { get; }


        public UpdateReadinessCommandDto(
            string identityId, UserReadinessDataDto readinessData)
        {
            IdentityId = identityId;
            ReadinessData = readinessData;
        }
    }
}
