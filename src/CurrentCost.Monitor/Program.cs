using CurrentCost.Monitor.Infrastructure.IO.Ports;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPortEmulator>();
#else
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPort>();
#endif

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
