using Amazon.S3;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistance.Core.Ceph
{
    internal class ObjectStorageS3Context : IObjectStorageS3Context
    {
        public AmazonS3Client Client { get; set; }


        public ObjectStorageS3Context(IOptions<ObjectStorageS3Options> options)
        {
            var storageOptions = options.Value;

            AmazonS3Config config = new AmazonS3Config();
            config.ServiceURL = storageOptions.ServiceURL;

            Client = new AmazonS3Client(
                    storageOptions.AccessKey,
                    storageOptions.SecretKey,
                    config
                    );
        }
    }
}
