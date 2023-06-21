namespace CurrentCost.Monitor.Infrastructure.IO.Ports;

sealed internal class SimpleSerialPortEmulator : ISimpleSerialPort
{
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
        Thread.Sleep(1000);
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return $"<msg><src>CC128-v0.11</src><dsb>00224</dsb><time>{now.Hour}:{now.Minute}:{now.Second}</time><tmpr>21.5</tmpr><sensor>0</sensor><id>02926</id><type>1</type><ch1><watts>{01445 + now.Second}</watts></ch1><ch2><watts>{01957 + now.Second + 10}</watts></ch2><ch3><watts>{04110 - now.Second - 10}</watts></ch3></msg>";
    }
}
