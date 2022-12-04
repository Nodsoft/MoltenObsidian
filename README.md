# Molten Obsidian

**.NET 6+ Library for Obsidian-flavoured Markdown parsing for Blazor with Vault mapping support.**

Molten Obsidian is a high-performance library designed as an easily integrated and lightweight FOSS alternative to [Obsidian Publish](https://publish.obsidian.md). 
With extensibility and integration-oriented conception, this library makes it perfect for integrating Obsidian-flavoured markdown notes on your Blazor App, but also importing entire vaults as a navigation-ready area, with full routing support.

## Example

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

**Now let's open a vault on the Filesystem, and wire it to a routable Blazor component :**
*`Startup.cs`*
```csharp
using Nodsoft.MoltenObsidian.Blazor;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

// First deal with the DI :
public void ConfigureServices(IServiceCollection services)
{
	services.AddMoltenObsidianBlazorIntegration();

	services.AddSingleton<IVault>(FileSystemVault.FromDirectory(new DirectoryInfo("/path/to/obsidian/vault"));
}
```

*`_Imports.razor`*
```cs
@using Nodsoft.MoltenObsidian.Blazor
@using Nodsoft.MoltenObsidian.Blazor.Helpers;
@using Nodsoft.MoltenObsidian.Vault;
```
*`VaultPage.razor`*
```cs
@page "/vault/{*VaultPath}"  
@inject IVault Vault   
  
<ObsidianVaultDisplay BasePath="@this.GetCallingBaseVaultPath()" CurrentPath="@VaultPath" />  
  
@code {  
   [Parameter]  
   public string VaultPath { get; set; }
}
```

In a matter of minutes, you've just created a web app integration for your own Obsidian Vault, for all to see. Congratulations!


## Customizations

### Vault sources
Molten Obsidian is designed with extensibility at its core, and allows you to implement your own Vault source. Should the currently provided filesystem store not be suitable for your Vault storage needs, you can provide your own implementation. 

**A few examples of stores you can implement:**
 - Database store (xSQL, MongoDB, etc...)
 - Over-the-wire/Network-based (Web server, NFS, etc...)
 - VCS-based (Git repo)

If you're finding yourself implementing any of these, feel free to PR! We'll be more than happy to support new vault providers.

### Layouts
Molten Obsidian is meant to tailor itself to your app. As such, you can provide within the Blazor Component a series of `RenderFragment` delegates responsible for organizing the Vault display.

You can provide them in cascade, as such :
```csharp
<ObsidianVaultDisplay BasePath="@this.GetCallingBaseVaultPath()" CurrentPath="@VaultPath">  
   <FoundFile Context="file">  
      <h1>Vault note: @file.NoteName</h1>  
      <a class="lead text-muted" href="@file.Parent?.Path">Return to @(file.Parent?.Name ?? "Root")</a>  
  
      <hr />  
  
      @(new MarkupString(file.ReadDocument().ToHtml()))  
   </FoundFile>  
  
   <NotFound>  
      <h3 class="text-warning">Sorry, there is nothing here.</h3>  
   </NotFound>  
</ObsidianVaultDisplay>
```

Alternatively, you can provide delegates, like so :
```cs
<ObsidianVaultDisplay BasePath="@this.GetCallingBaseVaultPath()" CurrentPath="@VaultPath"  
   FoundFile="OnFoundFile"  
   NotFound="OnNotFound"  
/>  

@code {  
   public static RenderFragment OnFoundFile(IVaultNote file) => __builder =>  
   {  
      <h1>Vault note: @file.NoteName</h1>  
      <a class="lead text-muted" href="@file.Parent?.Path">Return to @(file.Parent?.Name ?? "Root")</a>  

      <hr />  

      @(new MarkupString(file.ReadDocument().ToHtml()))  
   };  
     
   public static RenderFragment OnNotFound(string _) => static __builder =>  
   {  
      <h3 class="text-warning">Sorry, there's nothing here.</h3>  
   }; 
}
```

