# Filesystem Vault Provider 
**NuGet Package : [`Nodsoft.MoltenObsidian.Vaults.FileSystem`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.FileSystem)**

### Description
This provider supports the classic/vanilla way of loading an Obsidian vault, which is through the filesystem. By targeting a directory, you can serve a Molten Obsidian vault from it, independently of that vault being initialised through Obsidian first, or not.

### Example Usage
Declare a Filesystem vault in Dependency Injection: 
```csharp
using Microsoft.Extensions.DependencyInjection; 

public void ConfigureServices(IServiceCollection services) 
{ 
	// Declare a FileSystem vault from path:
	services.AddMoltenObsidianFileSystemVault(new DirectoryInfo("/path/to/vault"));
	
	// Alternatively you can declare from an IServiceProvider delegate, returning a path.
	services.AddMoltenObsidianFileSystemVault(s => s.GetRequiredService<IMyService>().GetVaultDirectory());
}
```

Alternatively you can instantiate your own Filesystem vault like so:
```cs
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

IVault vault = FileSystemVault.FromDirectory("/path/to/vault");
```

### Known Limitations (Potential future features?)
 - **No caching support on the provider itself.** This is both by design and by constraint, as we intend to keep the reference Vault implementations as unopinionated as possible, relying on the most minimal set of dependencies (exception noted for [MS-DI/MEDI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection), which is taken for granted as a standard for DI).

***If any of those features are considered a necessity in your use case, feel free to voice your need by [raising an issue](https://github.com/Nodsoft/MoltenObsidian/issues).***
