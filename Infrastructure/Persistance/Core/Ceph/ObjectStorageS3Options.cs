
namespace Infrastructure.Persistance.Core.Ceph
{
    internal class ObjectStorageS3Options
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string ServiceURL { get; set; }
    }
}
