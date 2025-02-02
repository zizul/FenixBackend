namespace Domain.ValueObjects
{
    public class MonthlyRule
    {
        public List<string> Months { get; }
        public List<DailyRule> DailyRules { get; }

        public MonthlyRule(List<string> months, List<DailyRule> dailyRules)
        {
            Months = months;
            DailyRules = dailyRules;
        }
    }
}