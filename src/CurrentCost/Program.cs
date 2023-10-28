using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CurrentCost;
using CurrentCost.Consumers;
using CurrentCost.Consumers.SignalR;
using CurrentCost.Domain;
using CurrentCost.Infrastructure;
using CurrentCost.Infrastructure.Extensions;
using CurrentCost.Pages;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
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
    .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]
                                   ?? throw new InvalidOperationException())));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorise(options => options.Immediate = true);
builder.Services.AddBootstrap5Providers();
builder.Services.AddFontAwesomeIcons();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

var rabbitMqSettings = builder.Configuration.GetSection(nameof(RabbitMqSettings)).GetSafely<RabbitMqSettings>();
var seqSettings = builder.Configuration.GetSection(nameof(SeqSettings)).GetSafely<SeqSettings>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Seq($"http://{(seqSettings.InDocker ? seqSettings.DockerHost : seqSettings.Host)}:{seqSettings.Port}")
    .Enrich.WithAssemblyName()
    .CreateLogger();

Log.Logger.Information("Starting CurrentCost App");

builder.Logging.AddSerilog(Log.Logger);
builder.Services.AddTransient<IndexViewModel>();
// Add the event bus to the service collection

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<MonitorMessageConsumer, MonitorMessageConsumerDefinition>();
    x.UsingRabbitMq((rabbitContext, rabbitConfig) =>
    {
        var address = rabbitMqSettings.GetAddress();
        rabbitConfig.Host(new Uri(address), "/", h =>
        {
            h.Username(rabbitMqSettings.Username);
            h.Password(rabbitMqSettings.Password);
        });
      //  rabbitConfig.OverrideDefaultBusEndpointQueueName(CurrentCostMessagingConstants.MonitorMessageQueue);
        rabbitConfig.ConfigureEndpoints(rabbitContext);
        rabbitConfig.Durable = true;
    });
});
//builder.Services.AddScoped<MonitorMessageConsumer>();
//builder.Services.AddScoped<NotificationCreatedConsumer>();
var connectionString = builder.Configuration.GetSection(nameof(ConnectionString)).GetSafely<ConnectionString>();
builder.Services.AddSingleton(connectionString);

var migrationsAssemblyName = typeof(CurrentCost.Domain.Migrations.InitialCreate).Assembly.FullName;
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connectionString.GetAddress(), b => b.MigrationsAssembly(migrationsAssemblyName)).EnableSensitiveDataLogging());
builder.Host.UseSerilog((hostContext, services, configuration) => configuration
        .ReadFrom.Configuration(hostContext.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithAssemblyName()
        .Enrich.WithProperty("Application", "CurrentCost")
        .Enrich.WithProperty("Environment", hostContext.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version)
        .WriteTo.Seq($"http://{(seqSettings.InDocker ? seqSettings.DockerHost : seqSettings.Host)}:{seqSettings.Port}")
        .WriteTo.Console());
StaticWebAssetsLoader.UseStaticWebAssets(
    builder.Environment,
    builder.Configuration);
var app = builder.Build();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseWebSockets();

app.UseRouting();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
   {
       Predicate = _ => true,
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
   });
app.UseHealthChecksPrometheusExporter("/metrics");
app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");
var appSettings = builder.Configuration.GetSection("AppSettings").GetSafely<AppSettings>();

if (appSettings.RunMigrations)
{
#pragma warning disable ASP0000
    using var scope = builder.Services.BuildServiceProvider().CreateScope();
#pragma warning restore ASP0000
    var dbContext = scope.ServiceProvider.GetService<Context>();

    dbContext?.Database.Migrate();
}
app.MapHub<MessagingHub>("/signalr-messaging");
app.Run();
