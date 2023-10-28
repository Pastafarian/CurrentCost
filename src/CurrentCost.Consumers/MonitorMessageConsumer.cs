using System.Diagnostics;
using CurrentCost.Consumers.SignalR;
using CurrentCost.Domain;
using CurrentCost.Domain.Entities;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CurrentCost.Consumers
{
    public class MonitorMessageConsumerDefinition :
        ConsumerDefinition<MonitorMessageConsumer>
    {
        public MonitorMessageConsumerDefinition() => ConcurrentMessageLimit = 4;

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<MonitorMessageConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
    public class MonitorMessageConsumer : IConsumer<Messages.Messages.MonitorMessage>
    {
        private readonly ILogger<MonitorMessageConsumer> _logger;
        private readonly Context _context;
        private readonly IHubContext<MessagingHub, IMonitorMessageReceivedCommand> _messagingHub;

        public MonitorMessageConsumer(ILogger<MonitorMessageConsumer> logger, Context context, IHubContext<MessagingHub, IMonitorMessageReceivedCommand> messagingHub)
        {
            _logger = logger;
            _context = context;
            _messagingHub = messagingHub;
        }

        public async Task Consume(ConsumeContext<Messages.Messages.MonitorMessage> context)
        {
            Console.Write("Consumed Message");
            Log.Logger.Information("Monitor message received. Total Watts {TotalWatts}", context.Message.GetTotalWatts());
            _logger.LogInformation("Monitor message received");
            Debug.WriteLine(context.Message.GetTotalWatts());
            var message = new Message
            {
                Ch1Watts = context.Message.Ch1.Watts,
                Ch2Watts = context.Message.Ch2.Watts,
                Ch3Watts = context.Message.Ch3.Watts,
                CreatedTime = context.Message.Time,
                Dsb = context.Message.Dsb,
                Sensor = context.Message.Sensor,
                Src = context.Message.Src,
                Tmpr = context.Message.Tmpr,
                TotalWatts = context.Message.GetTotalWatts(),
                Type = context.Message.Type
            };
            _context.Messages.Add(message);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            var command = new SignalRMonitorMessage
            {
                Id = message.Id,
                Ch1Watts = context.Message.Ch1.Watts,
                Ch2Watts = context.Message.Ch2.Watts,
                Ch3Watts = context.Message.Ch3.Watts,
                CreatedTime = DateTime.SpecifyKind(context.Message.Time, DateTimeKind.Utc),
                Dsb = context.Message.Dsb,
                Sensor = context.Message.Sensor,
                Src = context.Message.Src,
                Tmpr = context.Message.Tmpr,
                TotalWatts = context.Message.GetTotalWatts(),
                Type = context.Message.Type
            };

            await _messagingHub.Clients.All.MessageReceivedAsync(command);
        }
    }
}
