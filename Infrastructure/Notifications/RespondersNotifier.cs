using Application.Services.Event.Contracts;
using Domain.Entities.Event;
using FirebaseAdmin.Messaging;
using Newtonsoft.Json;

namespace Infrastructure.Notifications
{
    internal class RespondersNotifier : IRespondersNotifier
    {
        private readonly IAppNotifier notifier;


        public RespondersNotifier(IAppNotifier notifier)
        {
            this.notifier = notifier;
        }

        public async Task Notify(string[] firebaseTokens, ReportedEvent reportedEvent)
        {
            var message = CreateMessage(firebaseTokens, reportedEvent);
            await notifier.SendMulticast(message);
        }

        private MulticastMessage CreateMessage(string[] tokens, ReportedEvent reportedEvent)
        {
            return new MulticastMessage()
            {
                Tokens = tokens,
                Notification = new Notification()
                {
                    Title = "Nowe zdarzenie",
                    Body = "nowe zdarzenie",
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "event" },
                    { "eventId", reportedEvent.Id },
                    { "status", reportedEvent.Status.ToString() },
                    { "location", JsonConvert.SerializeObject(reportedEvent.Coordinates) },
                    { "msg", "Nowe zdarzenie" }
                },
                Android = new AndroidConfig()
                {
                    Priority = Priority.High
                }
            };
        }
    }
}
