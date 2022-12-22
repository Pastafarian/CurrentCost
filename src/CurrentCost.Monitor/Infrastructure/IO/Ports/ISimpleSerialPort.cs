namespace CurrentCost.Monitor.Infrastructure.IO.Ports;

public interface ISimpleSerialPort : IDisposable
{
    void Open();
    void Close();
    string ReadLine();
}
