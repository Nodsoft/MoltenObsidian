using System.Net;
using FluentFTP;
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Samples.FtpServer.Data;
using Nodsoft.MoltenObsidian.Vaults.Ftp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMoltenObsidianBlazorIntegration();
builder.Services.AddMoltenObsidianFtpVault(_ => new AsyncFtpClient("localhost", new NetworkCredential("clear", "clear01"), 21));
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

await app.RunAsync();