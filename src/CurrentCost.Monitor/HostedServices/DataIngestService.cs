using System.IO.Ports;
using CurrentCost.Messages.Messages;
using CurrentCost.Monitor.Infrastructure;
using CurrentCost.Monitor.Infrastructure.IO.Ports;

namespace CurrentCost.Monitor.HostedServices;

public abstract class MessageStrategy
{
    public abstract bool IsMatch(string message);
    public abstract Task ExecuteStrategy(CancellationToken cancellationToken);
}

public interface IMonitorMessageDeserializer
{
    MonitorMessage Deserialize(string messageXml);
}

public class DataIngestServiceProcessor : IDataIngestServiceProcessor
{
    private readonly ISimpleSerialPort _serialPort;
    private readonly IMessageStrategyFactory _messageStrategyFactory;
    private readonly CurrentCostMonitorMetrics _currentCostMonitorMetrics;
    private CancellationToken _cancellationToken;
    public DataIngestServiceProcessor(ISimpleSerialPort serialPort,
        IMessageStrategyFactory messageStrategyFactory,
        CurrentCostMonitorMetrics currentCostMonitorMetrics)
    {
        _serialPort = serialPort;
        _messageStrategyFactory = messageStrategyFactory;
        _currentCostMonitorMetrics = currentCostMonitorMetrics;
    }

    public Task Process(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;
        _serialPort.Open();
        _serialPort.DataReceived += SerialPortOnDataReceived;

        while (!stoppingToken.IsCancellationRequested)
        {
            Task.Yield();
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
             
                    var messageStrategy = _messageStrategyFactory.GetStrategy(messageString);
                    messageStrategy.ExecuteStrategy(_cancellationToken);
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
