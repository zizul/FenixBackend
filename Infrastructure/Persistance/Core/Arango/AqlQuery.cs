using Domain.ValueObjects;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Infrastructure.Persistance.Core.Arango
{
    internal class AqlQuery
    {
        private readonly StringBuilder sb;


        internal AqlQuery()
        {
            sb = new StringBuilder();
        }

        internal static AqlQuery CreateForQuery(string collectionName, string paramName = "doc")
        {
            var query = new AqlQuery();
            query.AddToQuery($"FOR {paramName} in {collectionName}");
            return query;
        }


        /// <summary>
        /// https://docs.arangodb.com/3.11/aql/functions/geo/
        /// </summary>
        internal AqlQuery AddGeoInRangeFilter(Coordinates coordinates, double min, double max, bool isIncludeMin, bool isIncludeMax)
        {
            AddToQuery($"FILTER GEO_IN_RANGE(" +
                $"doc.geometry," +
                $"{GetCoordinatesArrayText(coordinates)}," +
                $"{min.ToString(CultureInfo.InvariantCulture)}," +
                $"{max.ToString(CultureInfo.InvariantCulture)}," +
                $"{isIncludeMin}," +
                $"{isIncludeMax})");
            return this;
        }

        internal AqlQuery AddGeoInRangeFilter(Coordinates coordinates, double radiusInM)
        {
            return AddGeoInRangeFilter(coordinates, 0, radiusInM, false, true);
        }

        internal AqlQuery AddSort(params (string, ListSortDirection)[] fieldsWithSort)
        {
            StringBuilder sortExpression = new StringBuilder();
            for (int i = 0; i < fieldsWithSort.Length; i++)
            {
                var field = fieldsWithSort[i].Item1;
                var sort = fieldsWithSort[i].Item2;
                if (sort == ListSortDirection.Ascending)
                {
                    sortExpression.Append($"{field} ASC");
                }
                else
                {
                    sortExpression.Append($"{field} DESC");
                }

                if (i < fieldsWithSort.Length - 1)
                {
                    sortExpression.Append(", ");
                }
            }

            AddToQuery($"SORT {sortExpression}");
            return this;
        }

        internal AqlQuery Return(string paramName = "doc")
        {
            AddToQuery($"RETURN {paramName}");
            return this;
        }

        internal string GetQuery()
        {
            return sb.ToString();
        }

        private string GetCoordinatesArrayText(Coordinates coordinates)
        {
            var longitudeStr = coordinates.Longitude.ToString(CultureInfo.InvariantCulture);
            var latitudeStr = coordinates.Latitude.ToString(CultureInfo.InvariantCulture);
            return $"[{longitudeStr}, {latitudeStr}]";
        }

        private void AddToQuery(string text)
        {
            sb.Append(" ");
            sb.Append(text);
        }
    }
}
