namespace Domain.Entities.Event.DomainExceptions
{
    public class EventIsClosedDomainException : Exception
    {
        public EventIsClosedDomainException(string message)
            : base(message)
        {
        }

        public EventIsClosedDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static EventIsClosedDomainException WithId(string eventId)
        {
            return new EventIsClosedDomainException($"Event with id {eventId} is already closed");
        }
    }
}
