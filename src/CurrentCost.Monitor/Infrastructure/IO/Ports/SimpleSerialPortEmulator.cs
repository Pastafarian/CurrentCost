using System.IO.Ports;
using System.Reflection;

namespace CurrentCost.Monitor.Infrastructure.IO.Ports;

sealed internal class SimpleSerialPortEmulator : ISimpleSerialPort
{
    private readonly ILogger<SimpleSerialPortEmulator> _logger;
    private readonly System.Timers.Timer _timer;
    public event SerialDataReceivedEventHandler? DataReceived;

    void timer_Elapsed(object? senderx, System.Timers.ElapsedEventArgs ex)
    {
        var constructor = typeof(SerialDataReceivedEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerialData) }, null);
        var eventArgs = (SerialDataReceivedEventArgs)constructor?.Invoke(new object[] { SerialData.Eof })!;
        _logger.LogInformation("timer elapsed in Serial Port Emulator {SignalTime}", ex.SignalTime);
        DataReceived?.Invoke(this, eventArgs);
    }
    public SimpleSerialPortEmulator(ILogger<SimpleSerialPortEmulator> logger)
    {
        _logger = logger;
        _timer = new System.Timers.Timer(1000 * 5);
        _timer.Elapsed += timer_Elapsed;
        _timer.Enabled = true;
        _logger.LogInformation("Serial Port Emulator created");
    }

    public void Close()
    {

    }

    public void Dispose()
    {
    }

    public void Open()
    {
    }

    public string ReadLine()
    {
        var dateTime = DateTime.UtcNow;
        var now = TimeOnly.FromDateTime(dateTime);
        return $"<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>{now.Hour}:{now.Minute}:{now.Second}</time><tmpr>21.5</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>{new Random().Next(01445, 01499)}</watts></ch1><ch2><watts>{new Random().Next(01900, 02900)}</watts></ch2><ch3><watts>{new Random().Next(04000, 04810)}</watts></ch3></msg>";
    }
}
