using Asp.Versioning;

namespace Presentation.Common
{
    public static class ApiVersions
    {
        public const string V1String = "1.0";
        public static readonly ApiVersion V1 = new ApiVersion(1, 0);
        public static readonly ApiVersion Default = V1;
    }
}
