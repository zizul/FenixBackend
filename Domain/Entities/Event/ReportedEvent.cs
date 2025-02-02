using Domain.Entities.Event.DomainEvents;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Event
{
    public class ReportedEvent : AggregateRoot
    {
        public string Id { get; set; }
        public Coordinates Coordinates { get; set; }
        public Reporter Reporter { get; set; }
        public List<Responder> Responders { get; set; } = new List<Responder>();
        
        public string? EventType { get; set; }
        public int? InjuredCount { get; set; }
        public Address? Address { get; set; }
        public string? Description { get; set; }

        public EventStatusType Status { get; set; } = EventStatusType.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }


        public void ReportEvent()
        {
            Status = EventStatusType.Pending;
            RegisterDomainEvent(new EventReportedDomainEvent(Id));
        }

        public void ChangeEventStatus(EventStatusType newStatus)
        {
            if (newStatus == EventStatusType.Cancelled)
            {
                if (IsEventClosed())
                {
                    throw EventIsClosedDomainException.WithId(Id);
                }

                CancelEvent();
                return;
            }

            throw ForbiddenEventStatusDomainException.WithStatus(newStatus);
        }

        public void AssignResponder(string identityId, Coordinates? coordinates = null)
        {
            if (IsEventClosed())
            {
                throw EventIsClosedDomainException.WithId(Id);
            }

            if (Responders.Exists(x => x.IdentityId == identityId))
            {
                throw ResponderAlreadyAssignedDomainException.WithId(identityId, Id);
            }
            Responder responder = new (
                eventId : Id,
                identityId: identityId,
                status: ResponderStatusType.Pending,
                coordinates: coordinates
            );
            Responders.Add(responder);
        }

        public void UpdateResponder(
            string responderId,
            ResponderStatusType? status,
            TransportType? transport,
            DateTime? eta,
            Coordinates? coordinates)
        {
            if (IsEventClosed())
            {
                throw EventIsClosedDomainException.WithId(Id);
            }

            var responder = GetResponder(responderId);
            responder.UpdateResponder(status, transport, eta, coordinates);

            UpdateEventStatus();
        }

        private Responder GetResponder(string responderId)
        {
            try
            {
                return Responders.Single(r => r.IdentityId == responderId);
            }
            catch (InvalidOperationException)
            {
                throw ResponderNotRelatedToEventDomainException.WithId(responderId, Id);
            }
        }

        private bool IsEventClosed()
        {
            return (Status == EventStatusType.Completed ||
                    Status == EventStatusType.Cancelled);
        }

        public void UpdateEventStatus()
        {
            if (IsEventShouldBeCancelledByTime())
            {
                CancelEvent();
            }
            else if (IsEventShouldBePending())
            {
                SetPendingEvent();
            }
            else if (IsEventShouldBeAccepted())
            {
                AcceptEvent();
            }
            else if (IsEventShouldBeCompleted())
            {
                CompleteEvent();
            }
        }

        private bool IsEventShouldBePending()
        {
            if (Status == EventStatusType.Pending ||
                Status != EventStatusType.Accepted)
            {
                return false;
            }

            return Responders.All(
                x => x.Status == ResponderStatusType.Rejected || x.Status == ResponderStatusType.Pending);
        }

        private void SetPendingEvent()
        {
            Status = EventStatusType.Pending;
        }

        private bool IsEventShouldBeAccepted()
        {
            if (Status == EventStatusType.Accepted ||
                Status != EventStatusType.Pending)
            {
                return false;
            }

            return Responders.Any(
                x => x.Status == ResponderStatusType.Accepted);
        }

        private void AcceptEvent()
        {
            Status = EventStatusType.Accepted;
        }

        private bool IsEventShouldBeCompleted()
        {
            if (Status == EventStatusType.Completed ||
                Status != EventStatusType.Accepted)
            {
                return false;
            }

            return Responders.Any(
                x => x.Status == ResponderStatusType.Completed);
        }

        private void CompleteEvent()
        {
            Status = EventStatusType.Completed;
            ClosedAt = DateTime.UtcNow;

            foreach (var responder in Responders)
            {
                responder.UpdateResponderStatusOnEventCompleted();
            }

            RegisterDomainEvent(new EventCompletedDomainEvent(Id, Status));
        }

        private bool IsEventShouldBeCancelledByTime()
        {
            if (Status == EventStatusType.Cancelled || IsCancelStateNotPossible())
            {
                return false;
            }

            return (DateTime.UtcNow - CreatedAt).TotalHours > 1;
        }

        private bool IsCancelStateNotPossible()
        {
            return (Status != EventStatusType.Pending &&
                    Status != EventStatusType.Accepted);
        }

        private void CancelEvent()
        {
            Status = EventStatusType.Cancelled;
            ClosedAt = DateTime.UtcNow;

            foreach (var responder in Responders)
            {
                responder.UpdateResponderStatusOnEventCancelled();
            }

            RegisterDomainEvent(new EventCompletedDomainEvent(Id, Status));
        }
    }
}
