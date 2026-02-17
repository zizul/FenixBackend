using Application.Common;
using MassTransit;

namespace Infrastructure.Messaging
{
    /// <summary>
    /// Delegates message publishing to MassTransit's IPublishEndpoint (RabbitMQ).
    /// Scoped lifetime mirrors IPublishEndpoint's scope to properly flow consume context.
    /// Sealed to prevent subclassing which could bypass the intended publish behavior.
    /// </summary>
    internal sealed class MassTransitMessagePublisher : IMessagePublisher
    {
        private readonly IPublishEndpoint publishEndpoint;


        public MassTransitMessagePublisher(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => publishEndpoint.Publish(message, cancellationToken);
    }
}
