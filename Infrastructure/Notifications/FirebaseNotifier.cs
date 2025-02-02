using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.Notifications
{
    internal class FirebaseNotifier : IAppNotifier
    {
        private readonly FirebaseMessaging messaging;
        private readonly ILogger<FirebaseNotifier> logger;


        public FirebaseNotifier(IOptions<FirebaseAdminOptions> options, ILogger<FirebaseNotifier> logger)
        {
            InitFirebaseApp(options.Value);
            this.messaging = FirebaseMessaging.DefaultInstance;
            this.logger = logger;
        }

        private void InitFirebaseApp(FirebaseAdminOptions options)
        {
            var optionsJson = JsonConvert.SerializeObject(options);
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(optionsJson),
                    ProjectId = options.ProjectId
                });
            }
        }

        public async Task Send(Message message)
        {
            var response = await messaging.SendAsync(message);

            logger.LogInformation($"Notification was sent to {message.Token}, Response: {response}");
        }

        public async Task SendMulticast(MulticastMessage message)
        {
            var response = await messaging.SendMulticastAsync(message);

            var successfulTokens = GetSuccessfulTokens(response, message);
            var failedResponses = GetFailedResponses(response, message);

            LogSuccessfulDeliveries(successfulTokens, response.SuccessCount, response.FailureCount);
            LogFailedDeliveries(failedResponses);
        }

        private List<string> GetSuccessfulTokens(BatchResponse response, MulticastMessage message)
        {
            var successfulTokens = new List<string>();

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (response.Responses[i].IsSuccess)
                {
                    successfulTokens.Add(message.Tokens[i]);
                }
            }

            return successfulTokens;
        }

        private List<(string Token, FirebaseMessagingException Exception)> GetFailedResponses(BatchResponse response, MulticastMessage message)
        {
            var failedResponses = new List<(string Token, FirebaseMessagingException Exception)>();

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    failedResponses.Add((message.Tokens[i], response.Responses[i].Exception));
                }
            }

            return failedResponses;
        }

        private void LogSuccessfulDeliveries(List<string> successfulTokens, int successCount, int failureCount)
        {
            logger.LogInformation($"Notification was sent to {string.Join(",", successfulTokens)}, Successfully delivered: {successCount}, Failures: {failureCount}");
        }

        private void LogFailedDeliveries(List<(string Token, FirebaseMessagingException Exception)> failedResponses)
        {
            foreach (var failedResponse in failedResponses)
            {
                var exception = failedResponse.Exception;
                logger.LogWarning($"Failed to send notification to {failedResponse.Token}: {exception.Message}, " +
                    $"MessagingErrorCode: {exception.MessagingErrorCode}, " +
                    $"ErrorCode: {exception.ErrorCode}, " +
                    $"HttpResponse: {exception.HttpResponse}");
            }

        }
    }
}
