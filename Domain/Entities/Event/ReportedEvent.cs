using Domain.Entities.Event.DomainEvents;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Event
{
    /// <summary>
    /// Aggregate root for emergency event lifecycle.
    /// 
    /// Properties use public setters as a persistence compromise — ArangoDB's JSON
    /// deserializer (custom converters in Infrastructure layer) requires writable properties
    /// to rehydrate entities from the database. Ideally, state would only be mutable through
    /// domain methods (ReportEvent, ChangeEventStatus, AssignResponder, etc.).
    /// All state transitions MUST go through domain methods to enforce invariants.
    /// </summary>
    public class ReportedEvent : AggregateRoot
    {
        /// <summary>
        /// Maximum time an event can stay open before automatic cancellation.
        /// </summary>
        private const double AutoCancelAfterHours = 1.0;

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
            RegisterDomainEvent(new EventReportedDomainEvent(EventId: Id));
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

            // Find uses List<T>.Find — no enumerator allocation, O(n) scan with early exit
            if (Responders.Find(x => x.IdentityId == identityId) is not null)
            {
                throw ResponderAlreadyAssignedDomainException.WithId(identityId, Id);
            }

            var responder = new Responder(
                eventId: Id,
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

        /// <summary>
        /// Finds a responder by identity. Uses List.Find (no allocation) instead of
        /// LINQ Single + try/catch, avoiding exception-driven control flow.
        /// </summary>
        private Responder GetResponder(string responderId)
        {
            var responder = Responders.Find(r => r.IdentityId == responderId);
            if (responder is null)
            {
                throw ResponderNotRelatedToEventDomainException.WithId(responderId, Id);
            }
            return responder;
        }

        private bool IsEventClosed()
        {
            return Status == EventStatusType.Completed ||
                   Status == EventStatusType.Cancelled;
        }

        /// <summary>
        /// Re-evaluates the aggregate status based on responder states and elapsed time.
        /// Called internally after responder updates; also called by EventCoordinatorService
        /// during periodic responder search to detect time-based cancellation.
        /// </summary>
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

            // Check if all responders are in non-active states (rejected or still pending)
            // List<T>.TrueForAll avoids LINQ enumerator allocation, equivalent to All()
            return Responders.TrueForAll(
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

            // List<T>.Exists avoids LINQ enumerator allocation, equivalent to Any()
            return Responders.Exists(
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

            return Responders.Exists(
                x => x.Status == ResponderStatusType.Completed);
        }

        private void CompleteEvent()
        {
            Status = EventStatusType.Completed;
            ClosedAt = DateTime.UtcNow;

            for (var i = 0; i < Responders.Count; i++)
            {
                Responders[i].UpdateResponderStatusOnEventCompleted();
            }

            // Event raised AFTER state mutation — represents a committed fact
            RegisterDomainEvent(new EventClosedDomainEvent(EventId: Id, FinalStatus: Status));
        }

        private bool IsEventShouldBeCancelledByTime()
        {
            if (Status == EventStatusType.Cancelled || IsCancelStateNotPossible())
            {
                return false;
            }

            return (DateTime.UtcNow - CreatedAt).TotalHours > AutoCancelAfterHours;
        }

        private bool IsCancelStateNotPossible()
        {
            return Status != EventStatusType.Pending &&
                   Status != EventStatusType.Accepted;
        }

        private void CancelEvent()
        {
            Status = EventStatusType.Cancelled;
            ClosedAt = DateTime.UtcNow;

            for (var i = 0; i < Responders.Count; i++)
            {
                Responders[i].UpdateResponderStatusOnEventCancelled();
            }

            // Event raised AFTER state mutation — represents a committed fact
            RegisterDomainEvent(new EventClosedDomainEvent(EventId: Id, FinalStatus: Status));
        }
    }
}
