using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using Domain.Entities.Event;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace UnitTests.Domain.Event
{
    public class ResponderTests
    {
        [Fact]
        public void NullUpdate_Should_NotSetNull()
        {
            var eventStatus = EventStatusType.Pending;
            var expectedResponderStatus = ResponderStatusType.Accepted;
            var expectedResponderTransportType = TransportType.Car;
            var expectedResponderEta = DateTime.UtcNow.AddMinutes(15);
            var expectedResponderCoordinates = new Coordinates(5.5, 5.5);

            var reportedEvent = CreateEventWithAssignedResponder("123", "0", eventStatus, expectedResponderStatus, expectedResponderTransportType, expectedResponderEta, expectedResponderCoordinates);

            // nothing should change in responder, no timeline entry should be added
            reportedEvent.UpdateResponder("0", status: null, transport: null, eta: null, coordinates: null);

            var responderFromEvent = reportedEvent.Responders.First(x => x.IdentityId == "0");
            Assert.Equivalent(expectedResponderCoordinates, responderFromEvent.Coordinates);
            Assert.Equal(expectedResponderEta, (DateTime)responderFromEvent.ETA!, TimeSpan.FromMinutes(1));
            Assert.Equal(expectedResponderTransportType, responderFromEvent.Transport);
            Assert.Equal(expectedResponderStatus, responderFromEvent.Status);

            // new timeline entry should not be added, first timeline entry transport, ETA should not change after UpdateResponder()
            Assert.Equal(1, responderFromEvent.Timeline.Count);
            Assert.Equal(expectedResponderStatus, responderFromEvent.Timeline.Last().Status);
            Assert.Equal(expectedResponderTransportType, responderFromEvent.Timeline.Last().Transport);
        }

        [Fact]
        public void Update_Should_ChangeData()
        {
            var reportedEvent = CreateEventWithAssignedResponder("123", "0");
            var updateDto = GetUpdateDto("123", "0");
            Coordinates coordinates = new Coordinates(1, 1);


            reportedEvent.UpdateResponder(
                updateDto.IdentityId,
                updateDto.ResponderData.Status,
                updateDto.ResponderData.Transport,
                updateDto.ResponderData.ETA,
                coordinates);

            var responderFromEvent = reportedEvent.Responders.First(x => x.IdentityId == "0");
            Assert.Equal(updateDto.ResponderData.ETA, responderFromEvent.ETA);
            Assert.Equal(updateDto.ResponderData.Transport, responderFromEvent.Transport);
            Assert.Equal(updateDto.ResponderData.Status, responderFromEvent.Status);

            Assert.Equal(2, responderFromEvent.Timeline.Count);
            Assert.Equal(updateDto.ResponderData.Status, responderFromEvent.Timeline.Last().Status);
            Assert.Equal(updateDto.ResponderData.Transport, responderFromEvent.Timeline.Last().Transport);
            Assert.Equal(updateDto.ResponderData.ETA, responderFromEvent.Timeline.Last().ETA);
            Assert.Equal(coordinates, responderFromEvent.Timeline.Last().Coordinates);
        }

        [Fact]
        public void Update_Should_ThrowNotRelatedToEvent()
        {
            var reportedEvent = CreateEventWithAssignedResponder("123", "0");

            var act = () => reportedEvent.UpdateResponder("-1", ResponderStatusType.Arrived, null, null, null);

            Assert.Throws<ResponderNotRelatedToEventDomainException>(act);
        }

        [Fact]
        public void Update_Should_ThrowEventIsClosed()
        {
            var reportedEvent = CreateEventWithAssignedResponder("123", "0", EventStatusType.Completed);

            var act = () => reportedEvent.UpdateResponder("0", ResponderStatusType.Arrived, null, null, null);

            Assert.Throws<EventIsClosedDomainException>(act);
        }

        [Fact]
        public void Update_Should_ThrowInvalidState()
        {
            var reportedEvent = CreateEventWithAssignedResponder("123", "0");

            var act = () => reportedEvent.UpdateResponder("0", ResponderStatusType.Pending, null, null, null);

            Assert.Throws<ResponderInvalidStateDomainException>(act);
        }

        private static ReportedEvent CreateEventWithAssignedResponder(
            string eventId, string responderId,
            EventStatusType eventStatus = EventStatusType.Pending, 
            ResponderStatusType responderStatus = ResponderStatusType.Accepted, 
            TransportType transportType = TransportType.Car,
            DateTime? eta = null,
            Coordinates? coordinates = null)
        {
            eta ??= DateTime.UtcNow.AddMinutes(15);
            coordinates ??= new Coordinates(5.5, 5.5);
            return new ReportedEvent()
            {
                Id = eventId,
                Responders = new List<Responder>() { CreateResponder(eventId, responderId, responderStatus, transportType, eta, coordinates) },
                Status = eventStatus
            };
        }

        private static Responder CreateResponder(string eventId, string userId, ResponderStatusType responderStatus, TransportType transportType, DateTime? eta, Coordinates coordinates)
        {
            return new Responder
            (
                eventId,
                userId,
                responderStatus,
                transportType,
                eta,
                coordinates
            );
        }

        private UpdateResponderStatusCommandDto GetUpdateDto(string eventId, string responderId)
        {
            return new UpdateResponderStatusCommandDto(
                eventId, 
                responderId, 
                new ResponderInputDto()
                {
                    ETA = DateTime.UtcNow.AddMinutes(7),
                    Transport = TransportType.Walking,
                    Status = ResponderStatusType.Rejected,
                }
            );
        }
    }
}
