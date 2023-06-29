using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CurrentCost.HostedServices;
using CurrentCost.Messages.Common;
using CurrentCost.Pages;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CurrentCost.Web")
        .AddAttributes(new List<KeyValuePair<string, object>> { new("Application", "CurrentCost.Web") })
    )
    .AddAspNetCoreInstrumentation()
    .AddProcessInstrumentation()
    .AddRuntimeInstrumentation()
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]
                                   ?? throw new InvalidOperationException());
    }));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorise(options => options.Immediate = true);
builder.Services.AddBootstrap5Providers();
builder.Services.AddFontAwesomeIcons();
builder.Services.AddHealthChecks();

RabbitMqSettings? settings = builder.Configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();

if (settings == null)
{
    throw new NullReferenceException("The Event Bus Settings has not been configured. Please check the settings and update them.");
}

//builder.Services.AddMassTransit(x =>
//{
//    //x.UsingRabbitMq((rabbitContext, rabbitConfig) =>
//    //{
//    //    rabbitConfig.Host(new Uri($"amqp://rabbitmq:5672"), "/", h =>
//    //    {
//    //        h.Username("guest");
//    //        h.Password("guest");
//    //    });

//    //    rabbitConfig.ConfigureEndpoints(rabbitContext);
//    //    rabbitConfig.Durable = true;
//    //});
//});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq("http://seq:5341")
    .Enrich.WithAssemblyName()
    .CreateLogger();

Log.Logger.Information("Starting CurrentCost App");

builder.Logging.AddSerilog();
builder.Services.AddTransient<IndexViewModel>();
builder.Host.UseSerilog();

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseRouting();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
   {
       Predicate = _ => true,
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
   });
app.UseHealthChecksPrometheusExporter("/metrics");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
