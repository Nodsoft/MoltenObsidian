using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.Http;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//builder.Services.AddSingleton<IVault>(services => FileSystemVault.FromDirectory(
//	new(Path.Join(services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "Vault", "SocialGuard"))
//));

builder.Services.AddHttpClient("", client =>
{
	client.BaseAddress = new("http://localhost:7010/");
});

builder.Services.AddSingleton<IVault>(services =>
{
	var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient();
	
	// Get the vault manifest from the server
	RemoteVaultManifest manifest = httpClient.GetFromJsonAsync<RemoteVaultManifest>("moltenobsidian.manifest.json").GetAwaiter().GetResult()
		?? throw new InvalidOperationException("Failed to retrieve the vault manifest from the server.");
	
	return HttpRemoteVault.FromManifest(manifest, httpClient);
});

builder.Services.AddMoltenObsidianBlazorIntegration();

WebApplication app = builder.Build();

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