using System.Text;
using CurrentCost.Messages.Messages;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CurrentCost.Monitor.HostedServices
{
    public interface IMessageSender
    {
        Task<bool> SendMessage(BaseMessage message, CancellationToken cancellationToken);
    }

    public class MessageSender : IMessageSender
    {
        private readonly IPublishEndpoint _publisher;
        public MessageSender(IPublishEndpoint publisher) => _publisher = publisher;

        public async Task<bool> SendMessage(BaseMessage message, CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            try
            {
               await _publisher.Publish(message, cancellationToken);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;

        }
    }
}
