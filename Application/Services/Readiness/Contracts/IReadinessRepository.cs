using Domain.Entities.Readiness;

namespace Application.Services.Readiness.Contracts
{
    public interface IReadinessRepository
    {
        Task<UserReadiness> Get(string identityId);
        Task<UserReadiness> Update(string identityId, UserReadiness userReadiness);
    }
}
