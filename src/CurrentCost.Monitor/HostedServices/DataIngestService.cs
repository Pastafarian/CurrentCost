using System.Diagnostics;
using System.Reflection;
using CurrentCost.Monitor.Infrastructure.IO.Ports;

namespace CurrentCost.Monitor.HostedServices;

public class DataIngestService : BackgroundService
{
    private bool _continue;
    private ISimpleSerialPort _serialPort = null!;
    private Thread _readThread = null!;

    private readonly ActivitySource _activitySource = new ActivitySource(Name());

    public DataIngestService(ISimpleSerialPort serialPort)
        => _serialPort = serialPort;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity(nameof(DataIngestService.ExecuteAsync));
        activity?.SetTag("testTag", "testValue");
        return Task.CompletedTask;

        // TODO: DataIngestService.ExecuteAsync
        //var message = string.Empty;
        //var stringComparer = StringComparer.OrdinalIgnoreCase;
        //_readThread = new Thread(() =>
        //{
        //    while (_continue)
        //    {
        //        try
        //        {
        //            string message1 = _serialPort.ReadLine();
        //            //_serialPort.WriteLine(Environment.NewLine + Environment.NewLine + string.Format("<{0}>: {1}", _serialPort.PortName, message));
        //        }
        //        catch (TimeoutException) { }
        //    }
        //});

        //// Create a new SerialPort object with default settings.
        //_serialPort = new SimpleSerialPort()
        //{
        //    BaudRate = 57600,
        //    Parity = Parity.None,
        //    DataBits = 8,
        //    StopBits = StopBits.One,
        //    Handshake = Handshake.None,
        //    ReadTimeout = 500,
        //    WriteTimeout = 500
        //};
        //_serialPort.Open();

        //_continue = true;

        //_readThread.Start();
        //_readThread.Join();

        //_serialPort.Close();
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
