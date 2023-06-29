using System.IO.Ports;
using System.Reflection;

namespace CurrentCost.Monitor.Infrastructure.IO.Ports;

sealed internal class SimpleSerialPortEmulator : ISimpleSerialPort
{
    private readonly ILogger<SimpleSerialPortEmulator> _logger;

    public event SerialDataReceivedEventHandler? DataReceived ;

    public SimpleSerialPortEmulator(ILogger<SimpleSerialPortEmulator> logger)
    {
        _logger = logger;
    }

    void timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        var constructor = typeof(SerialDataReceivedEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerialData) }, null);
        var eventArgs = (SerialDataReceivedEventArgs)constructor.Invoke(new object[] { SerialData.Eof });



        _logger.LogInformation("timer elapsed in Serial Port Emulator");
        DataReceived?.Invoke(this, eventArgs);
        Console.Write("FOO");
    }
    public SimpleSerialPortEmulator()
    {
        var timer = new System.Timers.Timer(1000 * 5);
        timer.Elapsed += timer_Elapsed;
        timer.Enabled = true;
        Console.WriteLine("Timer has started");
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
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return $"<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>{now.Hour}:{now.Minute}:{now.Second}</time><tmpr>21.5</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>{01445 + now.Second}</watts></ch1><ch2><watts>{01957 + now.Second + 10}</watts></ch2><ch3><watts>{04110 - now.Second - 10}</watts></ch3></msg>";
    }
}
