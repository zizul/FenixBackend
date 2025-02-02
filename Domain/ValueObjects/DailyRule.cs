namespace Domain.ValueObjects
{
    public class DailyRule
    {
        public List<string> Days { get; }
        public List<HourRule> HourRules { get; }

        public DailyRule(List<string> days, List<HourRule> hourRules)
        {
            Days = days;
            HourRules = hourRules;
        }
    }
}