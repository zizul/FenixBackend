namespace Domain.Entities.Event.DomainExceptions
{
    public class ResponderNotRelatedToEventDomainException : Exception
    {
        public ResponderNotRelatedToEventDomainException(string message)
            : base(message)
        {
        }

        public ResponderNotRelatedToEventDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public static ResponderNotRelatedToEventDomainException WithId(
            string responderId, string eventId)
        {
            return new ResponderNotRelatedToEventDomainException(
                $"Responder with id {responderId} is not related to the event with id {eventId}");
        }
    }
}
