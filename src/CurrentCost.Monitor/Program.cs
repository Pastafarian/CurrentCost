using CurrentCost.Messages.Common;
using CurrentCost.Monitor.HostedServices;
using CurrentCost.Monitor.Infrastructure;
using CurrentCost.Monitor.Infrastructure.IO.Ports;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using CurrentCost.Monitor.Infrastructure.Deserialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(
    (_, options) =>
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    });
builder.Logging.AddOpenTelemetry(x =>
{
    x.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CurrentCost.Monitor"));
    x.IncludeFormattedMessage = true;
    x.AddConsoleExporter(options =>
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        options.Targets = ConsoleExporterOutputTargets.Debug;
    });

});

CurrentCostMonitorMetrics metrics = new();

builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("CurrentCost.Monitor"))
    .AddMeter(metrics.MetricName)
    .AddAspNetCoreInstrumentation()
    .AddProcessInstrumentation()
    .AddRuntimeInstrumentation()
    .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]
                                   ?? throw new InvalidOperationException())));
//builder.Services.AddOpenTelemetry()
//    .WithTracing(tracerProviderBuilder =>
//    {
//        tracerProviderBuilder.AddSource(DataIngestService.Name()).SetResourceBuilder(resouces);
//        tracerProviderBuilder.AddAspNetCoreInstrumentation();
//        tracerProviderBuilder.AddConsoleExporter(options =>
//        {
//            if (options == null) throw new ArgumentNullException(nameof(options));
//            options.Targets = ConsoleExporterOutputTargets.Debug;
//        });
//    }).WithMetrics(opts =>
//    {
//        opts.SetResourceBuilder(resouces);
//        opts.AddAspNetCoreInstrumentation();
//        opts.AddRuntimeInstrumentation();
//        opts.AddProcessInstrumentation();
//        opts.AddOtlpExporter(x =>
//        {
//            var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
//            x.Endpoint = new Uri(otlpEndpoint);
//        });   
//    }); 

builder.Services.AddHealthChecks();

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    setup.AddHealthCheckEndpoint("Current Cost UI", "http://localhost/healthz");
    setup.AddHealthCheckEndpoint("Current Cost Monitor Service ", "/healthz");

    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for healthchecks updates every 5 seconds
}).AddInMemoryStorage();

var testMode = bool.Parse(builder.Configuration["TestMode"]);
if (testMode)
{
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPortEmulator>();
}
else
{
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPort>();
}

builder.Services.AddEventBusService(builder.Configuration);
builder.Services.AddScoped<IMessageStrategyService, MessageStrategyService>();
builder.Services.AddScoped<MessageStrategy, StandardMessageStrategy>();
builder.Services.AddScoped<MessageStrategy, UnknownMessageStrategy>();
builder.Services.AddScoped<IMessageSender, MessageSender>();
builder.Services.AddScoped<IMonitorMessageDeserializer, MonitorMessageDeserializer>();
builder.Services.AddScoped<IDataIngestServiceProcessor, DataIngestServiceProcessor>();
builder.Services.AddHostedService<DataIngestService>();

builder.Logging.Services.AddSingleton<CurrentCostMonitorMetrics>();

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
