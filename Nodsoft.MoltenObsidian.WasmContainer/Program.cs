using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nodsoft.MoltenObsidian.WasmContainer.Components;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.RegisterForJavaScript<RemoteVaultDisplay>(
    identifier: "moltenobsidian-display-remote",
    javaScriptInitializer: "initializeComponent"
);

builder.RootComponents.RegisterForJavaScript<RemoteVaultNavigation>(
    identifier: "moltenobsidian-nav-remote",
    javaScriptInitializer: "initializeComponent"
);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();