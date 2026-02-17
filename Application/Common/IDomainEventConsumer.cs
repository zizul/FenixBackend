using Domain.Entities;

namespace Application.Common
{
    public interface IDomainEventConsumer
    {
        /// <summary>
        /// Dispatches domain events to their respective handlers via MediatR.
        /// Awaitable to guarantee all handlers complete before control returns to the caller,
        /// preventing fire-and-forget scenarios that silently swallow exceptions.
        /// </summary>
        Task Consume(IReadOnlyList<IDomainEvent> changes);
    }
}
