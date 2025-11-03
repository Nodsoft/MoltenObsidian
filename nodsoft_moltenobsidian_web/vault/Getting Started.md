---
order: 5
---
# Getting Started

MoltenObsidian can be used with two different methods :
- **Library :** Use the .NET MoltenObsidian library to provide programmatic Obsidian integration within your .NET / ASP.NET Core application *(see below)*.
- **CLI Tool :** Use the CLI tool to integrate externally and/or at build time on any web application *(see [[Tool/Index|Tools]])*.

As all tool-related operations are detailed on [[Tool/Index|the tool's readme]], this page focuses on getting started using the .NET library.

## Installing the library
The MoltenObsidian library can be found on [NuGet](https://www.nuget.org/packages?q=Tags:"moltenobsidian"). Here's a rundown of the main packages you'll be using :
- [`Nodsoft.MoltenObsidian`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian): The backbone of the library. Include this package if none other, as it is implicitly referenced by all other packages.
- [`Nodsoft.MoltenObsidian.Blazor`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Blazor): The Blazor integration. This package provides a turnkey solution for Blazor Server, United, and WASM based applications.
- [[Library/Vaults Providers/Index|A vault provider]]: One of them will be required to import a vault.

Package installation follows the same pattern as other NuGet packages. 
Use your IDE's package manager, or run these commands within the root of your .NET project : 
```sh
# Base packages
dotnet package add "Nodsoft.MoltenObsidian" # For basic Obsidian Markdown conversions
dotnet package add "Nodsoft.MoltenObsidian.Blazor" # For Blazor Integration

# Vault providers
dotnet package add "Nodsoft.MoltenObsidian.Vaults.FileSystem" # For local vaults, imported from the filesystem
dotnet package add "Nodsoft.MoltenObsidian.Vaults.Http" # For remote vaults pulled over HTTP
dotnet package add "Nodsoft.MoltenObsidian.Vaults.Ftp" # For remote vaults located on FTP shares
dotnet package add "Nodsoft.MoltenObsidian.Vaults.InMemory" # For in-memory vaults, ideal for testing and runtime content
```
Not all packages are necessary depending on your needs.

## Converting raw Markdown

**Converting an Obsidian-flavoured Markdown note to HTML** is as simple as this : 
```csharp
using Nodsoft.MoltenObsidian;

// Create a new ObsidianText instance with the content to convert
ObsidianText obsidianMarkdown = new(@"
# Hello, world!   

This is a sample Markdown document.    
And a paragraph with **bold** and *italic* text.
");

// This is the HTML string you can then call in Blazor components as `@htmlText`.
MarkupString htmlText = obsidianMarkdown.ToHtml();
```
But that's just the basics. Under the hood, [Markdig](https://github.com/xoofx/markdig) is what makes it happen. Easy!  

Check out [[Markdown Conversion]] for more on converting raw markdown.

## Setting up a vault

**Now let's open an Obsidian vault on the Filesystem, and wire it to a routable Blazor component :**  

*`Startup.cs`*
```csharp
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

// First deal with the DI, by adding a Filesystem vault and the Blazor integration:
public void ConfigureServices(IServiceCollection services)
{
	services.AddMoltenObsidianFileSystemVault(new DirectoryInfo("/path/to/vault"));
	services.AddMoltenObsidianBlazorIntegration();
}
```
*`_Imports.razor`*
```csharp
@using Nodsoft.MoltenObsidian.Blazor
@using Nodsoft.MoltenObsidian.Blazor.Helpers;
@using Nodsoft.MoltenObsidian.Vault;
```
*`VaultPage.razor`*
```csharp
@page "/vault/{*VaultPath}"  
@inject IVault Vault   
  
<ObsidianVaultDisplay BasePath="@this.GetCallingBaseVaultPath()" CurrentPath="@VaultPath" />  
  
@code {  
   [Parameter]  
   public string VaultPath { get; set; }
}
```

In a matter of minutes, you've just created a web app integration for your own Obsidian Vault, for all to see. Congratulations!
