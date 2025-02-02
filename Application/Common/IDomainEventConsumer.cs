using Domain.Entities;

namespace Application.Common
{
    public interface IDomainEventConsumer
    {
        void Consume(IReadOnlyList<IDomainEvent> changes);
    }
}
