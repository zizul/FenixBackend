using Domain.Entities;
using MediatR;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Application.Common
{
    public sealed class DomainEventDispatcher : IDomainEventConsumer
    {
        /// <summary>
        /// Cache of compiled factory delegates per domain event type.
        /// Eliminates DLR overhead from the previous (dynamic) cast approach.
        /// Each factory creates a DomainEventNotification{T} for its specific T.
        /// Thread-safe: ConcurrentDictionary handles concurrent GetOrAdd calls.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<IDomainEvent, INotification>> NotificationFactoryCache = new();

        private readonly IMediator mediator;


        public DomainEventDispatcher(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task Consume(IReadOnlyList<IDomainEvent> changes)
        {
            // Index-based loop avoids IEnumerator<T> allocation from foreach on IReadOnlyList<T>
            for (var i = 0; i < changes.Count; i++)
            {
                var domainEvent = changes[i];

                // Resolve compiled factory from cache (near-native speed after first call per event type)
                var factory = NotificationFactoryCache.GetOrAdd(
                    domainEvent.GetType(),
                    static type => CompileNotificationFactory(type));

                var notification = factory(domainEvent);

                // Awaiting ensures handler exceptions propagate and handlers complete
                // before the caller continues (fixes previous fire-and-forget bug)
                await mediator.Publish(notification);
            }
        }

        /// <summary>
        /// Compiles a delegate: (IDomainEvent e) => new DomainEventNotification{T}((T)e)
        /// Uses Expression trees for near-native invocation speed after initial compilation.
        /// Replaces the previous (dynamic) cast which incurred DLR overhead on every call.
        /// </summary>
        private static Func<IDomainEvent, INotification> CompileNotificationFactory(Type domainEventType)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEventType);
            var ctor = notificationType.GetConstructor(new[] { domainEventType })
                ?? throw new InvalidOperationException(
                    $"DomainEventNotification<{domainEventType.Name}> is missing the expected constructor.");

            var param = Expression.Parameter(typeof(IDomainEvent), "domainEvent");
            var castParam = Expression.Convert(param, domainEventType);
            var newExpr = Expression.New(ctor, castParam);
            var castResult = Expression.Convert(newExpr, typeof(INotification));

            return Expression.Lambda<Func<IDomainEvent, INotification>>(castResult, param).Compile();
        }
    }
}
