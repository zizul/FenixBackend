using FirebaseAdmin.Messaging;
using Infrastructure.Notifications;

namespace IntegrationTests.Common.Stubs
{
    internal class AppNotifierStub : IAppNotifier
    {
        internal readonly List<Message> SentMessages = new List<Message>();
        internal readonly List<MulticastMessage> SentMulticastMessages = new List<MulticastMessage>();


        public Task Send(Message message)
        {
            SentMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task SendMulticast(MulticastMessage message)
        {
            SentMulticastMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}