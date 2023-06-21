using CurrentCost.Monitor.HostedServices;
using CurrentCost.Monitor.Infrastructure.IO.Ports;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var resouces = ResourceBuilder.CreateDefault().AddService(Assembly.GetAssembly(typeof(Program))?.FullName ?? "CurrentCost.Monitor");
builder.Logging.AddOpenTelemetry(x =>
{
    x.SetResourceBuilder(resouces);
    x.IncludeFormattedMessage = true;
    x.AddConsoleExporter(options =>
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        options.Targets = ConsoleExporterOutputTargets.Debug;
    });
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddSource(DataIngestService.Name()).SetResourceBuilder(resouces);
        tracerProviderBuilder.AddAspNetCoreInstrumentation();
        tracerProviderBuilder.AddConsoleExporter(options =>
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            options.Targets = ConsoleExporterOutputTargets.Debug;
        });
    }).WithMetrics(opts =>
    {
        opts.SetResourceBuilder(resouces);
        opts.AddAspNetCoreInstrumentation();
        opts.AddRuntimeInstrumentation();
        opts.AddProcessInstrumentation();
        opts.AddOtlpExporter(x =>
        {
            var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
            x.Endpoint = new Uri(otlpEndpoint);
        });   
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

public partial class Program
{
}
