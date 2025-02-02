using Domain.Enums;

namespace Domain.Entities.Event.DomainExceptions
{
    public class EventInvalidStateDomainException : Exception
    {
        public EventInvalidStateDomainException(string message)
            : base(message)
        {
        }

        public EventInvalidStateDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static EventInvalidStateDomainException WithStates(
            EventStatusType from, EventStatusType to)
        {
            return new EventInvalidStateDomainException(
                $"Event status change is not possible from state: " +
                $"{from} -> {to}");
        }
    }
}
