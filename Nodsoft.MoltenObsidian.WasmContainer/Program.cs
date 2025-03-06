using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.WasmContainer.Components;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// builder.RootComponents.Add<RemoteVaultDisplay>(".moltenobsidian-content");
builder.RootComponents.RegisterForJavaScript<RemoteVaultDisplay>(
    identifier: "moltenobsidian-display-remote"
);

// builder.RootComponents.Add<RemoteVaultNavigation>(".moltenobsidian-nav");
builder.RootComponents.RegisterForJavaScript<RemoteVaultNavigation>(
    identifier: "moltenobsidian-nav-remote"
);

builder.Services.AddMoltenObsidianBlazorIntegration();
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();