using CurrentCost.Monitor.Infrastructure.IO.Ports;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPortEmulator>();
#else
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPort>();
#endif
builder.Services.AddHealthChecks();

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    setup.AddHealthCheckEndpoint("Current Cost UI", "http://localhost/healthz");
    setup.AddHealthCheckEndpoint("Current Cost Monitor Service ", "/healthz");

    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for healthchecks updates every 5 seconds
}).AddInMemoryStorage();

var app = builder.Build();
app.UseRouting();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.UseEndpoints(config => config.MapHealthChecksUI()); // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
app.Run();
