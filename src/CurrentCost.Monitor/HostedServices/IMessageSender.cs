using CurrentCost.Messages.Messages;

namespace CurrentCost.Monitor.HostedServices
{
    public interface IMessageSender
    {
        Task<bool> SendMessage(MonitorMessage message, CancellationToken cancellationToken);
    }
}
