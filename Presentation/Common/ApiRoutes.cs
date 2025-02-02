
namespace Presentation.Common
{
    public static class ApiRoutes
    {
        public const string POI_ROUTE = "api/v{version:apiVersion}/medical-poi";
        public const string AED_RESOURCE = "aeds";

        public const string EVENTS_ROUTE = "api/v{version:apiVersion}/events";
        public const string RESPONDERS_RESOURCE = "responders";

        public const string USERS_ROUTE = "api/v{version:apiVersion}/users";
        public const string LOCATION_ROUTE = "api/v{version:apiVersion}/location";
        public const string DEVICES_RESOURCE = "devices";

        public const string READINESS_ROUTE = "api/v{version:apiVersion}/readiness";

        public const string PHOTO_RESOURCE = "photo";
    }
}
