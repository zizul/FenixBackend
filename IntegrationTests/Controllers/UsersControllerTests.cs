using Application.Services.User.DTOs.Common;
using Domain.Enums;
using IntegrationTests.Common;
using IntegrationTests.Common.Fixtures;
using IntegrationTests.Common.Stubs;
using IntegrationTests.Common.Utils;
using Presentation.Common;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.Controllers
{
    public class UsersControllerTests : IClassFixture<UsersFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string UsersRoute => fixture.Application.GetRoute(ApiRoutes.USERS_ROUTE);

        private readonly UsersFixture fixture;
        private readonly HttpClient client;


        public UsersControllerTests(UsersFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
        }
        
        [Fact]
        public async Task POSTAccount_Should_ReturnCreatedCode201()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}");
            var account = GetAccountData("post-201@t.com", "201201201", "post-201", "post-201");

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await ControllersTestsUtils.GetFromResponse<UserDto>(response);
            Assert.NotNull(result);
            Assert.Equivalent(account, result);
        }

        [Theory]
        [InlineData("wrongEmailFormat", "100000000")]
        [InlineData("post-400@t.com", "wrongPhoneFormat")]
        public async Task POSTAccount_Should_ReturnBadRequest400(string email, string phone)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}");
            var account = GetAccountData(email, phone, "post-400", "post-400");

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(UsersFixture.AddedEmail, "100000000", "post-409", "post-409")]
        [InlineData("post-409@t.com", UsersFixture.AddedPhone, "post-409", "post-409")]
        [InlineData("post-409@t.com", "100000000", UsersFixture.ReadOnlyIdentityId, "post-409")]
        [InlineData("post-409@t.com", "100000000", "post-409", UsersFixture.AddedUsername)]
        public async Task POSTAccount_Should_ReturnConflict409(string email, string phone, string identityId, string username)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}");
            var account = GetAccountData(email, phone, identityId, username);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PUTAccount_Should_ReturnOkCode200()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{UsersFixture.IdentityIdToUpdate1}");
            var account = GetAccountData("put-200@t.com", "200200200", "put-200", "put-200");

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await ControllersTestsUtils.GetFromResponse<UserDto>(response);
            Assert.NotNull(result);
            Assert.Equivalent(account, result);
        }

        [Fact]
        public async Task PUTAccount_Should_ReturnNotFound404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{-1}");
            var account = GetAccountData("put-404@t.com", "404404404", "put-404", "put-404");

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(UsersFixture.AddedEmail, "200000000", "put-409", "put-409")]
        [InlineData("put-409@t.com", UsersFixture.AddedPhone, "put-409", "put-409")]
        [InlineData("put-409@t.com", "200000000", UsersFixture.ReadOnlyIdentityId, "put-409")]
        [InlineData("put-409@t.com", "200000000", "put-409", UsersFixture.AddedUsername)]
        public async Task PUTAccount_Should_ReturnConflict409(string email, string phone, string identityId, string username)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{UsersFixture.IdentityIdToUpdate2}");
            var account = GetAccountData(email, phone, identityId, username);

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData("wrongEmailFormat", "100000000")]
        [InlineData("put-400@t.com", "wrongPhoneFormat")]
        public async Task PUTAccount_Should_ReturnBadRequest400(string email, string phone)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{UsersFixture.IdentityIdToUpdate2}");
            var account = GetAccountData(email, phone, "put-400", "put-400");

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), account);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DELETEAccount_Should_ReturnNoContentCode204()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{UsersFixture.IdentityIdToDelete}");
            var devicesExistedBefore = await fixture.AreDevicesExist(UsersFixture.DevicesIdToDelete);

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.True(devicesExistedBefore);
            Assert.False(await fixture.AreDevicesExist(UsersFixture.DevicesIdToDelete));
            Assert.False(await fixture.IsReadinessExists(UsersFixture.IdentityIdToDelete));
        }

        [Fact]
        public async Task DELETEAccount_Should_ReturnNotFound404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{-1}");

            var response = await client.DeleteAsync(uriBuilder.ToString());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private UserDto GetAccountData(
            string email = "example@test.com", 
            string phone = "111222333", 
            string identityId = "example-id",
            string username = "example-username")
        {
            return new UserDto()
            {
                FirstName = "Test",
                LastName = "Test",
                IdentityId = identityId,
                Username = username,
                MobileNumber = phone,
                IsEmailVerified = true,
                IsMobileNumberVerified = false,
                Email = email,
                Role = UserRoles.User
            };
        }

        [Theory]
        [InlineData(UsersFixture.AddedDeviceId)]
        [InlineData("new-device-id")]
        public async Task POSTDevice_Should_ReturnNoContent204(string deviceId)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{UsersFixture.ReadOnlyIdentityId}/{ApiRoutes.DEVICES_RESOURCE}");
            var device = GetDeviceData(deviceId);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), device);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.True(await fixture.IsActiveDeviceIdIsSet(deviceId, UsersFixture.ReadOnlyIdentityId));
        }

        [Fact]
        public async Task POSTDevice_Should_ReturnNotFound404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{UsersRoute}/{-1}/{ApiRoutes.DEVICES_RESOURCE}");
            var device = GetDeviceData();

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), device);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private DeviceDto GetDeviceData(string id = "test id")
        {
            return new DeviceDto()
            {
                DeviceId = id,
                FirebaseToken = "test token",
                DeviceModel = "test model",
            };
        }
    }
}