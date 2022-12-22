using System.IO.Ports;
using CurrentCost.Monitor.Infrastructure.IO.Ports;

namespace CurrentCost.Monitor.HostedServices;

public class DataIngestService : BackgroundService
{
    private bool _continue;
    private ISimpleSerialPort _serialPort = null!;
    private Thread _readThread = null!;

    public DataIngestService(ISimpleSerialPort serialPort)
        => _serialPort = serialPort;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();

        var message = string.Empty;
        var stringComparer = StringComparer.OrdinalIgnoreCase;
        _readThread = new Thread(() =>
        {
            while (_continue)
            {
                try
                {
                    string message1 = _serialPort.ReadLine();
                    //_serialPort.WriteLine(Environment.NewLine + Environment.NewLine + string.Format("<{0}>: {1}", _serialPort.PortName, message));
                }
                catch (TimeoutException) { }
            }
        });

        // Create a new SerialPort object with default settings.
        _serialPort = new SimpleSerialPort()
        {
            BaudRate = 57600,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 500
        };
        _serialPort.Open();

        _continue = true;

        _readThread.Start();
        _readThread.Join();

        _serialPort.Close();
    }

    public void Stop() => _continue = false;

    public override void Dispose()
    {
        _serialPort.Dispose();
        base.Dispose();
    }
}
