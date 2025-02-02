
namespace Domain.Entities
{
    public abstract class AggregateRoot
    {
        public List<IDomainEvent> DomainEvents => domainEvents;

        private readonly List<IDomainEvent> domainEvents = new List<IDomainEvent>();


        protected void RegisterDomainEvent(IDomainEvent domainEvent)
        {
            domainEvents.Add(domainEvent);
        }
    }
}