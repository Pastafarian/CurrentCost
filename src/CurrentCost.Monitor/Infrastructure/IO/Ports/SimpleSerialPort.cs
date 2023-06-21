using System.IO.Ports;

namespace CurrentCost.Monitor.Infrastructure.IO.Ports;

sealed internal class SimpleSerialPort : SerialPort, ISimpleSerialPort
{
    public SimpleSerialPort()
    {
        BaudRate = 57600;
        Parity = Parity.None;
        DataBits = 8;
        StopBits = StopBits.One;
        Handshake = Handshake.None;
        ReadTimeout = 500;
        WriteTimeout = 500;
        DtrEnable = true;
    }
}
