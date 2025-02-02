namespace Domain.ValueObjects
{
    public class Availability
    {
        public List<MonthlyRule> MonthlyRules { get; }
        public List<SpecialRule> SpecialRules { get; }

        public Availability(List<MonthlyRule> monthlyRules, List<SpecialRule> specialRules)
        {
            MonthlyRules = monthlyRules;
            SpecialRules = specialRules;
        }
    }
}