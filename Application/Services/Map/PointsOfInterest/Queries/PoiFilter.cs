using Domain.Enums;

namespace Application.Services.Map.PointsOfInterest.Queries
{
    public class PoiFilter
    {
        public PointOfInterestType Type { get; }
        public bool IsInclude { get; }


        public PoiFilter(PointOfInterestType type, bool isInclude)
        {
            Type = type;
            IsInclude = isInclude;
        }

        public static bool IsIncludeFilter(List<PoiFilter> filters, PointOfInterestType type)
        {
            if (filters.Count(x => x.Type == type) == 0)
            {
                return true;
            }
            return filters.Any(x => x.Type == type && x.IsInclude);
        }
    }
}