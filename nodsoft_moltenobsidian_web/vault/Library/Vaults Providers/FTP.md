## FTP / FTPS Vault Provider 
**NuGet Package : [`Nodsoft.MoltenObsidian.Vaults.Ftp`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.Ftp)**

### Description
This provider supports serving a MoltenObsidian vault hosted on a remote FTP server. By targeting a Vault manifest file (generated by the [CLI Tool](/Nodsoft.MoltenObsidian.Tool)), you can serve a Molten Obsidian over the wire, which is considered out of bounds of the reference Obsidian implementation.


### Example Usage
Declare an FTP vault in Dependency Injection:
```csharp
using Microsoft.Extensions.DependencyInjection; 

public void ConfigureServices(IServiceCollection services) 
{
	// Add an FTP Client pointing to your remote host, along with credentials if needed,
	// Then add an FTP vault that uses that client.
	services.AddSingleton(new AsyncFtpClient("ftp.example.com", "user", "password", 21));
	services.AddMoltenObsidianFtpVault(s => s.GetRequiredService<AsyncFtpClient>());
}
```

Alternatively you can instantiate your own FTP vault like so:
```csharp
using Nodsoft.MoltenObsidian.Vaults.Ftp;

// Instantiate the FtpClient.
// Please note that the client's lifetime must follow that of the Vault itself, 
// as it will be reused for retrieving the vault's contents on-demand.
AsyncFtpClient ftpClient = new("ftp.example.com", "user", "password", 21);

// Get the vault manifest from the server.
byte[] bytes = await ftpClient.DownloadBytes("moltenobsidian.manifest.json", CancellationToken.None)   
?? throw new InvalidOperationException("Could not download manifest.");

// Instantiate the vault.
IVault vault = FtpRemoteVault.FromManifest(manifest, ftpClient);
```

Please note that the example path used in the above examples reflect the FTP path preceding the Manifest's `moltenobsidian.manifest.json`. This means the actual manifest link would be `ftp://user:password@path.to/vault/moltenobsidian.manifest.json`.

### Known Limitations (Potential future features?)
- **The FTP provider is readonly.**
- **No tree refresh capabilities have been implemented yet.** Once instantiated, the Vault file structure is immutable. This is by constraint, as we'd need to design a refresh mechanism on the vault's manifest itself ; The implications of which are debatable.
- **No caching support on the provider itself.** This is both by design and by constraint, as we intend to keep the reference Vault implementations as unopinionated as possible, relying on the most minimal set of dependencies (exception noted for [MS-DI/MEDI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection), which is taken for granted as a standard for DI).
- **No checksum comparison implementation**. While the Manifest holds the checksum of each file, there is currently no use for these as of yet. If you don't perform any remote host validation and/or don't require transport security, **this can cause a security issue, as the provider may in worst cases serve tampered files over the wire**.

***If any of those features are considered a necessity in your use case, feel free to voice your need by [raising an issue](https://github.com/Nodsoft/MoltenObsidian/issues).***
