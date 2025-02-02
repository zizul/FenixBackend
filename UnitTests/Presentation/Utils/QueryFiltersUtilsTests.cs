using Domain.Enums;
using Presentation.Utils;

namespace UnitTests.Presentation.Utils
{
    public class QueryFiltersUtilsTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Parse_Should_IgnoreQuery(string? query)
        {
            var filters = QueryFiltersUtils.ParsePoiFilters(query);
            Assert.Empty(filters);
        }

        [Fact]
        public void Parse_Should_CorrectlyProcessQuery()
        {
            var query =
                $"{PointOfInterestType.AED}=false,{PointOfInterestType.SOR}=false,{PointOfInterestType.NISWOL}=true";

            var filters = QueryFiltersUtils.ParsePoiFilters(query);

            Assert.Equal(3, filters.Count);
            Assert.Collection(filters,
                x => Assert.True(x.Type == PointOfInterestType.AED && x.IsInclude == false),
                x => Assert.True(x.Type == PointOfInterestType.SOR && x.IsInclude == false),
                x => Assert.True(x.Type == PointOfInterestType.NISWOL && x.IsInclude == true));
        }

        [Theory]
        [InlineData("aed")]
        [InlineData("aed=trfue")]
        [InlineData("aed=ffalsesor=false,niswol=true")]
        public void Parse_Should_ThrowArgumentException(string? query)
        {
            Assert.Throws<ArgumentException>(() => QueryFiltersUtils.ParsePoiFilters(query));
        }
    }
}
