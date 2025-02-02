using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistance.Core;
using Infrastructure.Coordinator.Common;
using IntegrationTests.Common.Fixtures.EventCoordinator;
using IntegrationTests.Common.Fixtures;
using Domain.ValueObjects;
using Domain.Entities.User;
using Domain.Enums;

namespace IntegrationTests.Infrastructure.EventCoordinator
{
    public class ResponderSearchHelperTests : IClassFixture<ResponderSearchHelperFixture>
    {
        private readonly ResponderSearchHelperFixture fixture;
        private readonly IDocumentRepository<BasicUser> repository;


        public ResponderSearchHelperTests(ResponderSearchHelperFixture fixture)
        {
            this.fixture = fixture;
            var scope = fixture.Application.Services.GetRequiredService<IServiceScopeFactory>();
            this.repository = scope.CreateScope().ServiceProvider.GetRequiredService<IDocumentRepository<BasicUser>>();
        }

        [Theory]
        [MemberData(nameof(Locations))]
        public async Task IsResponderNearbyEventFilter_Should_Find(Coordinates location)
        {
            var query = ResponderSearchHelper.IsResponderNearbyEventFilter("@coordinates", "@radius", "u");
            var vars = new Dictionary<string, object>()
            {
                { "coordinates", new double[2]
                    {
                        location.Longitude,
                        location.Latitude
                    }
                },
                { "radius", 5000 }
            };

            var found = (await GetSearchResult(query, vars)).ToList();

            Assert.Equal(fixture.DbRespondersNearby[location].Count, found.Count);
            for (int i = 0; i < fixture.DbRespondersNearby[location].Count; i++)
            {
                Assert.Equivalent(fixture.DbRespondersNearby[location][i], found[i]);
            }
        }

        [Fact]
        public async Task IsResponderOnDutyFilter_Should_Find()
        {
            var schedule = ResponderSearchHelperFixture.TestSchedule;
            var query = ResponderSearchHelper.IsResponderOnDutyFilter("u._key", schedule.Item2, TimeSpan.FromHours(schedule.Item1));

            var found = (await GetSearchResult(query, null)).ToList();

            Assert.Equal(fixture.DbRespondersOnDuty.Count, found.Count);
            for (int i = 0; i < fixture.DbRespondersOnDuty.Count; i++)
            {
                Assert.Equivalent(fixture.DbRespondersOnDuty[i], found[i]);
            }
        }

        [Fact]
        public async Task IsResponderNotAssignedToEventsFilter_Should_Find()
        {
            var query = ResponderSearchHelper.IsResponderNotAssignedToEventsFilter("@event_ref", "u");
            var vars = new Dictionary<string, object>()
            {
                { "event_ref", ResponderSearchHelperFixture.TestNotAssignedToEventId },
            };

            var found = (await GetSearchResult(query, vars)).ToList();

            Assert.Equal(fixture.DbRespondersNotAssignedToEvents.Count, found.Count);
            for (int i = 0; i < fixture.DbRespondersNotAssignedToEvents.Count; i++)
            {
                Assert.Equivalent(fixture.DbRespondersNotAssignedToEvents[i], found[i]);
            }
        }

        [Fact]
        public async Task IsResponderNotCreatorOfEventFilter_Should_Find()
        {
            var query = ResponderSearchHelper.IsResponderNotCreatorOfEventFilter("@event_ref", "u");
            var vars = new Dictionary<string, object>()
            {
                { "event_ref", fixture.TestNotCreatorOfEventId },
            };

            var found = (await GetSearchResult(query, vars)).ToList();

            Assert.Equal(fixture.DbRespondersNotCreatorOfEvent.Count, found.Count);
            for (int i = 0; i < fixture.DbRespondersNotCreatorOfEvent.Count; i++)
            {
                Assert.Equivalent(fixture.DbRespondersNotCreatorOfEvent[i], found[i]);
            }
        }

        public static IEnumerable<object[]> Locations =>
            new List<object[]>
            {
                new object[] { EventCoordinatorFixture.LocationA },
                new object[] { EventCoordinatorFixture.LocationB },
                new object[] { EventCoordinatorFixture.LocationC },
            };

        private async Task<IEnumerable<object>> GetSearchResult(
            string responderFilter, Dictionary<string, object> vars = null)
        {
            var query = GetSearchQuery(responderFilter);
            return await repository.Execute(query, vars);
        }

        private string GetSearchQuery(string responderFilter)
        {
            return
                $"FOR u in {GlobalCollections.USERS} " +
                $"FILTER u.role == '{UserRoles.Responder}' " +
                $"{responderFilter} " +
                $"RETURN u";
        }
    }
}