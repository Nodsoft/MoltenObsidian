using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<IVault>(services => FileSystemVault.FromDirectory(
	new(Path.Join(services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "Vault", "SocialGuard"))
));

builder.Services.AddHttpClient("",client =>
{
	client.BaseAddress = new("http://localhost:57333/");
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