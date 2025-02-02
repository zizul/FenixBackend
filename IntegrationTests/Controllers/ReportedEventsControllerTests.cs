using Application.Exceptions;
using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;
using IntegrationTests.Common;
using IntegrationTests.Common.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using IntegrationTests.Common.Utils;

namespace IntegrationTests.Controllers
{
    public class ReportedEventsControllerTests : IClassFixture<ReportedEventFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string EventsRoute => fixture.Application.GetRoute(ApiRoutes.EVENTS_ROUTE);

        private readonly ReportedEventFixture fixture;
        private readonly HttpClient client;


        public ReportedEventsControllerTests(ReportedEventFixture fixture)
        {
            this.fixture = fixture;
            this.client = fixture.Client;
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            client.DefaultRequestHeaders.Clear();
        }

        [Fact]
        public async Task POSTEvent_Should_ReturnCreatedCode201()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");
            var eventData = GetEventData(fixture.WriteOnlyLocation);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), eventData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<ReportedEventResultDto>(response);
            Assert.NotNull(result);
            Assert.NotEqual("0", result.Id);
            Assert.Equal(EventStatusType.Pending, result.Status);
        }

        [Fact]
        public async Task POSTEvent_Should_ReturnBadRequest400()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");
            var aedData = GetEventData(fixture.LocationOutOfRange);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), aedData);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private ReportedEventDto GetEventData(Coordinates coordinates)
        {
            return new ReportedEventDto()
            {
                Coordinates = coordinates,
                Address = new Address("test street", "2B", "2", "0"),
                Description = "test description",
                EventType = "test type",
                InjuredCount = 2
            };
        }

        [Fact]
        public async Task GETEventAmz_Should_ReturnOkCode200()
        {
            var reportedEvent = fixture.ReadOnlyReportedEvent;
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, fixture.ReadOnlyReporter.IdentityId);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<ReportedEventResultDto>(response);
            Assert.NotNull(result);
            Assert.Equal(reportedEvent.Id, result.Id);
            Assert.Equal(reportedEvent.Status, result.Status);
            Assert.Null(result.Reporter);
            Assert.Equal(reportedEvent.Responders.Count, result.Responders.Count);
            Assert.All(result.Responders, x => Assert.Null(x.Transport));
            Assert.All(result.Responders, x => Assert.Null(x.Coordinates));
        }

        [Fact]
        public async Task GETEventAmr_Should_ReturnOkCode200()
        {
            var reportedEvent = fixture.ReadOnlyReportedEvent;
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleClaim, Roles.RESPONDER);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<ReportedEventResultDto>(response);
            Assert.NotNull(result);
            Assert.Equal(reportedEvent.Id, result.Id);
            Assert.Equal(reportedEvent.Status, result.Status);
            Assert.Equal(reportedEvent.Reporter.MobileNumber, result.Reporter.MobileNumber);
            Assert.Equal(reportedEvent.Reporter.IdentityId, result.Reporter.IdentityId);
            Assert.Equal(reportedEvent.Responders.Count, result.Responders.Count);
        }

        [Fact]
        public async Task GETEvent_Should_ReturnBadRequest400()
        {
            var reportedEvent = fixture.ReadOnlyReportedEvent;
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, "not-an-owner");
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleClaim, Roles.USER);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GETEvent_Should_ReturnNotFound404()
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{-1}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GETEventsAmz_Should_ReturnOkCode200()
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, fixture.Reporter2.IdentityId);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<GetUserEventsResultDto>(response);
            Assert.NotNull(result);
            Assert.NotNull(result.ReportedEvents);
            Assert.Empty(result.AssignedEvents);
            Assert.Equal(4, result.ReportedEvents.Count);
            for (int i = 0; i < result.ReportedEvents.Count; i++)
            {
                var expectedEvent = fixture.DbUserEvents[fixture.Reporter2][i];
                var resultEvent = result.ReportedEvents[i];
                Assert.Equal(expectedEvent.Id, resultEvent.Id);
                Assert.Equal(expectedEvent.Responders.Count, resultEvent.Responders.Count);
                Assert.All(expectedEvent.Responders, x => Assert.Null(x.Transport));
                Assert.All(expectedEvent.Responders, x => Assert.Null(x.Coordinates));
            }
        }

        [Theory]
        [MemberData(nameof(GetEventsWithStatusCases))]
        public async Task GETEventsAmz_WithStatus_Should_ReturnOkCode200(string statusFilter, HttpStatusCode expectedCode, int expectedCount)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, fixture.ReadOnlyReporter.IdentityId);
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}?status={statusFilter}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(expectedCode, response.StatusCode);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var result = await ControllersTestsUtils.GetFromResponse<GetUserEventsResultDto>(response);
            Assert.NotNull(result);
            Assert.NotNull(result.ReportedEvents);
            Assert.Empty(result.AssignedEvents);
            Assert.Equal(expectedCount, result.ReportedEvents.Count);
            for (int i = 0; i < result.ReportedEvents.Count; i++)
            {
                var resultEvent = result.ReportedEvents[i];
                Assert.Contains(resultEvent.Status.ToString(), statusFilter.Split(","));
            }
        }


        [Fact]
        public async Task GETEventsAmz_UserWithNoEvents_Should_ReturnOkCode200()
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, "user-with-no-events");
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<GetUserEventsResultDto>(response);
            Assert.NotNull(result);
            Assert.NotNull(result.ReportedEvents);
            Assert.Empty(result.ReportedEvents);
        }

        [Fact]
        public async Task GETEventsAmr_Should_ReturnOkCode200()
        {
            string expectedIdentityId = fixture.ResponderIdentityId;

            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleClaim, Roles.RESPONDER);
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, expectedIdentityId);

            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");
            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await ControllersTestsUtils.GetFromResponse<GetUserEventsResultDto>(response);
            Assert.NotNull(result);
            Assert.NotNull(result.AssignedEvents);
            Assert.Equal(4, result.AssignedEvents.Count);
            for (int i = 0; i < result.AssignedEvents.Count; i++)
            {
                var expectedEvent = fixture.DbUserEvents[fixture.Reporter2][i];
                var resultEvent = result.AssignedEvents[i];

                Assert.Equal(expectedEvent.Id, resultEvent.Id);
                Assert.Equal(expectedEvent.Responders.Count, resultEvent.Responders.Count);

                var existsResponderWithId = resultEvent.Responders.Any(responder => responder.IdentityId == expectedIdentityId);
                Assert.True(existsResponderWithId, $"No responder with IdentityId {expectedIdentityId} found in event {resultEvent.Id}");
            }
        }

        [Theory]
        [MemberData(nameof(GetEventsWithStatusCases))]
        public async Task GETEventsAmr_WithStatus_Should_ReturnOkCode200(string statusFilter, HttpStatusCode expectedCode, int expectedCount)
        {
            string expectedIdentityId = fixture.ResponderIdentityId;

            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleClaim, Roles.RESPONDER);
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, expectedIdentityId);

            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}?status={statusFilter}");
            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(expectedCode, response.StatusCode);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            var result = await ControllersTestsUtils.GetFromResponse<GetUserEventsResultDto>(response);
            Assert.NotNull(result);
            Assert.NotNull(result.AssignedEvents);
            Assert.Equal(expectedCount, result.AssignedEvents.Count);
            for (int i = 0; i < result.AssignedEvents.Count; i++)
            {
                var expectedEvent = fixture.DbUserEvents[fixture.Reporter2][i];
                var resultEvent = result.AssignedEvents[i];

                Assert.Contains(resultEvent.Status.ToString(), statusFilter.Split(","));
                var existsResponderWithId = resultEvent.Responders.Any(responder => responder.IdentityId == expectedIdentityId);
                Assert.True(existsResponderWithId, $"No responder with IdentityId {expectedIdentityId} found in event {resultEvent.Id}");
            }
        }

        [Fact]
        public async Task PUTEvent_Should_ReturnNoContentCode204()
        {
            var location = fixture.CancelLocation;
            var reportedEvent = fixture.DbEvents[location][0];
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");
            var eventStatus = EventStatusType.Cancelled;

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), eventStatus);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task PUTEvent_Should_ReturnBadRequest400()
        {
            var location = fixture.CancelLocation;
            var reportedEvent = fixture.DbEvents[location][1];
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");
            var eventStatus = EventStatusType.Accepted;

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), eventStatus);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PUTEvent_Should_ReturnConflict409()
        {
            var location = fixture.CancelLocation;
            var reportedEvent = fixture.DbEvents[location][2];
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{reportedEvent.Id}");
            var eventStatus = EventStatusType.Cancelled;

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), eventStatus);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task POSTResponder_Should_ReturnNoContentCode204()
        {
            var location = fixture.WriteOnlyLocation;
            var reportedEvent = fixture.DbEvents[location].First();
            var uriBuilder = new UriBuilder(GetResponderRoute(reportedEvent.Id));

            var response = await client.PostAsync(uriBuilder.ToString(), null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task POSTResponder_Should_ReturnNotFoundCode404()
        {
            var uriBuilder = new UriBuilder(GetResponderRoute("-1"));

            var response = await client.PostAsync(uriBuilder.ToString(), null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var result = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
            Assert.Equal(
                ResourceNotFoundException.WithId<ReportedEvent>("-1").Message,
                result.Detail);
        }

        [Fact]
        public async Task PUTResponder_Should_ReturnNoContentCode204()
        {
            var location = fixture.WriteOnlyLocation;
            var reportedEvent = fixture.DbEvents[location].Last();
            var savedResponder = reportedEvent.Responders.First();
            var uriBuilder = new UriBuilder(GetResponderRoute(reportedEvent.Id, savedResponder.IdentityId));
            var responderData = GetUpdateData();

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), responderData);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task PUTResponder_Should_ReturnNotFoundCode404()
        {
            var location = fixture.WriteOnlyLocation;
            var reportedEvent = fixture.DbEvents[location][1];
            var uriBuilder = new UriBuilder(GetResponderRoute(reportedEvent.Id, fixture.ResponderIdentityId));
            var responderData = GetUpdateData();

            var response = await client.PutAsJsonAsync(uriBuilder.ToString(), responderData);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var result = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
            Assert.Equal(
                ResponderNotRelatedToEventDomainException.WithId(fixture.ResponderIdentityId, reportedEvent.Id).Message,
                result.Detail);
        }

        private string GetResponderRoute(string eventId, string? responderId = null)
        {
            var route = $"{BaseUrl}/{EventsRoute}/{eventId}/{ApiRoutes.RESPONDERS_RESOURCE}";
            if (route != null)
            {
                route += $"/{responderId}";
            }
            return route!;
        }

        private ResponderInputDto GetUpdateData()
        {
            return new ResponderInputDto()
            {
                Status = ResponderStatusType.Accepted,
            };
        }

        public static IEnumerable<object[]> GetEventsWithStatusCases() => new List<object[]> {
                new object[]
                {
                    "Pending",
                    HttpStatusCode.OK,
                    1
                },
                new object[]
                {
                    "Accepted",
                    HttpStatusCode.OK,
                    1
                },
                new object[]
                {
                    "Cancelled",
                    HttpStatusCode.OK,
                    1
                },
                new object[]
                {
                    "Completed",
                    HttpStatusCode.OK,
                    1
                },
                new object[]
                {
                    "Pending,Completed",
                    HttpStatusCode.OK,
                    2
                },
                new object[]
                {
                    "Pending,Accepted,Completed,Cancelled",
                    HttpStatusCode.OK,
                    4
                },
                new object[]
                {
                    "UnknownStatus",
                    HttpStatusCode.BadRequest,
                    0
                },
                new object[]
                {
                    "Pending,UnknownStatus",
                    HttpStatusCode.BadRequest,
                    0
                }
            };
    }
}
