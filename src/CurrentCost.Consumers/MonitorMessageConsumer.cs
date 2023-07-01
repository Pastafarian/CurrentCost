using System.Diagnostics;
using CurrentCost.Messages.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CurrentCost.Consumers
{
    public class MonitorMessageConsumerDefinition :
        ConsumerDefinition<MonitorMessageConsumer>
    {
        public MonitorMessageConsumerDefinition()
        {
            // override the default endpoint name, for whatever reason
         //   EndpointName = "monitor-messages";

            // limit the number of messages consumed concurrently
            // this applies to the consumer only, not the endpoint
            ConcurrentMessageLimit = 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<MonitorMessageConsumer> consumerConfigurator)
        {
            //endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
            //endpointConfigurator.UseInMemoryOutbox();
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
            Log.Logger.Information("Monitor message received. Total Watts {TotalWatts}", context.Message.GetTotalWatts());
            _logger.LogInformation("Monitor message received");
            Debug.WriteLine(context.Message.GetTotalWatts());

            return Task.CompletedTask;
        }
    }
}
