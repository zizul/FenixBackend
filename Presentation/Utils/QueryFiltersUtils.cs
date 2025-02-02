using Application.Services.Map.PointsOfInterest.Queries;
using Domain.Enums;

namespace Presentation.Utils
{
    public static class QueryFiltersUtils
    {

        public static List<EventStatusType> ParseEventStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return null;
            }

            var splittedStatusStr = status.Trim().Split(',');
            var result = new List<EventStatusType>();

            foreach (var statusStr in splittedStatusStr)
            {
                if (!Enum.TryParse(statusStr, true, out EventStatusType statusType))
                {
                    throw new ArgumentException($"Wrong event status format: {statusStr}");
                }
                result.Add(statusType);
            }
            return result;
        }

        /// <param name="query">In format: "type1=value1,type2=value2,..."</param>
        public static List<PoiFilter> ParsePoiFilters(string? query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<PoiFilter>();
            }

            var filtersStr = query.Trim().Split(',');
            if (filtersStr.Length == 0)
            {
                ThrowArgumentException(query);
            }

            var filters = new List<PoiFilter>();
            foreach (var filterStr in filtersStr)
            {
                filters.Add(ParsePoiFilter(filterStr));
            }

            return filters;
        }

        private static PoiFilter ParsePoiFilter(string filter)
        {
            var splittedFilterStr = filter.Trim().Split('=');
            if (splittedFilterStr.Length != 2)
            {
                ThrowArgumentException(filter);
            }

            var typeStr = splittedFilterStr[0];
            var isIncludedStr = splittedFilterStr[1];

            if (!Enum.TryParse(typeStr, true, out PointOfInterestType type))
            {
                ThrowArgumentException(filter);
            }

            if (!bool.TryParse(isIncludedStr, out bool isIncluded))
            {
                ThrowArgumentException(filter);
            }

            return new PoiFilter(type, isIncluded);
        }

        private static void ThrowArgumentException(string filter)
        {
            throw new ArgumentException($"Wrong filter format: {filter}");
        }
    }
}
