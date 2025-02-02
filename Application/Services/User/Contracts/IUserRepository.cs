using Domain.Entities.User;

namespace Application.Services.User.Contracts
{
    public interface IUserRepository
    {
        Task<BasicUser> Add(BasicUser credentials);
        Task<BasicUser> Update(string id, BasicUser credentials);
        Task Delete(string id);
    }
}
