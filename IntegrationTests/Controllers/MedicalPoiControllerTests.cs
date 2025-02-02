using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using Domain.Enums;
using Domain.ValueObjects;
using IntegrationTests.Common;
using IntegrationTests.Common.Fixtures;
using IntegrationTests.Common.Utils;
using Microsoft.AspNetCore.Http;
using Presentation.Common;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace IntegrationTests.Controllers
{
    public class MedicalPoiControllerTests : IClassFixture<PoiFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string PoiRoute => fixture.Application.GetRoute(ApiRoutes.POI_ROUTE);
        private string AedRoute => fixture.Application.GetRoute($"{PoiRoute}/{ApiRoutes.AED_RESOURCE}");

        private readonly PoiFixture fixture;
        private readonly HttpClient client;


        public MedicalPoiControllerTests(PoiFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
        }
        
        [Theory]
        [InlineData(5, null)]
        [InlineData(5.234312, true)]
        [InlineData(5.234312, false)]
        public async Task GETPoi_Should_ReturnOkCode200(double rangeInKm, bool? isSortByDistance)
        {
            var location = PoiFixture.ReadOnlyLocation;
            var uriBuilder = new UriBuilder($"{BaseUrl}/{PoiRoute}");
            uriBuilder.Query = GetPoiQuery(uriBuilder, location, rangeInKm, isSortByDistance);

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<GetMedicalPoiResultDto>(response);
            Assert.NotNull(result);

            Assert.Empty(result.Niswols);
            Assert.Equal(fixture.DbAeds[location].Count, result.Aeds.Count);
            Assert.Equal(fixture.DbSors[location].Count, result.Sors.Count);
            Assert.Equivalent(fixture.DbAeds[location].Select(x => x.Id), result.Aeds.Select(x => x.Id));
            Assert.Equivalent(fixture.DbSors[location].Select(x => x.Id), result.Sors.Select(x => x.Id));

            if (isSortByDistance.HasValue && isSortByDistance.Value)
            {
                var aedsIndices = fixture.DbPoiDistanceFactors.Where(x => result.Aeds.Any(r => r.Id == x.Key))
                    .OrderBy(x => x.Value)
                    .Select(x => x.Key);
                var sorsIndices = fixture.DbPoiDistanceFactors.Where(x => result.Sors.Any(r => r.Id == x.Key))
                    .OrderBy(x => x.Value)
                    .Select(x => x.Key);
                Assert.True(aedsIndices.SequenceEqual(result.Aeds.Select(x => x.Id)));
                Assert.True(sorsIndices.SequenceEqual(result.Sors.Select(x => x.Id)));
            }
        }

        [Fact]
        public async Task GETPoi_Should_ReturnBadRequestCode400()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{PoiRoute}");
            uriBuilder.Query = GetPoiQuery(uriBuilder, PoiFixture.LocationOutOfRange, 5);

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private string GetPoiQuery(UriBuilder builder, Coordinates coordinates, double rangeInKm, bool? isSortByDistance = null)
        {
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["longitude"] = (coordinates.Longitude - 0.0001).ToString(CultureInfo.InvariantCulture);
            query["latitude"] = (coordinates.Latitude).ToString(CultureInfo.InvariantCulture);
            query["rangeInKm"] = rangeInKm.ToString(CultureInfo.InvariantCulture);
            query["filters"] = $"{PointOfInterestType.NISWOL}=false, {PointOfInterestType.AED}=true";
            if (isSortByDistance.HasValue)
            {
                query["isSortByDistance"] = isSortByDistance.Value.ToString();
            }
            return query.ToString()!;
        }

        [Theory]
        [MemberData(nameof(AddAedTestCases))]
        public async Task POSTAed_Should_ReturnResourceCreatedCode201(AedDto aedData)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}");

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<AedResultDto>(response);
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equivalent(aedData, result.Data);
        }

        [Fact]
        public async Task POSTAed_Should_ReturnBadRequestCode400()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}");
            var aedData = GetAedData(PoiFixture.LocationOutOfRange);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PUTAed_Should_ReturnOkCode200()
        {
            var oldAed = await GetAed(PoiFixture.WriteReadLocation);

            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{oldAed.Id}");
            var aedData = GetAedData(PoiFixture.WriteReadLocation);
            aedData.Description = "Changed";
            aedData.OpeningHours = "12/34";

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<AedResultDto>(response);
            Assert.NotNull(result);
            Assert.Equal(oldAed.Id, result.Id);
            Assert.Equivalent(aedData, result.Data);
        }

        [Fact]
        public async Task PUTAed_Should_ReturnBadRequestCode400()
        {
            var oldAed = await GetAed(PoiFixture.WriteReadLocation);

            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{oldAed.Id}");
            var aedData = GetAedData(PoiFixture.LocationOutOfRange);
            aedData.Description = "Changed";
            aedData.OpeningHours = "12/34";

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PUTAed_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{-1}");
            var aedData = GetAedData(PoiFixture.WriteReadLocation);
            aedData.Description = "Changed";
            aedData.OpeningHours = "12/34";

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<AedResultDto> GetAed(Coordinates coordinates, int index = 0)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{PoiRoute}");
            uriBuilder.Query = GetPoiQuery(uriBuilder, coordinates, 5);

            var getResponse = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var getResponseObj = await ControllersTestsUtils.GetFromResponse<GetMedicalPoiResultDto>(getResponse);
            Assert.NotNull(getResponseObj);

            var aed = getResponseObj.Aeds[index];
            return aed;
        }

        private static AedDto GetAedData(Coordinates coordinates)
        {
            return new AedDto()
            {
                Coordinates = coordinates,
                Description = "test description",
                Location = "test location",
                InDoor = true,
                OpeningHours = "Jun-Jul Mo-Fr 08:00-18:00",
                Phone = "123 456 789",
                Access = AedAccessType.Public,
                Address = new Address("test street"),
                Operator = "test operator",
                Level = "2",
                Availability = new Availability(
                    new List<MonthlyRule>()
                    {
                        new MonthlyRule(
                            new List<string>() {"Jun", "Jul"},
                            new List<DailyRule>()
                            {
                                new DailyRule(
                                    new List<string>() {"Mo", "Tu", "We", "Th", "Fr" },
                                    new List<HourRule>()
                                    {
                                        new HourRule("08:00", "18:00")
                                    }
                                )
                            }
                        )
                    },
                    new List<SpecialRule>()
                    {
                        new SpecialRule(
                            new List<string>() { "2024-08-15", "2024-11-01", "2024-11-11" },
                            true,
                            new List<HourRule>()
                            {
                                new HourRule("10:00", "14:00")
                            }
                        ),
                        new SpecialRule(
                            new List<string>() { "2024-04-01", "2025-04-21", "2026-04-06" },
                            false,
                            new List<HourRule>()
                            {
                                new HourRule("10:00", "14:00")
                            }
                        )
                    }
                ),
            };
        }

        private static AedDto GetAedDataAddressWithFloorOnly(Coordinates coordinates)
        {
            return new AedDto()
            {
                Coordinates = coordinates,
                Description = "test description 2",
                Location = "test locatio 2n",
                InDoor = true,
                OpeningHours = "24/6",
                Phone = "321 456 789",
                Access = AedAccessType.WorkHours,
                Address = new Address(street: null, floor: "5"),
                Operator = "test operator 2",
                Level = "2",
            };
        }

        public static IEnumerable<object[]> AddAedTestCases() => new List<object[]> {
                new object[] { GetAedData(PoiFixture.WriteReadLocation) },
                new object[] { GetAedDataAddressWithFloorOnly(PoiFixture.WriteReadLocation) }
        };

        [Fact]
        public async Task DELETEAed_Should_ReturnNoContent204()
        {
            var aed = await PostAed(PoiFixture.DeleteLocation);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{aed.Id}");

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DELETEAed_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{-1}");
            
            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<AedResultDto> PostAed(Coordinates coordinates)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}");
            var aedData = GetAedData(coordinates);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), aedData);

            var result = await ControllersTestsUtils.GetFromResponse<AedResultDto>(response);
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);

            return result;
        }

        [Fact]
        public async Task PUTAedPhoto_Should_ReturnOkCode200()
        {
            var aed = await GetAed(PoiFixture.ReadOnlyLocation);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{aed.Id}/{ApiRoutes.PHOTO_RESOURCE}");
            using (var stream = FilesTestsUtils.GetTestImage(out string contentType))
            {
                var content = GetMultipartFormData(stream, contentType);
                var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString()) { Content = content };

                var response = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var resultUrl = await response.Content.ReadAsStringAsync();
                Assert.Contains(Path.GetFileName(stream.Name), resultUrl);
            }
        }

        [Fact]
        public async Task PUTAedPhoto_Should_ReturnBadRequestCode400()
        {
            var aed = await GetAed(PoiFixture.ReadOnlyLocation);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{aed.Id}/{ApiRoutes.PHOTO_RESOURCE}");
            using (var stream = FilesTestsUtils.GetTestTextFile(out string contentType))
            {
                var content = GetMultipartFormData(stream, contentType);
                var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString()) { Content = content };

                var response = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task PUTAedPhoto_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{-1}/{ApiRoutes.PHOTO_RESOURCE}");
            using (var stream = FilesTestsUtils.GetTestImage(out string contentType))
            {
                var content = GetMultipartFormData(stream, contentType);
                var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString()) { Content = content };

                var response = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        private static MultipartFormDataContent GetMultipartFormData(FileStream fileStream, string contentType)
        {
            var content = new MultipartFormDataContent();
            var file = new StreamContent(fileStream);
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // "photo" - name must be the same as the variable name in the controller's endpoint
            content.Add(file, "photo", fileStream.Name);

            return content;
        }

        [Fact]
        public async Task DELETEAedPhoto_Should_ReturnNoContent204()
        {
            var aed = await GetAed(PoiFixture.ReadOnlyLocation, 1);
            await PutAedPhoto(aed.Id.ToString());
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{aed.Id}/{ApiRoutes.PHOTO_RESOURCE}");

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DELETEAedPhoto_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{-1}/{ApiRoutes.PHOTO_RESOURCE}");

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DELETEAedPhoto_Should_ReturnNotFoundCode404_2()
        {
            var aed = await GetAed(PoiFixture.ReadOnlyLocation, 2);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{aed.Id}/{ApiRoutes.PHOTO_RESOURCE}");

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task PutAedPhoto(string id)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{AedRoute}/{id}/{ApiRoutes.PHOTO_RESOURCE}");
            using (var stream = FilesTestsUtils.GetTestImage(out string contentType))
            {
                var content = GetMultipartFormData(stream, contentType);
                var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.ToString()) { Content = content };

                var response = await client.SendAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}