using Amazon.S3;

namespace Infrastructure.Persistance.Core.Ceph
{
    internal interface IObjectStorageS3Context
    {
        public AmazonS3Client Client { get; }
    }
}
