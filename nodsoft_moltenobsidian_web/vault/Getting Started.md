---
order: 5
---
# Getting Started

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
```razor
@using Nodsoft.MoltenObsidian.Blazor
@using Nodsoft.MoltenObsidian.Blazor.Helpers;
@using Nodsoft.MoltenObsidian.Vault;
```
*`VaultPage.razor`*
```razor
@page "/vault/{*VaultPath}"  
@inject IVault Vault   
  
<ObsidianVaultDisplay BasePath="@this.GetCallingBaseVaultPath()" CurrentPath="@VaultPath" />  
  
@code {  
   [Parameter]  
   public string VaultPath { get; set; }
}
```

In a matter of minutes, you've just created a web app integration for your own Obsidian Vault, for all to see. Congratulations!
