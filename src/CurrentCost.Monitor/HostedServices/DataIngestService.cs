using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using CurrentCost.Monitor.Infrastructure.IO.Ports;

namespace CurrentCost.Monitor.HostedServices;

public class DataIngestService : BackgroundService
{
    private bool _continue;
    private readonly ISimpleSerialPort _serialPort = null!;
    private Thread _readThread = null!;

    private readonly ActivitySource _activitySource;
   
    public DataIngestService(ISimpleSerialPort serialPort)
    {
        _serialPort = serialPort;
        _activitySource = new ActivitySource(nameof(DataIngestService.ExecuteAsync));
        var activityListener = new ActivityListener
        {
            ShouldListenTo = s => true,
            SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(activityListener);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _readThread = new Thread(() =>
        {
            while (_continue)
            {
                try
                {
                    using var activity = _activitySource.StartActivity(nameof(DataIngestService.ExecuteAsync));
                    var message1 = _serialPort.ReadLine();
                    activity?.SetEndTime(DateTime.UtcNow);
                    //_serialPort.WriteLine(Environment.NewLine + Environment.NewLine + string.Format("<{0}>: {1}", _serialPort.PortName, message));
                }
                catch (TimeoutException) { }
            }
        });

        _serialPort.Open();

        _continue = true;

        _readThread.Start();
        _readThread.Join();

        _serialPort.Close();
        
        return Task.CompletedTask;
    }

    public void Stop() => _continue = false;

    public override void Dispose()
    {
        _serialPort.Dispose();
        base.Dispose();
    }

    internal static string Name()
    {
        var rootServiceName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty;
        return $"{rootServiceName}.{nameof(DataIngestService)}";
    }
}
