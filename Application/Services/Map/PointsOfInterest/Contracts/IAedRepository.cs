using Domain.Entities.Map;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Map.PointsOfInterest.Contracts
{
    public interface IAedRepository
    {
        Task<Aed> Add(Aed aed);
        Task<Aed> Update(string id, Aed aed);
        Task Delete(string id);
        Task<string> UpdatePhoto(string id, IFormFile photo);
        Task DeletePhoto(string id);
    }
}
