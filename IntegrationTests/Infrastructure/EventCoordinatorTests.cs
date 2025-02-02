using Presentation.Common;
using System.Net.Http.Json;
using System.Net;
using Domain.ValueObjects;
using Application.Services.Event.DTOs.Common;
using Domain.Enums;
using Application.Services.Event.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Domain.Entities.Event.DomainExceptions;
using IntegrationTests.Common;
using IntegrationTests.Common.Fixtures.EventCoordinator;
using Infrastructure.Notifications;
using IntegrationTests.Common.Stubs;

namespace IntegrationTests.Infrastructure
{
    public class EventCoordinatorTests : IClassFixture<EventCoordinatorFixture>
    {
        private string BaseUrl => fixture.Application.BaseUrl;
        private string EventsRoute => fixture.Application.GetRoute(ApiRoutes.EVENTS_ROUTE);

        private readonly EventCoordinatorFixture fixture;
        private readonly IEventCoordinatorService coordinator;
        private readonly IAppNotifier appNotifier;
        private readonly HttpClient client;


        public EventCoordinatorTests(EventCoordinatorFixture fixture)
        {
            this.fixture = fixture;
            this.coordinator = fixture.Application.Services.GetRequiredService<IEventCoordinatorService>();
            this.appNotifier = fixture.Application.Services.GetRequiredService<IAppNotifier>();
            this.client = fixture.Client;
        }

        [Fact]
        public async Task Coordinator_Should_AssignRespondersToEvent()
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, fixture.DbReporters.Last().IdentityId);
            var locationA = EventCoordinatorFixture.LocationA;
            var idA = await ReportEvent(locationA);

            try
            {
                await coordinator.TryFindAndAssignRespondersToEvent(idA, 5);
            }
            // responder also can be assigned by a running thread
            catch (ResponderAlreadyAssignedDomainException e) { }

            var reportedEventA = await GetEvent(idA);
            Assert.Equal(fixture.DbAvailableResponders[locationA].Count, reportedEventA.Responders.Count);
            var sentMessages = (appNotifier as AppNotifierStub).SentMulticastMessages;
            Assert.Equal(fixture.DbAvailableResponders[locationA].Count, sentMessages.First().Tokens.Count);
        }

        [Fact]
        public async Task Coordinator_Should_CancelEventAfterTime()
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdClaim, fixture.DbReporters.Last().IdentityId);
            var locationB = EventCoordinatorFixture.LocationB;
            var idB = await ReportEvent(locationB);
            await fixture.ExpireEventCreatedAt(idB);

            try
            {
                await coordinator.TryFindAndAssignRespondersToEvent(idB, 5);
            }
            // event also can be closed by a running thread
            catch (EventIsClosedDomainException e) { }

            var reportedEventB = await GetEvent(idB);
            Assert.Equal(EventStatusType.Cancelled, reportedEventB.Status);
        }

        private async Task<string> ReportEvent(Coordinates coordinates)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}");
            var eventData = GetEventData(coordinates);

            var response = await client.PostAsJsonAsync(uriBuilder.ToString(), eventData);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            string jsonContent = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ReportedEventResultDto>(jsonContent);
            return result!.Id;
        }

        private async Task<ReportedEventResultDto> GetEvent(string id)
        {
            var uriBuilder = new UriBuilder($"{BaseUrl}/{EventsRoute}/{id}");

            var response = await client.GetAsync(uriBuilder.ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string jsonContent = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ReportedEventResultDto>(jsonContent);
            return result!;
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
    }
}