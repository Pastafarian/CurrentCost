using System.IO.Ports;

namespace CurrentCost.Console;

public class PortChat
{
    static bool _continue;
    static SerialPort _serialPort;

    public static void Main()
    {
        //string name;
        string message;
        var stringComparer = StringComparer.OrdinalIgnoreCase;
        var readThread = new Thread(Read);

        // Create a new SerialPort object with default settings.
        _serialPort = new SerialPort
        {
            Site = null,
            BaudRate = 0,
            BreakState = false,
            DataBits = 0,
            DiscardNull = false,
            DtrEnable = false,
            Encoding = null,
            Handshake = Handshake.None,
            NewLine = null,
            Parity = Parity.None,
            ParityReplace = 0,
            PortName = null,
            ReadBufferSize = 0,
            ReadTimeout = 0,
            ReceivedBytesThreshold = 0,
            RtsEnable = false,
            StopBits = StopBits.None,
            WriteBufferSize = 0,
            WriteTimeout = 0
        };

        // Allow the user to set the appropriate properties.
        _serialPort.PortName = "COM6"; //SetPortName(_serialPort.PortName);
        _serialPort.BaudRate = 57600; // SetPortBaudRate(_serialPort.BaudRate);
        _serialPort.Parity = Parity.None; // SetPortParity(_serialPort.Parity);
        _serialPort.DataBits = 8; // SetPortDataBits(_serialPort.DataBits);
        _serialPort.StopBits = StopBits.One; // SetPortStopBits(_serialPort.StopBits);
        _serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);

        // Set the read/write timeouts
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;

        _serialPort.Open();
        _continue = true;
        readThread.Start();

        //Console.Write("Name: ");
        //name = Console.ReadLine();

        System.Console.WriteLine("Type QUIT to exit");

        while (_continue)
        {
            message = System.Console.ReadLine();

            if (stringComparer.Equals("quit", message))
            {
                _continue = false;
            }
            else
            {
                _serialPort.WriteLine(Environment.NewLine + Environment.NewLine +
                                      $"<{_serialPort.PortName}>: {message}");
            }
        }

        readThread.Join();
        _serialPort.Close();
    }

    public static void Read()
    {
        while (_continue)
        {
            try
            {
                var message = _serialPort.ReadLine();
                System.Console.WriteLine(message);
            }
            catch (TimeoutException) { }
        }
    }

    // Display Port values and prompt user to enter a port.
    public static string SetPortName(string defaultPortName)
    {
        string portName;

        System.Console.WriteLine("Available Ports:");
        foreach (var s in SerialPort.GetPortNames())
        {
            System.Console.WriteLine("   {0}", s);
        }

        System.Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
        portName = System.Console.ReadLine();

        if (portName == "" || !portName.ToLower().StartsWith("com"))
        {
            portName = defaultPortName;
        }
        return portName;
    }
    // Display BaudRate values and prompt user to enter a value.
    public static int SetPortBaudRate(int defaultPortBaudRate)
    {
        string baudRate;

        System.Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
        baudRate = System.Console.ReadLine();

        if (baudRate == "")
        {
            baudRate = defaultPortBaudRate.ToString();
        }

        return int.Parse(baudRate);
    }

    // Display PortParity values and prompt user to enter a value.
    public static Parity SetPortParity(Parity defaultPortParity)
    {
        System.Console.WriteLine("Available Parity options:");
        foreach (var s in Enum.GetNames(typeof(Parity)))
        {
            System.Console.WriteLine("   {0}", s);
        }

        System.Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
        var parity = System.Console.ReadLine();

        if (parity == "")
        {
            parity = defaultPortParity.ToString();
        }

        return (Parity)Enum.Parse(typeof(Parity), parity, true);
    }
    // Display DataBits values and prompt user to enter a value.
    public static int SetPortDataBits(int defaultPortDataBits)
    {
        string dataBits;

        System.Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
        dataBits = System.Console.ReadLine();

        if (dataBits == "")
        {
            dataBits = defaultPortDataBits.ToString();
        }

        return int.Parse(dataBits.ToUpperInvariant());
    }

    // Display StopBits values and prompt user to enter a value.
    public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
    {
        string stopBits;

        System.Console.WriteLine("Available StopBits options:");
        foreach (var s in Enum.GetNames(typeof(StopBits)))
        {
            System.Console.WriteLine("   {0}", s);
        }

        System.Console.Write("Enter StopBits value (None is not supported and \n" +
                             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
        stopBits = System.Console.ReadLine();

        if (stopBits == "")
        {
            stopBits = defaultPortStopBits.ToString();
        }

        return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
    }
    public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
    {
        string handshake;

        System.Console.WriteLine("Available Handshake options:");
        foreach (var s in Enum.GetNames(typeof(Handshake)))
        {
            System.Console.WriteLine("   {0}", s);
        }

        System.Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
        handshake = System.Console.ReadLine();

        if (handshake == "")
        {
            handshake = defaultPortHandshake.ToString();
        }

        return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
    }
}
