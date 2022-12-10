using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CurrentCost.Infrastructure.IO.Ports;
using CurrentCost.Pages;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorise(options => options.Immediate = true);
builder.Services.AddBootstrap5Providers();
builder.Services.AddFontAwesomeIcons();

#if DEBUG
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPortEmulator>();
#else
    builder.Services.AddSingleton<ISimpleSerialPort, SimpleSerialPort>();
#endif

builder.Services.AddTransient<IndexViewModel>();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
