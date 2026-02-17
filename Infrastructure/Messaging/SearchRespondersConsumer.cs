using Application.Services.Event.Contracts;
using Application.Services.Event.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging
{
    /// <summary>
    /// RabbitMQ consumer that handles the responder search loop.
    /// Replaces the previous in-memory WorkerBackgroundService with a durable, fault-tolerant approach:
    ///   - Messages survive application restarts (RabbitMQ persistence)
    ///   - MassTransit retry middleware handles transient failures with exponential backoff
    ///   - Failed messages are moved to an error queue for inspection (no silent loss)
    ///   - ConcurrentMessageLimit prevents database saturation under load
    ///   - Safety limit (MaxAttempts) prevents infinite loops from stale events
    /// </summary>
    internal sealed class SearchRespondersConsumer : IConsumer<SearchRespondersCommand>
    {
        /// <summary>
        /// Safety limit to prevent infinite re-publishing from bugs or stale events.
        /// At 500ms intervals, 3600 attempts ≈ 30 minutes of searching.
        /// </summary>
        private const int MaxAttempts = 3600;

        private readonly IEventCoordinatorService coordinator;
        private readonly ILogger<SearchRespondersConsumer> logger;


        public SearchRespondersConsumer(
            IEventCoordinatorService coordinator,
            ILogger<SearchRespondersConsumer> logger)
        {
            this.coordinator = coordinator;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<SearchRespondersCommand> context)
        {
            var message = context.Message;
            var cancellationToken = context.CancellationToken;

            logger.LogDebug(
                "Processing responder search for event {EventId}, attempt {Attempt}",
                message.EventId, message.Attempt);

            // Coordinator checks event status, finds responders, assigns, and notifies via Firebase
            var shouldContinue = await coordinator
                .TryFindAndAssignRespondersToEvent(message.EventId, message.SearchRadiusKm)
                .ConfigureAwait(false);

            if (!shouldContinue)
            {
                logger.LogInformation(
                    "Responder search completed for event {EventId} after {Attempts} attempts",
                    message.EventId, message.Attempt);
                return;
            }

            // Safety limit prevents infinite loops from bugs or events stuck in Pending state
            if (message.Attempt >= MaxAttempts)
            {
                logger.LogWarning(
                    "Max search attempts ({Max}) reached for event {EventId}. Stopping search.",
                    MaxAttempts, message.EventId);
                return;
            }

            // Delay before re-publishing to avoid saturating the database with rapid queries
            await Task.Delay(message.SearchDelayMs, cancellationToken).ConfigureAwait(false);

            // Re-publish with incremented attempt counter.
            // Immutable record 'with' expression — efficient copy without manual construction.
            await context.Publish(
                message with { Attempt = message.Attempt + 1 },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
