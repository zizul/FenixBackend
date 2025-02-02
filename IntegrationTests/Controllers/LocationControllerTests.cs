using Application.Services.Location.DTOs.Common;
using IntegrationTests.Common.Fixtures;
using Presentation.Common;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.Controllers
{
    public class LocationControllerTests : IClassFixture<LocationFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string LocationRoute => fixture.Application.GetRoute(ApiRoutes.LOCATION_ROUTE);

        private readonly LocationFixture fixture;
        private readonly HttpClient client;

        public LocationControllerTests(LocationFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
        }

        [Theory]
        [MemberData(nameof(GetTestCases))]
        public async Task PUT_UpdateLocation(string deviceId, Object deviceLocation, HttpStatusCode expectedCode)
        {
            // Assign
            var url = new UriBuilder($"{BaseUrl}/{LocationRoute}/{deviceId}").ToString();

            // Act
            var response = await client.PutAsJsonAsync(url, deviceLocation);

            // Assert
            Assert.Equal(expectedCode, response.StatusCode);
        }

        public static IEnumerable<object[]> GetTestCases() => new List<object[]> {
                new object[]
                {
                    LocationFixture.DEVICE_ID_1,
                    new DeviceLocationDto(LocationFixture.NewCorrectLocation),
                    HttpStatusCode.NoContent
                },
                new object[]
                {
                    LocationFixture.DEVICE_ID_1,
                    new DeviceLocationDto(LocationFixture.LocationOutOfRange),
                    HttpStatusCode.BadRequest
                },
                new object[]
                {
                    LocationFixture.UNKNOWN_DEVICE_ID,
                    new DeviceLocationDto(LocationFixture.NewCorrectLocation),
                    HttpStatusCode.NotFound
                },
                new object[]
                {
                    "",
                    new DeviceLocationDto(LocationFixture.NewCorrectLocation),
                    HttpStatusCode.NotFound
                },
                new object[]
                {
                    null,
                    new DeviceLocationDto(LocationFixture.NewCorrectLocation),
                    HttpStatusCode.NotFound
                },
                new object[]
                {
                    LocationFixture.DEVICE_ID_1,
                    new DeviceLocationDto(null),
                    HttpStatusCode.BadRequest
                },
                new object[]
                {
                    LocationFixture.DEVICE_ID_1,
                    // invalid device location data format
                    new {
                        lat = LocationFixture.NewCorrectLocation.Latitude,
                        lng = LocationFixture.NewCorrectLocation.Latitude
                    },
                    HttpStatusCode.BadRequest
                }
            };
    }
}
