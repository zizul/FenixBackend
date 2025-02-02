namespace Domain.ValueObjects
{
    public class SpecialRule
    {
        public List<string> Dates { get; }
        public bool Repeatable { get; }
        public List<HourRule> HourRules { get; }

        public SpecialRule(List<string> dates, bool repeatable, List<HourRule> hourRules)
        {
            Dates = dates;
            Repeatable = repeatable;
            HourRules = hourRules;
        }
    }
}