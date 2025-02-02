using Domain.Common;
using Domain.Entities.Event.DomainExceptions;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities.Event
{
    public class Responder
    {
        public string UserId { get; set; }
        public string IdentityId { get; set; }
        public string EventId { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public Timeline<ResponderTimelineEntry> Timeline { get; set; }
        public ResponderStatusType Status { get; set; } = ResponderStatusType.Pending;

        public TransportType? Transport { get; set; }
        public DateTime? ETA { get; set; }
        public Coordinates? Coordinates { get; set; }

        public Responder()
        {
            Timeline = new Timeline<ResponderTimelineEntry>();
        }

        public Responder(string eventId, string identityId, ResponderStatusType status, TransportType? transport = null, DateTime? eta = null, Coordinates? coordinates = null, string userId = null)
        {
            EventId = eventId;
            IdentityId = identityId;
            UserId = userId;
            Coordinates = coordinates;
            Status = status;
            Transport = transport;
            ETA = eta;

            Timeline = new Timeline<ResponderTimelineEntry>();
            Timeline.AddEntry(new ResponderTimelineEntry(status, transport, eta, coordinates));
            
        }

        public bool IsActiveOnCall()
        {
            return Status == ResponderStatusType.Pending ||
                   Status == ResponderStatusType.Accepted ||
                   Status == ResponderStatusType.Arrived;
        }

        internal void UpdateResponderStatusOnEventCompleted()
        {
            if (Status == ResponderStatusType.Arrived)
            {
                UpdateResponderStatus(ResponderStatusType.Completed);
            }
            else if (Status == ResponderStatusType.Accepted ||
                     Status == ResponderStatusType.Pending)
            {
                UpdateResponderStatus(ResponderStatusType.Incompleted);
            }
        }

        internal void UpdateResponderStatusOnEventCancelled()
        {
            if (IsActiveOnCall())
            {
                UpdateResponderStatus(ResponderStatusType.Incompleted);
            }
        }

        internal void UpdateResponder(ResponderStatusType? status, TransportType? transport, DateTime? eta, Coordinates? coordinates)
        {
            if (status == null && transport == null && eta == null && coordinates == null)
            {
                return;
            }

            ResponderTimelineEntry newStatusEntry = Timeline.AddEntry();

            if (status != null && Status != status)
            {
                if (!IsStatusChangePossible(status.Value))
                {
                    throw ResponderInvalidStateDomainException.WithStates(Status, status.Value);
                }

                Status = status.Value;
                newStatusEntry.Status = status.Value;
            }

            if (transport != null)
            {
                Transport = transport.Value;
                newStatusEntry.Transport = Transport;
            }

            if (eta != null)
            {
                ETA = eta.Value;
                newStatusEntry.ETA = ETA;
            }

            if (coordinates != null)
            {
                Coordinates = coordinates;
                newStatusEntry.Coordinates = Coordinates;
            }
        }

        internal void UpdateResponderStatus(ResponderStatusType status)
        {
            if (Status == status)
            {
                return;
            }

            if (!IsStatusChangePossible(status))
            {
                throw ResponderInvalidStateDomainException.WithStates(Status, status);
            }

            Status = status;
            ResponderTimelineEntry newStatusEntry = Timeline.AddEntry();
            newStatusEntry.Status = status;
        }

        internal void UpdateResponderTrackingData(
            TransportType? transport,
            DateTime? eta)
        {
            // check if all parameters null than return 
            if (transport == null && eta == null)
            {
                return;
            }

            ResponderTimelineEntry newStatusEntry = Timeline.AddEntry();
            if (transport != null)
            {
                Transport = transport.Value;
                newStatusEntry.Transport = Transport;
            }
            if (eta != null)
            {
                ETA = eta.Value;
                newStatusEntry.ETA = ETA;
            }
        }

        private bool IsStatusChangePossible(ResponderStatusType next)
        {
            if (next == ResponderStatusType.Pending)
            {
                return Status == ResponderStatusType.Pending;
            }
            else if (next == ResponderStatusType.Accepted)
            {
                return Status == ResponderStatusType.Pending;
            }
            else if (next == ResponderStatusType.Arrived)
            {
                return Status == ResponderStatusType.Accepted;
            }
            else if (next == ResponderStatusType.Completed)
            {
                return Status == ResponderStatusType.Arrived;
            }
            else if (next == ResponderStatusType.Incompleted)
            {
                return Status != ResponderStatusType.Completed && Status != ResponderStatusType.Rejected;
            }
            else if (next == ResponderStatusType.Rejected)
            {
                return Status != ResponderStatusType.Completed && Status != ResponderStatusType.Incompleted;
            }

            return false;
        }
    }
}
