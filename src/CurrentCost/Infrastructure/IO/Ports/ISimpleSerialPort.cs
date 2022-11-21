namespace CurrentCost.Infrastructure.IO.Ports
{
    internal interface ISimpleSerialPort : IDisposable
    {
        void Open();
        void Close();
        string ReadLine();
    }
}
