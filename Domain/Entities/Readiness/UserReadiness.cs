using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Readiness
{
    public class UserReadiness
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public ReadinessStatus ReadinessStatus { get; set; }
        public ReadinessRange[] ReadinessRanges { get; set; }


        public bool IsUserAvailable()
        {
            if (ReadinessStatus == ReadinessStatus.Ready)
            {
                return true;
            }

            if (ReadinessStatus == ReadinessStatus.NotReady)
            {
                return false;
            }

            return IsWithinAvailabilityRanges();
        }

        private bool IsWithinAvailabilityRanges()
        {
            var currentTime = DateTime.UtcNow;
            foreach (var range in ReadinessRanges)
            {
                if (range.IsDateWithinRange(currentTime))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
