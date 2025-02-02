using Infrastructure.Persistance.Core;
using Microsoft.AspNetCore.Http;

namespace IntegrationTests.Common.Stubs
{
    internal class ObjectStorageStub : IObjectStorage
    {
        internal readonly List<string> StoredFiles = new();

        private readonly object deleteLock = new();
        private readonly object updateLock = new();


        public Task Delete(string url)
        {
            lock (deleteLock)
            {
                StoredFiles.Remove(url);
            }

            return Task.CompletedTask;
        }

        public Task<string> Put(IFormFile file, string bucketName)
        {
            var url = $"test/{bucketName}/{Guid.NewGuid()}-{file.FileName}";

            lock (updateLock)
            {
                StoredFiles.Add(url);
            }

            return Task.Run(() => url);
        }
    }
}