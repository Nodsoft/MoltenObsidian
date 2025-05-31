using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Samples.HttpWasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMoltenObsidianHttpAsyncVault("https://naratteu.github.io/daangn-garden/");
builder.Services.AddMoltenObsidianBlazorIntegration();

await builder.Build().RunAsync();