using Application.Services.Map.PointsOfInterest.Validators;
using Domain.ValueObjects;

namespace UnitTests.Application.Map.PointsOfInterest.Validators
{
    public class CoordinatesValidatorTests
    {
        private readonly CoordinatesValidator validator;


        public CoordinatesValidatorTests()
        {
            validator = new CoordinatesValidator();
        }

        [Theory]
        [InlineData(10.5, 20.5)]
        [InlineData(0, 0)]
        [InlineData(-5, -50)]
        public void Validate_Should_Valid(double longitude, double latitude)
        {
            var coordinates = new Coordinates(longitude, latitude);

            var result = validator.Validate(coordinates);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(180, -180)]
        [InlineData(500, 0)]
        [InlineData(-5, -500)]
        public void Validate_Should_Fail(double longitude, double latitude)
        {
            var coordinates = new Coordinates(longitude, latitude);

            var result = validator.Validate(coordinates);

            Assert.False(result.IsValid);
        }
    }
}
