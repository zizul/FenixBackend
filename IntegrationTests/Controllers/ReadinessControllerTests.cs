using IntegrationTests.Common.Fixtures;
using Presentation.Common;
using System.Net.Http.Json;
using System.Net;
using IntegrationTests.Common;
using Application.Services.Readiness.DTOs.Common;
using Domain.ValueObjects;
using IntegrationTests.Common.Utils;

namespace IntegrationTests.Controllers
{
    public class ReadinessControllerTests : IClassFixture<ReadinessFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string ReadinessRoute => fixture.Application.GetRoute(ApiRoutes.READINESS_ROUTE);

        private readonly ReadinessFixture fixture;
        private readonly HttpClient client;


        public ReadinessControllerTests(ReadinessFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
        }

        [Fact]
        public async Task GETReadiness_Should_ReturnOkCode200()
        {
            var identityId = fixture.ReadOnlyUser.IdentityId;
            var readiness = fixture.ReadOnlyReadiness;
            var uriBuilder = new UriBuilder($"{BaseUrl}/{ReadinessRoute}/{identityId}");

            var response = await client.GetAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await ControllersTestsUtils.GetFromResponse<UserReadinessDataDto>(response);
            Assert.NotNull(result);
            Assert.Equal(readiness.ReadinessStatus, result.ReadinessStatus);
            Assert.Equivalent(readiness.ReadinessRanges[0], result.ReadinessRanges[0]);
        }

        [Fact]
        public async Task GETReadiness_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{ReadinessRoute}/{-1}");

            var response = await client.GetAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PUTReadiness_Should_ReturnNoContentCode204()
        {
            var identityId = fixture.WriteOnlyUser.IdentityId;
            var uriBuilder = new UriBuilder($"{BaseUrl}/{ReadinessRoute}/{identityId}");
            var newReadiness = GetUserReadinessData();

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), newReadiness);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task PUTReadiness_Should_ReturnNotFoundCode404_1()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{ReadinessRoute}/{-1}");
            var newReadiness = GetUserReadinessData();

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), newReadiness);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PUTReadiness_Should_ReturnNotFoundCode404_2()
        {
            var readinessId = fixture.UserWithoutReadiness.Id;
            var uriBuilder = new UriBuilder($"{BaseUrl}/{ReadinessRoute}/{readinessId}");
            var newReadiness = GetUserReadinessData();

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), newReadiness);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private UserReadinessDataDto GetUserReadinessData()
        {
            return new UserReadinessDataDto()
            {
                ReadinessStatus = Domain.Enums.ReadinessStatus.Ready,
                ReadinessRanges = new ReadinessRange[]
                {
                    new ReadinessRange(true, TimeSpan.FromHours(5), TimeSpan.FromHours(12), DayOfWeek.Tuesday)
                }
            };
        }
    }
}