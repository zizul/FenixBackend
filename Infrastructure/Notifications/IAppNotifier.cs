using FirebaseAdmin.Messaging;

namespace Infrastructure.Notifications
{
    internal interface IAppNotifier
    {
        Task Send(Message message);
        Task SendMulticast(MulticastMessage message);
    }
}
