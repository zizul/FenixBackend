using Domain.Enums;

namespace Domain.Entities.Event.DomainEvents
{
    public class EventCompletedDomainEvent : IDomainEvent
    {
        public string Id { get; }
        public EventStatusType Status { get; }


        public EventCompletedDomainEvent(string id, EventStatusType status) 
        { 
            Id = id; 
            Status = status;
        }
    }
}
