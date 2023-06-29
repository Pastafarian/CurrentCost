using System.Diagnostics;
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
using Serilog;
using Serilog.Data;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://seq:5341")
    .Enrich.WithAssemblyName()
    .CreateLogger();

Log.Logger.Information("Starting CurrentCost.Monitor");
builder.Logging.AddSerilog();
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
builder.Services.AddSingleton<IMessageStrategyService, MessageStrategyService>();
builder.Services.AddSingleton<MessageStrategy, StandardMessageStrategy>();
builder.Services.AddSingleton<MessageStrategy, UnknownMessageStrategy>();
builder.Services.AddSingleton<IMessageSender, MessageSender>();
builder.Services.AddSingleton<IMonitorMessageDeserializer, MonitorMessageDeserializer>();
builder.Services.AddSingleton<IDataIngestServiceProcessor, DataIngestServiceProcessor>();
builder.Services.AddHostedService<DataIngestService>();

builder.Logging.Services.AddSingleton<CurrentCostMonitorMetrics>();

builder.Host.UseSerilog((hostContext, services, configuration) => {
    configuration
        .ReadFrom.Configuration(hostContext.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithAssemblyName()
        .Enrich.WithProperty("Application", "CurrentCost.Monitor")
        .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version)
        .WriteTo.Seq("http://seq:5341")
        .WriteTo.Console();
});
var app = builder.Build();
;

app.UseRouting();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.UseHealthChecksPrometheusExporter("/metrics");
app.UseEndpoints(config => config.MapHealthChecksUI()); // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks

Log.Logger.Error("BOOM!!!");
Debug.WriteLine("BOOM!!!");

app.Run();

public partial class Program
{
}
