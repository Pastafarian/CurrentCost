using System.Diagnostics;
using System.Text.Json;
using CurrentCost.Messages.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CurrentCost.Consumers
{
    public class NotificationCreatedConsumer : IConsumer<INotificationCreated>
    {
        public async Task Consume(ConsumeContext<INotificationCreated> context)
        {
            var serializedMessage = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { });

            Console.WriteLine($"NotificationCreated event consumed. Message: {serializedMessage}");
        }
    }

    public class MonitorMessageConsumer : IConsumer<MonitorMessage>
    {
        private readonly ILogger<MonitorMessageConsumer> _logger;

        public MonitorMessageConsumer(ILogger<MonitorMessageConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<MonitorMessage> context)
        {
            _logger.LogInformation("Monitor message received");
            Debug.WriteLine(context.Message.GetTotalWatts());

            return Task.CompletedTask;
        }
    }
}
