namespace Domain.ValueObjects
{
    public class ReadinessRange
    {
        public bool IsEnabled { get; }
        public TimeSpan AvailableFrom { get; }
        public TimeSpan AvailableTo { get; }
        public DayOfWeek Day { get; }


        public ReadinessRange(
            bool isEnabled,
            TimeSpan availableFrom,
            TimeSpan availableTo,
            DayOfWeek day)
        {
            this.IsEnabled = isEnabled;
            this.AvailableFrom = availableFrom;
            this.AvailableTo = availableTo;
            this.Day = day;
        }

        public bool IsDateWithinRange(DateTime date)
        {
            if (!IsEnabled)
            {
                return false;
            }

            if (date.DayOfWeek != Day)
            {
                return false;
            }

            var time = date.TimeOfDay;
            return (time >= AvailableFrom && time < AvailableTo);
        }
    }
}