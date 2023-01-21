using CurrentCost.Monitor.HostedServices;
using CurrentCost.Monitor.Infrastructure.IO.Ports;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        var serviceVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? string.Empty;
        tracerProviderBuilder.AddSource(DataIngestService.Name()).SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: DataIngestService.Name(), serviceVersion: serviceVersion));

        tracerProviderBuilder.AddAspNetCoreInstrumentation();

        tracerProviderBuilder.AddConsoleExporter();
    });

builder.Services.AddHealthChecks();

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    setup.AddHealthCheckEndpoint("Current Cost UI", "http://localhost/healthz");
    setup.AddHealthCheckEndpoint("Current Cost Monitor Service ", "/healthz");

    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for healthchecks updates every 5 seconds
}).AddInMemoryStorage();

#if DEBUG
builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPortEmulator>();
#else
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPort>();
#endif
builder.Services.AddHostedService<DataIngestService>();

var app = builder.Build();
app.UseRouting();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.UseHealthChecksPrometheusExporter("/metrics");
app.UseEndpoints(config => config.MapHealthChecksUI()); // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
app.Run();
