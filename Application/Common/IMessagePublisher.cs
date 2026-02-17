namespace Application.Common
{
    /// <summary>
    /// Abstraction over the message bus (RabbitMQ via MassTransit).
    /// Decouples the Application layer from specific messaging infrastructure,
    /// keeping the dependency graph clean (Application → Domain only).
    /// </summary>
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
