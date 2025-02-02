using Domain.Entities.Event;
using Domain.Entities.User;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;
using IntegrationTests.Common.Fixtures.EventCoordinator;

namespace IntegrationTests.Common.Fixtures
{
    public class ResponderSearchHelperFixture : EventCoordinatorFixture
    {
        public Dictionary<Coordinates, List<BasicUser>> DbRespondersNearby { get; set; }
        public List<BasicUser> DbRespondersOnDuty { get; set; }
        public List<BasicUser> DbRespondersNotAssignedToEvents { get; set; }
        public List<BasicUser> DbRespondersNotCreatorOfEvent { get; set; }

        public static (int, DayOfWeek) TestSchedule = (12, DayOfWeek.Friday);
        public static string TestNotAssignedToEventId = "123";
        public string TestNotCreatorOfEventId;


        public ResponderSearchHelperFixture()
        {
            DbRespondersNearby = new Dictionary<Coordinates, List<BasicUser>>();
            DbRespondersOnDuty = new List<BasicUser>();
            DbRespondersNotAssignedToEvents = new List<BasicUser>();
            DbRespondersNotCreatorOfEvent = new List<BasicUser>();
        }

        protected override async Task InitData()
        {
            var user1 = await CreateUser("0", UserRoles.User);
            var user2 = await CreateUser("1", UserRoles.User);
            DbReporters.Add(user1);
            DbReporters.Add(user2);

            await AddLocationA();
            await AddLocationB();
            await AddLocationC();
        }

        private async Task AddLocationA()
        {
            // location A available (6)
            await AddResponders(LocationA, 1);
            await AddResponders(LocationA, 2, "456", ResponderStatusType.Rejected);
            await AddResponders(LocationA, 1, "456", ResponderStatusType.Completed);
            await AddResponders(LocationA, 2, "123", ResponderStatusType.Rejected);

            // location A assigned (9)
            await AddResponders(LocationA, 2, "123", ResponderStatusType.Pending);
            await AddResponders(LocationA, 3, "123", ResponderStatusType.Accepted);
            await AddResponders(LocationA, 1, "123", ResponderStatusType.Arrived);
            await AddResponders(LocationA, 3, "456", ResponderStatusType.Pending);
            DbRespondersNearby[LocationA] = DbResponders[LocationA];

            DbRespondersNotAssignedToEvents.AddRange(DbResponders[LocationA].GetRange(0, 4).ToList());

            // location A on duty (2)
            await AddReadiness(LocationA, 0, 2, ReadinessStatus.BySchedule, GetReadinessRangeArray(true, TestSchedule.Item1, TestSchedule.Item2));
            await AddReadiness(LocationA, 2, 4, ReadinessStatus.NotReady, GetReadinessRangeArray(true, TestSchedule.Item1, TestSchedule.Item2));
            await AddReadiness(LocationA, 4, 15, ReadinessStatus.NotReady);
            DbRespondersOnDuty.AddRange(DbResponders[LocationA].Take(2).ToList());

            // location A creators (1)
            var eventId = await AddEvent(LocationA, 0, TestNotCreatorOfEventId);
            TestNotCreatorOfEventId = eventId;
            DbRespondersNotCreatorOfEvent.AddRange(DbResponders[LocationA].GetRange(1, 14).ToList());
        }

        private async Task AddLocationB()
        {
            // location B available (6)
            await AddResponders(LocationB, 1);
            await AddResponders(LocationB, 2, "789", ResponderStatusType.Completed);
            await AddResponders(LocationB, 7, "789", ResponderStatusType.Incompleted);
            DbRespondersNearby[LocationB] = DbResponders[LocationB];

            DbRespondersNotAssignedToEvents.AddRange(DbResponders[LocationB]);

            // 2 off duty
            await AddReadiness(LocationB, 0, 3, ReadinessStatus.NotReady);
            await AddReadiness(LocationB, 3, 4, ReadinessStatus.NotReady, GetReadinessRangeArray(true, TestSchedule.Item1, TestSchedule.Item2));

            // 3 on duty
            await AddReadiness(LocationB, 4, 5, ReadinessStatus.Ready, GetReadinessRangeArray(false, TestSchedule.Item1, TestSchedule.Item2));
            await AddReadiness(LocationB, 5, 6, ReadinessStatus.Ready, GetReadinessRangeArray(true, TestSchedule.Item1, TestSchedule.Item2));
            await AddReadiness(LocationB, 6, 7, ReadinessStatus.Ready);

            // 1 on duty with ReadinessStatus==BySchedule, all ranges on
            await AddReadiness(LocationB, 7, 8, ReadinessStatus.BySchedule, GetReadinessRangeArray(true, TestSchedule.Item1, TestSchedule.Item2));
            // 1 off duty because readiness ranges are empty
            await AddReadiness(LocationB, 8, 9, ReadinessStatus.BySchedule);
            // 1 off duty because all readiness ranges are off
            await AddReadiness(LocationB, 9, 10, ReadinessStatus.BySchedule, GetReadinessRangeArray(false, TestSchedule.Item1, TestSchedule.Item2));

            // location B 4 on duty
            DbRespondersOnDuty.AddRange(DbResponders[LocationB].GetRange(4, 4).ToList());

            DbRespondersNotCreatorOfEvent.AddRange(DbResponders[LocationB]);
        }

        private async Task AddLocationC()
        {
            // location C available (1)
            await AddResponders(LocationC, 1);

            // location C assigned (5)
            await AddResponders(LocationC, 2, "0123", ResponderStatusType.Accepted);
            await AddResponders(LocationC, 3, "0123", ResponderStatusType.Arrived);
            DbRespondersNearby[LocationC] = DbResponders[LocationC];

            DbRespondersNotAssignedToEvents.AddRange(DbResponders[LocationC].Take(1).ToList());

            // 0 on duty
            await AddReadiness(LocationC, 0, 1, ReadinessStatus.NotReady);
            await AddReadiness(LocationC, 0, 2, ReadinessStatus.NotReady);
            await AddReadiness(LocationC, 2, 5, ReadinessStatus.NotReady);
            DbRespondersOnDuty.AddRange(DbResponders[LocationC].Take(0).ToList());

            DbRespondersNotCreatorOfEvent.AddRange(DbResponders[LocationC]);
        }

        private async Task AddReadiness(
            Coordinates location,
            int fromIndex,
            int toIndex,
            ReadinessStatus readinessStatus,
            ReadinessRange[]? rangesArray = null
            )
        {
            rangesArray ??= Array.Empty<ReadinessRange>();
            var responders = DbResponders[location].Skip(fromIndex).Take((toIndex - fromIndex));
            foreach (var responder in responders)
            {
                await AddReadinessInDb(responder.Id, readinessStatus, rangesArray);
            }
        }

        private ReadinessRange[] GetReadinessRangeArray(bool rangeEnabled, int hours, DayOfWeek day)
        {
            return new ReadinessRange[] { GetReadinessRange(rangeEnabled, hours, day) };
        }

        private ReadinessRange GetReadinessRange(bool rangeEnabled, int hours, DayOfWeek day)
        {
            return new ReadinessRange(rangeEnabled, TimeSpan.FromHours(hours - 1), TimeSpan.FromHours(hours + 1), day);
        }

        private async Task<string> AddEvent(Coordinates location, int index, string eventId)
        {
            var reportedEvent = new ReportedEvent()
            {
                Id = eventId,
                Reporter = new Reporter()
                {
                    UserId = DbResponders[location].ElementAt(index).Id
                },
                Status = EventStatusType.Pending,
                Coordinates = new Coordinates(0, 0),
            };

            var result = await CreateDocument(GlobalCollections.EVENTS, reportedEvent);
            return result.Id;
        }
    }
}