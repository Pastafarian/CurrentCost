using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using CurrentCost.Messages.Messages;
using CurrentCost.Monitor.HostedServices;
using CurrentCost.Monitor.Infrastructure;
using CurrentCost.Monitor.Infrastructure.IO.Ports;
using k8s.KubeConfigModels;

namespace CurrentCost.Monitor.HostedServices;

public abstract class MessageStrategy
{
    public abstract bool IsMatch(string message);
    public abstract BaseMessage GetMessage();
}

public interface IMonitorMessageDeserializer
{
    MonitorMessage.Msg? Deserialize(string messageXml);
}

public partial class StandardMessageStrategy : MessageStrategy
{
    private readonly ILogger<StandardMessageStrategy> _logger;
    private readonly IMonitorMessageDeserializer _monitorMessageDeserializer;

    public StandardMessageStrategy(ILogger<StandardMessageStrategy> logger, IMonitorMessageDeserializer monitorMessageDeserializer)
    {
        _logger = logger;
        _monitorMessageDeserializer = monitorMessageDeserializer;
    }

    private readonly Regex _regex = StandardMessageRegex();
    private string Message { get; set; } = null!;
    public override bool IsMatch(string message)
    {
        Message = message;
        return _regex.IsMatch(message);
    }

    public override BaseMessage GetMessage()
    {
        MonitorMessage.Msg? message;
        try
        {
            message = _monitorMessageDeserializer.Deserialize(Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to deserialize message: {message} into {type}", Message, typeof(MonitorMessage));
            return new MonitorMessage.UnknownMessage();
        }

        if (message == null)
        {
            _logger.LogError("Failed to deserialize message: {message} into {type}", Message, typeof(MonitorMessage));
            return new MonitorMessage.UnknownMessage();
        }

        return message;
    }

    [GeneratedRegex("<msg><src>CC128-v0\\.11<\\/src><dsb>\\d+<\\/dsb><time>([0-1]?[0-9]|2[0-3]):[0-5]?[0-9]:[0-5]?[0-9]<\\/time><tmpr>\\d+.\\d+<\\/tmpr><sensor>0<\\/sensor><id>\\d+<\\/id><type>1<\\/type><ch1><watts>\\d+<\\/watts><\\/ch1><ch2><watts>\\d+<\\/watts><\\/ch2><ch3><watts>\\d+<\\/watts><\\/ch3><\\/msg>", RegexOptions.Compiled)]
    private static partial Regex StandardMessageRegex();
}

public class UnknownMessageStrategy : MessageStrategy
{
    public override bool IsMatch(string message) => false;

    public override BaseMessage GetMessage() => new MonitorMessage.UnknownMessage();
  
}

public interface IMessageStrategyService
{
    MessageStrategy GetStrategy(string message);
}

public class MessageStrategyService : IMessageStrategyService
{
    private readonly IEnumerable<MessageStrategy> _messageStrategies;

    public MessageStrategyService(IEnumerable<MessageStrategy> messageStrategies) => _messageStrategies = messageStrategies;

    public MessageStrategy GetStrategy(string message) => _messageStrategies.FirstOrDefault(x => x.IsMatch(message)) ?? new UnknownMessageStrategy();
}


public class DataIngestServiceProcessor : IDataIngestServiceProcessor
{
    private readonly ISimpleSerialPort _serialPort;
    private readonly CurrentCostMonitorMetrics _currentCostMonitorMetrics;
    private readonly IMessageStrategyService _messageStrategyService;
    private readonly IMessageSender _messageSender;
    private CancellationToken _cancellationToken;
    public DataIngestServiceProcessor(ISimpleSerialPort serialPort, CurrentCostMonitorMetrics currentCostMonitorMetrics,
        IMessageStrategyService messageStrategyService, IMessageSender messageSender)
    {
        _serialPort = serialPort;
        _currentCostMonitorMetrics = currentCostMonitorMetrics;
        _messageStrategyService = messageStrategyService;
        _messageSender = messageSender;
    }

    public Task Process(CancellationToken stoppingToken)
    {
        Thread.Sleep(1000);
        _cancellationToken = stoppingToken;
        _serialPort.Open();
        _serialPort.DataReceived += SerialPortOnDataReceived;

        while (!stoppingToken.IsCancellationRequested)
        {
          _serialPort.DataReceived += SerialPortOnDataReceived;
        }

        _serialPort.Close();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serialPort.Dispose();
        return Task.CompletedTask;
    }

    private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
            try
            {
                if (sender is ISimpleSerialPort p)
                {
                    var messageString = p.ReadLine();

                    _currentCostMonitorMetrics.MessagesReceive();
                    _currentCostMonitorMetrics.RecordTotalWattage(44566);
                    var messageStrategy = _messageStrategyService.GetStrategy(messageString);
                    var message = messageStrategy.GetMessage();
                    if (message.ShouldBeSent)
                    {
                        Task.Run(async () => await _messageSender.SendMessage(message, _cancellationToken),
                            _cancellationToken);
                    }
                }
            }
            catch (TimeoutException) { }
    }
}


public class DataIngestService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private IDataIngestServiceProcessor? _dataIngestServiceProcessor;

    public DataIngestService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            _dataIngestServiceProcessor = scope.ServiceProvider.GetRequiredService<IDataIngestServiceProcessor>();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                     _dataIngestServiceProcessor.Process(stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        return Task.CompletedTask;
    }

    public async override Task StopAsync(CancellationToken cancellationToken)
    {
        await (_dataIngestServiceProcessor?.StopAsync(cancellationToken) ?? Task.CompletedTask);
        await base.StopAsync(cancellationToken);
    }
};
