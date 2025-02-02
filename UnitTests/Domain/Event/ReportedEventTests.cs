using Domain.Entities.Event;
using Domain.Entities.Event.DomainEvents;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;

namespace UnitTests.Domain.Event
{
    public class ReportedEventTests
    {
        [Fact]
        public void ReportEvent_Should_RaiseDomainEvent()
        {
            var reportedEvent = CreateEvent("123", EventStatusType.Pending);

            reportedEvent.ReportEvent();

            Assert.Equal(EventStatusType.Pending, reportedEvent.Status);
            Assert.Collection(reportedEvent.DomainEvents,
                x => Assert.Equivalent(new EventReportedDomainEvent(reportedEvent.Id), x));
        }

        [Theory]
        [InlineData(EventStatusType.Pending)]
        [InlineData(EventStatusType.Accepted)]
        public void ChangeEventStatus_Should_ChangeEventState(EventStatusType current)
        {
            var reportedEvent = CreateEvent("123", current);

            reportedEvent.ChangeEventStatus(EventStatusType.Cancelled);

            Assert.Equal(EventStatusType.Cancelled, reportedEvent.Status);
            Assert.Collection(reportedEvent.DomainEvents,
                x => Assert.Equivalent(
                    new EventCompletedDomainEvent(reportedEvent.Id, EventStatusType.Cancelled), x));
        }

        [Theory]
        [InlineData(EventStatusType.Completed, EventStatusType.Cancelled, typeof(EventIsClosedDomainException))]
        [InlineData(EventStatusType.Accepted, EventStatusType.Pending, typeof(ForbiddenEventStatusDomainException))]
        public void ChangeEventStatus_Should_ThrowDomainException(
            EventStatusType current, EventStatusType next, Type expectedException)
        {
            var reportedEvent = CreateEvent("123", current);

            Action act = () => reportedEvent.ChangeEventStatus(next);

            Assert.Throws(expectedException, act);
        }

        [Theory]
        [InlineData(EventStatusType.Pending, EventStatusType.Cancelled)]
        [InlineData(EventStatusType.Accepted, EventStatusType.Cancelled)]
        public void UpdateEventStatus_Should_CancelEventAfterTime(
            EventStatusType current, EventStatusType expected)
        {
            var reportedEvent = CreateEvent("123", current);
            // simulate time passing
            reportedEvent.CreatedAt = DateTime.UtcNow.AddHours(-2);

            reportedEvent.UpdateEventStatus();

            Assert.Equal(expected, reportedEvent.Status);
            Assert.Collection(reportedEvent.DomainEvents,
                x => Assert.Equivalent(
                    new EventCompletedDomainEvent(reportedEvent.Id, EventStatusType.Cancelled), x));
        }

        [Fact]
        public void AssignResponder_Should_ThrowResponderAlreadyAssigned()
        {
            var reportedEvent = CreateEvent("123", EventStatusType.Pending);
            reportedEvent.AssignResponder("0");

            Action act = () => reportedEvent.AssignResponder("0");

            Assert.Throws<ResponderAlreadyAssignedDomainException>(act);
        }

        [Fact]
        public void AssignResponder_Should_ThrowEventIsClosed()
        {
            var reportedEvent = CreateEvent("123", EventStatusType.Completed);

            Action act = () => reportedEvent.AssignResponder("0");

            Assert.Throws<EventIsClosedDomainException>(act);
        }

        [Fact]
        public void AcceptRequest_Should_ChangeEventState()
        {
            var reportedEvent = CreateEvent("123", EventStatusType.Pending);

            reportedEvent.AssignResponder("0");
            reportedEvent.UpdateResponder("0", ResponderStatusType.Accepted, null, null, null);

            Assert.Equal(EventStatusType.Accepted, reportedEvent.Status);
            Assert.Collection(reportedEvent.Responders,
                x => Assert.Equal(ResponderStatusType.Accepted, x.Status));
            Assert.Empty(reportedEvent.DomainEvents);
        }

        [Theory]
        [InlineData(new ResponderStatusType[] { ResponderStatusType.Pending }, EventStatusType.Pending, EventStatusType.Pending)]
        [InlineData(new ResponderStatusType[] { ResponderStatusType.Accepted }, EventStatusType.Accepted, EventStatusType.Pending)]
        [InlineData(new ResponderStatusType[] { ResponderStatusType.Arrived }, EventStatusType.Accepted, EventStatusType.Pending)]
        [InlineData(new ResponderStatusType[] { ResponderStatusType.Accepted, ResponderStatusType.Pending }, EventStatusType.Accepted, EventStatusType.Pending)]
        [InlineData(new ResponderStatusType[] { ResponderStatusType.Arrived, ResponderStatusType.Pending }, EventStatusType.Accepted, EventStatusType.Pending)]
        public void RejectRequest_Should_SetCorrectEventState(
            ResponderStatusType[] responderStatuses,
            EventStatusType current, 
            EventStatusType expected)
        {
            
            var reportedEvent = CreateEvent("123", current, responderStatuses);

            reportedEvent.UpdateResponder("0", ResponderStatusType.Rejected, null, null, null);

            Assert.Equal(expected, reportedEvent.Status);
            Assert.Equal(ResponderStatusType.Rejected, reportedEvent.Responders[0].Status);
            Assert.Empty(reportedEvent.DomainEvents);
        }

        [Fact]
        public void CompleteEvent_Should_ChangeRespondersState()
        {
            var responders = new ResponderStatusType[] {
                ResponderStatusType.Pending, 
                ResponderStatusType.Rejected, 
                ResponderStatusType.Arrived, 
                ResponderStatusType.Arrived,
                ResponderStatusType.Accepted,
            };
            var reportedEvent = CreateEvent("123", EventStatusType.Accepted, responders);

            reportedEvent.UpdateResponder("2", ResponderStatusType.Completed, null, null, null);

            Assert.Equal(EventStatusType.Completed, reportedEvent.Status);
            Assert.Collection(reportedEvent.Responders,
                x => Assert.Equal(ResponderStatusType.Incompleted, x.Status),
                x => Assert.Equal(ResponderStatusType.Rejected, x.Status),
                x => Assert.Equal(ResponderStatusType.Completed, x.Status),
                x => Assert.Equal(ResponderStatusType.Completed, x.Status),
                x => Assert.Equal(ResponderStatusType.Incompleted, x.Status));
            Assert.Collection(reportedEvent.DomainEvents,
                x => Assert.Equivalent(
                    new EventCompletedDomainEvent(reportedEvent.Id, EventStatusType.Completed), x));
        }

        private ReportedEvent CreateEvent(
            string eventId, EventStatusType eventStatus, params ResponderStatusType[] responderStatuses)
        {
            var reportedEvent = new ReportedEvent();
            reportedEvent.Status = eventStatus;
            reportedEvent.Coordinates = new Coordinates(1, 1);
            reportedEvent.Id = eventId;
            // responder id: <0, ..., responderStatuses.Length)
            reportedEvent.Responders = responderStatuses.Select(
                (x, i) => CreateResponder(eventId, i.ToString(), x)).ToList();
            return reportedEvent;
        }

        private Responder CreateResponder(string eventId, string responderId, ResponderStatusType status)
        {
            return new Responder(eventId, responderId, status);
        }
    }
}
