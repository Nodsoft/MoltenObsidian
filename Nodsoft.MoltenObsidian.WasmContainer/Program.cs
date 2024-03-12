using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nodsoft.MoltenObsidian.WasmContainer.Components;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<RemoteVaultDisplay>(".moltenobsidian-content");
builder.RootComponents.RegisterForJavaScript<RemoteVaultDisplay>(
    identifier: "moltenobsidian-display-remote",
    javaScriptInitializer: "initializeRemoteVaultDisplay"
);

builder.RootComponents.Add<RemoteVaultNavigation>(".moltenobsidian-nav");
builder.RootComponents.RegisterForJavaScript<RemoteVaultNavigation>(
    identifier: "moltenobsidian-nav-remote",
    javaScriptInitializer: "initializeRemoteVaultNavigation"
);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();