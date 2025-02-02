using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistance.Core.Ceph
{
    internal class CephObjectStorageS3 : IObjectStorage
    {
        private readonly AmazonS3Client client;
        private readonly string serviceUrl;


        public CephObjectStorageS3(IObjectStorageS3Context context)
        {
            client = context.Client;
            serviceUrl = context.Client.Config.ServiceURL;
        }

        /// <returns>url to the added object</returns>
        public async Task<string> Put(IFormFile file, string bucketName)
        {
            await CreateBucketIfNotExists(bucketName);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using (var newMemoryStream = new MemoryStream())
            {
                file.CopyTo(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = fileName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // serviceUrl ends on /
            return $"{serviceUrl}{bucketName}/{fileName}";
        }

        private async Task CreateBucketIfNotExists(string bucketName)
        {
            if (await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName)) 
            {
                return;
            }

            PutBucketRequest request = new PutBucketRequest();
            request.BucketName = bucketName;
            await client.PutBucketAsync(request);
        }

        public async Task Delete(string url)
        {
            var urlTokens = url.Split('/');
            var bucketName = urlTokens[urlTokens.Length - 2];
            var key = urlTokens[urlTokens.Length - 1];

            await client.DeleteObjectAsync(bucketName, key);
        }
    }
}
