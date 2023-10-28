using System.Diagnostics;
using CurrentCost.Infrastructure;
using CurrentCost.Monitor.HostedServices;
using CurrentCost.Monitor.Infrastructure;
using CurrentCost.Monitor.Infrastructure.Deserialization;
using CurrentCost.Monitor.Infrastructure.IO.Ports;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;

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

var seqSettings = builder.Configuration.GetSection(nameof(SeqSettings)).Get<SeqSettings>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq($"http://{(seqSettings.InDocker ? seqSettings.DockerHost : seqSettings.Host)}:{seqSettings.Port}")
    .Enrich.WithAssemblyName()
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger);

Serilog.Debugging.SelfLog.Enable(msg =>
{
    Debug.WriteLine(msg);
});

Log.Logger.Information("Starting CurrentCost.Monitor");

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
builder.Services.AddTransient<IMessageStrategyFactory, MessageStrategyFactory>();
builder.Services.AddTransient<MessageStrategy, StandardMessageStrategy>();
builder.Services.AddTransient<MessageStrategy, UnknownMessageStrategy>();
builder.Services.AddTransient<IMessageSender, MessageSender>();
builder.Services.AddTransient<IMonitorMessageDeserializer, MonitorMessageDeserializer>();
builder.Services.AddTransient<IDataIngestServiceProcessor, DataIngestServiceProcessor>();
builder.Services.AddHostedService<DataIngestService>();

builder.Logging.Services.AddSingleton<CurrentCostMonitorMetrics>();

var settings = builder.Configuration.GetSection(nameof(SeqSettings)).Get<SeqSettings>();
builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(hostContext.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithAssemblyName()
        .Enrich.WithProperty("Application", "CurrentCost.Monitor")
        .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Version", typeof(CurrentCost.Monitor.Program).Assembly.GetName().Version)
        .WriteTo.Seq($"http://{(settings.InDocker ? settings.DockerHost : settings.Host )}:{settings.Port}")
        .WriteTo.Console();
});


var app = builder.Build();
;
app.UseMiddleware<ExceptionHandlerMiddleware>();
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

namespace CurrentCost.Monitor
{
    public partial class Program
    {
    }
}
