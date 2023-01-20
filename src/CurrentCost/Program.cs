using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CurrentCost.Pages;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorise(options => options.Immediate = true);
builder.Services.AddBootstrap5Providers();
builder.Services.AddFontAwesomeIcons();
builder.Services.AddHealthChecks();

builder.Services.AddTransient<IndexViewModel>();

var app = builder.Build();
app.UseStaticFiles();
app.UseHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true })
   .UseHealthChecks("/healthz", new HealthCheckOptions
   {
       Predicate = _ => true,
       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
   });
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
