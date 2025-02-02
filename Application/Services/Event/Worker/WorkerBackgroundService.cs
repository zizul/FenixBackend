using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.Event.Worker
{
    internal class WorkerBackgroundService : BackgroundService
    {
        private readonly ILogger logger;

        private readonly IWorkerManager manager;


        public WorkerBackgroundService(
            IWorkerManager manager, ILoggerFactory loggerFactory)
        {
            this.manager = manager;
            logger = loggerFactory.CreateLogger<WorkerBackgroundService>();
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await manager.DequeueAndRunJob(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    throw;
                }
            }
            
            logger.LogDebug("Queued Hosted Service is stopping.");
        }
    }
}
