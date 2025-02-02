using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistance.Core
{
    internal interface IObjectStorage
    {
        Task<string> Put(IFormFile file, string bucketName);
        Task Delete(string url);
    }
}
