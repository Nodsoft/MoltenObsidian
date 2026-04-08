using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.FileProviders;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.Http;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.Http;

/// <summary>
/// Hosts a static HTTP server that serves a MoltenObsidian remote vault (manifest + files).
/// Shared across all tests in the "HttpVault" collection.
/// </summary>
[UsedImplicitly]
public sealed class HttpVaultServerFixture : IAsyncLifetime
{
    /// <summary>
    /// The directory that the vault is based on.
    /// </summary>
    internal DirectoryInfo RootDirectory { get; }
    
    /// <summary>
    /// The base URI of the hosted vault.
    /// </summary>
    public Uri BaseUri => new($"http://127.0.0.1:{_port}/");
    
    private readonly WebApplication _app;
    private readonly int _port;

    private HttpClient Client
    {
        get => field ?? throw new InvalidOperationException("HttpClient not initialized yet.");
        set;
    }

    /// <summary>
    /// The vault being hosted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the vault is accessed before initialization.</exception>
    public HttpRemoteVault Vault
    {
        get => field ?? throw new InvalidOperationException("Vault not initialized yet.");
        private set;
    } = null!;
    
    /// <summary>
    /// The manifest of the hosted vault.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the manifest is accessed before initialization.</exception>
    public RemoteVaultManifest Manifest
    {
        get => field ?? throw new InvalidOperationException("Manifest not initialized yet.");
        set;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpVaultServerFixture"/> class.
    /// </summary>
    public HttpVaultServerFixture()
    {
        RootDirectory = new("Assets/TestVault");

        // 5. Start Kestrel on free port
        _port = GetFreeTcpPort();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost
            .UseKestrel()
            .UseUrls($"http://127.0.0.1:{_port}");

        _app = builder.Build();

        _app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(RootDirectory.FullName),
            ServeUnknownFileTypes = true
        });
    }

    /// <summary>
    /// Provides an HttpClient pointing to the vault root. Caller is responsible for disposing.
    /// </summary>
    public HttpClient CreateClient() => new() { BaseAddress = BaseUri };

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        Manifest = await BuildManifestAsync(RootDirectory);
        string manifestPath = Path.Combine(RootDirectory.FullName, RemoteVaultManifest.ManifestFileName);
        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(Manifest));
        
        await _app.StartAsync();
        
        Client = CreateClient();
        Vault = HttpRemoteVault.FromManifest(Manifest, Client);
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    /// <summary>
    /// Creates a vault instance talking to the live HTTP server. Handy if a test wants direct IVault.
    /// </summary>
    public async Task<IVault> CreateHttpRemoteVaultAsync(CancellationToken ct = default)
    {
        HttpClient client = CreateClient();
        RemoteVaultManifest m = await client.GetFromJsonAsync<RemoteVaultManifest>(RemoteVaultManifest.ManifestFileName, ct)
            ?? throw new InvalidOperationException("Manifest could not be retrieved.");
        return HttpRemoteVault.FromManifest(m, client);
    }

    private static async ValueTask<RemoteVaultManifest> BuildManifestAsync(DirectoryInfo root)
    {
        FileSystemVault fileVault = FileSystemVault.FromDirectory(root);
        return await VaultManifestGenerator.GenerateManifestAsync(fileVault);
    }

    private static int GetFreeTcpPort()
    {
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}