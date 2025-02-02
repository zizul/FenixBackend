namespace Domain.ValueObjects
{
    public class HourRule
    {
        public string OpenHour { get; }
        public string CloseHour { get; }

        public HourRule(string openHour, string closeHour)
        {
            OpenHour = openHour;
            CloseHour = closeHour;
        }
    }
}