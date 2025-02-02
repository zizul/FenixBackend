using Domain.Enums;

namespace Domain.Entities.Event.DomainExceptions
{
    public class ResponderInvalidStateDomainException : Exception
    {
        public ResponderInvalidStateDomainException(string message)
            : base(message)
        {
        }

        public ResponderInvalidStateDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static ResponderInvalidStateDomainException WithStates(
            ResponderStatusType from, ResponderStatusType to)
        {
            return new ResponderInvalidStateDomainException(
                $"Responder status change is not possible from state: " +
                $"{from} -> {to}");
        }
    }
}
