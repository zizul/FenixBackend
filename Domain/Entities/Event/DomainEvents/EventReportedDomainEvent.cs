
namespace Domain.Entities.Event.DomainEvents
{
    public class EventReportedDomainEvent : IDomainEvent
    {
        public string Id { get; }


        public EventReportedDomainEvent(string id)
        {
            Id = id;
        }
    }
}
