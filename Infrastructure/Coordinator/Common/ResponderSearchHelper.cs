using Domain.Enums;
using Infrastructure.Persistance.Core;

namespace Infrastructure.Coordinator.Common
{
    internal static class ResponderSearchHelper
    {
        internal static string IsResponderNearbyEventFilter(string coordinates, string radius, string userParam)
        {
            return
                $"LET nearbyOfEvent = (" +
                    $"FOR d in {GlobalCollections.USER_DEVICES} " +
                    $"FILTER (d.user_ref == {userParam}._key) " +
                    $"FILTER (d.device_id == {userParam}.active_device_id) " +
                    $"FILTER GEO_IN_RANGE([d.location.longitude, d.location.latitude], {coordinates}, 0, {radius}, false, true) " +
                    $"RETURN 1) " +
                $"FILTER LENGTH(nearbyOfEvent) > 0 ";
        }

        /// <summary>
        /// Filter logic is closely related to the Responder entity's
        /// business logic located inside of the Domain Layer
        /// </summary>
        internal static string IsResponderNotAssignedToEventsFilter(string eventId, string userParam)
        {
            return
                $"LET assignedToEvents = (FOR r in {GlobalCollections.EVENT_RESPONDERS} " +
                    $"FILTER (r.responder_ref == {userParam}._key) " +
                    $"FILTER ({GetResponderInActionFilter()} || (r.event_ref == {eventId})) " +
                    $"RETURN 1) " +
                $"FILTER LENGTH(assignedToEvents) == 0 ";
        }

        private static string GetResponderInActionFilter()
        {
            return $"(r.status == '{ResponderStatusType.Accepted}' || r.status == '{ResponderStatusType.Arrived}' || r.status == '{ResponderStatusType.Pending}')";
        }

        internal static string IsResponderNotCreatorOfEventFilter(string eventId, string userParam)
        {
            return
                $"LET isCreator = (FOR e in {GlobalCollections.EVENTS} " +
                    $"FILTER (e._key == {eventId}) " +
                    $"FILTER (e.reporter_ref == {userParam}._key) " +
                    $"RETURN 1) " +
                $"FILTER LENGTH(isCreator) == 0 ";
        }

        /// <summary>
        /// Filter logic is closely related to the UserReadiness entity's
        /// business logic located inside of the Domain Layer
        /// </summary>
        internal static string IsResponderOnDutyFilter(string userRef, DayOfWeek day, TimeSpan time)
        {
            return
                $"LET isAvailable = (" +
                    $"FOR r in {GlobalCollections.USER_READINESS} " +
                    $"FILTER r.user_ref == {userRef} " +
                    $"FILTER (r.readiness_status == '{nameof(ReadinessStatus.Ready).ToLower()}' " +
                        $"|| (r.readiness_status == '{nameof(ReadinessStatus.BySchedule).ToLower()}' " +
                        $"&& POSITION({GetInAvailableRangesFilter(day, time)}, true))) " +
                    $"RETURN 1 " +
                $") " +
                $"FILTER LENGTH(isAvailable) > 0 ";
        }

        private static string GetInAvailableRangesFilter(DayOfWeek day, TimeSpan time)
        {
            return
                $"FOR range in r.ranges " +
                $"FILTER range.enabled && {GetInRangeFilter(day, time)} RETURN true ";
        }

        private static string GetInRangeFilter(DayOfWeek day, TimeSpan time)
        {
            return
                $"range.day == '{day}' && range.start_time <= '{time}' && range.end_time > '{time}' ";
        }
    }
}
