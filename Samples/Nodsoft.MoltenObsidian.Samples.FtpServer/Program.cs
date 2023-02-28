using System.Net;
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Samples.FtpServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMoltenObsidianBlazorIntegration();
var creds = new NetworkCredential("clear", "clear01");
// var ips = NetworkInterface.GetAllNetworkInterfaces()
//     .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
//     .Where(x => x.OperationalStatus == OperationalStatus.Up)
//     .SelectMany(x => x.GetIPProperties().UnicastAddresses)
//     .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
//     .Select(x => x.Address)
//     .ToList();

builder.Services.AddMoltenObsidianFtpVault(_ => new(Path.Combine("localhost", "FtpTest"), creds));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();