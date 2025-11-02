# In-Memory Vault Provider 
**NuGet Package : [`Nodsoft.MoltenObsidian.Vaults.InMemory`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.InMemory)**

### Description
This provider supports creating and managing an Obsidian vault entirely in memory. Unlike other providers that read from external sources, the In-Memory vault is writable and stores all data in memory, making it ideal for testing, temporary vaults, or runtime-generated content.

The In-Memory vault implements `IWritableVault`, allowing you to create, modify, and delete folders, files, and notes programmatically.

### Example Usage
Declare an In-Memory vault in Dependency Injection: 
```csharp
using Microsoft.Extensions.DependencyInjection; 

public void ConfigureServices(IServiceCollection services) 
{ 
	// Declare an In-Memory vault with a name:
	services.AddMoltenObsidianInMemoryVault("MyVault");
}
```

Alternatively you can instantiate your own In-Memory vault like so:
```csharp
using Nodsoft.MoltenObsidian.Vaults.InMemory;

// Create a new in-memory vault
IVault vault = new InMemoryVault("MyVault");

// Populate the vault with content
await vault.WriteNoteAsync("Notes/Welcome.md", 
	new MemoryStream(Encoding.UTF8.GetBytes("# Welcome\nThis is a note in memory.")));

await vault.WriteFileAsync("Assets/image.png", imageStream);
```

### Setup Mode
The In-Memory vault supports a "setup mode" which can be enabled to prevent vault update events from being raised during bulk operations:

```csharp
// Create vault in setup mode
var vault = new InMemoryVault("MyVault", setup: true);

// Populate the vault without triggering events
await vault.WriteNoteAsync("Note1.md", contentStream1);
await vault.WriteNoteAsync("Note2.md", contentStream2);

// Disable setup mode to start receiving events
vault.Setup = false;
```

### Known Limitations (Potential future features?)
 - **No persistence support.** All data is stored in memory and will be lost when the application terminates. This is by design for the use cases this provider targets (testing, temporary vaults, runtime content).
 - **No caching support on the provider itself.** This is both by design and by constraint, as we intend to keep the reference Vault implementations as unopinionated as possible, relying on the most minimal set of dependencies (exception noted for [MS-DI/MEDI](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection), which is taken for granted as a standard for DI).

***If any of those features are considered a necessity in your use case, feel free to voice your need by [raising an issue](https://github.com/Nodsoft/MoltenObsidian/issues).***
