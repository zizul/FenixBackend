using Domain.Enums;

namespace Domain.Entities.Event.DomainExceptions
{
    public class ForbiddenEventStatusDomainException : Exception
    {
        public ForbiddenEventStatusDomainException(string message)
            : base(message)
        {
        }

        public ForbiddenEventStatusDomainException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static ForbiddenEventStatusDomainException WithStatus(EventStatusType status)
        {
            return new ForbiddenEventStatusDomainException($"Manual event status set for {status} is forbidden");
        }
    }
}
