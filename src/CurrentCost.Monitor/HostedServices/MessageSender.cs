using CurrentCost.Messages.Messages;
using MassTransit;

namespace CurrentCost.Monitor.HostedServices
{
    public class MessageSender : IMessageSender
    {
        private readonly IBus _bus;
        private readonly ILogger<MessageSender> _logger;

        public MessageSender(
            IBus bus,
            ILogger<MessageSender> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task<bool> SendMessage(MonitorMessage message, CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
                _logger.LogInformation("Sending message Message.Time: {MessageTime}", message.Time);
                await _bus.Publish(message, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send message: {message} into {type}", message, typeof(MonitorMessage));
                return false;
            }

            return true;

        }
    }
}
