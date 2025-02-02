using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger logger;


        public LoggingBehavior(ILogger<TRequest> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = request.GetType().Name;
            var requestGuid = Guid.NewGuid().ToString();

            var requestNameWithGuid = $"{requestName} [{requestGuid}]";

            logger.LogDebug($"[START] {requestNameWithGuid}");
            TResponse response;

            try
            {
                response = await next();
            }
            finally
            {
                logger.LogDebug(
                    $"[END] {requestNameWithGuid}");
            }

            return response;
        }
    }
}
