namespace Domain.Entities.Event.DomainExceptions
{
    public class ResponderAlreadyAssignedDomainException : Exception
    {
        public ResponderAlreadyAssignedDomainException(string message)
            : base(message)
        {
        }

        public ResponderAlreadyAssignedDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static ResponderAlreadyAssignedDomainException WithId(
            string responderId, string eventId)
        {
            return new ResponderAlreadyAssignedDomainException(
                $"Responder with id {responderId} is already assigned to the event with id {eventId}");
        }
    }
}
