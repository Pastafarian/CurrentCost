using System.Text.RegularExpressions;
using CurrentCost.Messages.Messages;
using CurrentCost.Monitor.Infrastructure;

namespace CurrentCost.Monitor.HostedServices
{
    public partial class StandardMessageStrategy : MessageStrategy
    {
        private readonly ILogger<StandardMessageStrategy> _logger;
        private readonly IMonitorMessageDeserializer _monitorMessageDeserializer;
        private readonly CurrentCostMonitorMetrics _currentCostMonitorMetrics;
        private readonly IMessageSender _messageSender;

        public StandardMessageStrategy(ILogger<StandardMessageStrategy> logger,
            IMonitorMessageDeserializer monitorMessageDeserializer,
            CurrentCostMonitorMetrics currentCostMonitorMetrics,
            IMessageSender messageSender)
        {
            _logger = logger;
            _monitorMessageDeserializer = monitorMessageDeserializer;
            _currentCostMonitorMetrics = currentCostMonitorMetrics;
            _messageSender = messageSender;
        }

        private readonly Regex _regex = StandardMessageRegex();
        private string Message { get; set; } = null!;
        public override bool IsMatch(string message)
        {
            Message = message;
            return _regex.IsMatch(message);
        }

        public override async Task ExecuteStrategy(CancellationToken cancellationToken)
        {
            MonitorMessage? message;
            try
            {
                message = _monitorMessageDeserializer.Deserialize(Message);
            }
            catch (Exception e)
            {
            
                _logger.LogError(e, "Failed to deserialize message: {message} into {type}", Message, typeof(MonitorMessage));
                return;
            }

            _currentCostMonitorMetrics.RecordTotalWattage(message.GetTotalWatts());


            await _messageSender.SendMessage(message, cancellationToken);
        }

        [GeneratedRegex("<msg><src>CC128-v0\\.11<\\/src><dsb>\\d+<\\/dsb><time>([0-1]?[0-9]|2[0-3]):[0-5]?[0-9]:[0-5]?[0-9]<\\/time><tmpr>\\d+.\\d+<\\/tmpr><sensor>0<\\/sensor><id>\\d+<\\/id><type>1<\\/type><ch1><watts>\\d+<\\/watts><\\/ch1><ch2><watts>\\d+<\\/watts><\\/ch2><ch3><watts>\\d+<\\/watts><\\/ch3><\\/msg>", RegexOptions.Compiled)]
        private static partial Regex StandardMessageRegex();
    }
}
