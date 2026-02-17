namespace Application.Services.Event.Messages
{
    /// <summary>
    /// Published to RabbitMQ when a new event is reported.
    /// The consumer searches for available responders and re-publishes the message
    /// until the event is resolved or the maximum number of attempts is reached.
    /// Sealed record for immutability; 'with' expressions create copies for re-publishing.
    /// </summary>
    public sealed record SearchRespondersCommand
    {
        public required string EventId { get; init; }
        public double SearchRadiusKm { get; init; } = 5.0;
        public int SearchDelayMs { get; init; } = 500;
        public int Attempt { get; init; } = 1;
    }
}
