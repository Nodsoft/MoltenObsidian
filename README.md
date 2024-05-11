<p align="center"><img src="icon.png" alt="logo" width="256"/></p>
<h1 align="center">MoltenObsidian</h1>
<h3 align="center">.NET 6+ Library for <a href="https://obsidian.md">Obsidian</a>-flavoured Markdown parsing, with support for vault mapping and Blazor.</h3>

<div align="center">
	<hr />
	<img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/Nodsoft/MoltenObsidian/build.yml?style=flat&logo=github&label=test%2Fbuild">
	<img alt="NuGet Core Version" src="https://img.shields.io/nuget/v/Nodsoft.MoltenObsidian?style=flat&logo=nuget&label=core">
	<img alt="NuGet Core Preversion" src="https://img.shields.io/nuget/vpre/Nodsoft.MoltenObsidian?style=flat&logo=nuget&label=core%20(pre)">
	<img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/Nodsoft.MoltenObsidian?style=flat&logo=nuget">
 	<hr />
 </div>
 <div>&#8203;</div>

## Premise

Molten Obsidian is a high-performance library designed as an easily integrated and lightweight FOSS alternative to [Obsidian Publish](https://publish.obsidian.md). 
With extensibility and integration-oriented conception, this library makes it perfect for integrating Obsidian-flavoured markdown notes on your Blazor App, but also importing entire vaults as a navigation-ready area, with full routing support.

Furthermore, Molten Obisidian extends past the original [Obsidian specifications](https://help.obsidian.md/), aiming to supercharge your documentation/wiki applications and websites needs, using a customizable data source interface, and supercharged YAML frontmatter capabilities.

### Example

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

Now, let's take it further.

## Customizations

### Vault sources (see: [Vaults](/Vaults))
Molten Obsidian is designed with extensibility at its core, and allows you to implement your own Vault source. Should the [**existing reference Vault providers**](/Vaults) not be suitable for your Vault storage needs, you can provide your own implementation. 

**A few examples of additional stores you can implement:**
 - Database store (xSQL, MongoDB, etc...)
 - Over-the-wire/Network-based (NFS, etc...)
 - VCS-based (Git repo)

If you're finding yourself implementing any of these, feel free to PR! We'll be more than happy to support new vault providers.

### Layouts
Molten Obsidian is meant to tailor itself to your app. As such, you can provide within the Blazor Component a series of `RenderFragment` delegates responsible for organizing the Vault display.

You can provide them in cascade, as such :
```razor
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
```razor
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


## CLI Tool
Our CLI tool aims at cutting down the menial tasks associated with implementing more advanced features of Molten Obsidian, allowing you to better focus on what matters, but also automating any of those integration tasks within you workflow.

### ***See: [Nodsoft.MoltenObsidian.Tool](/Nodsoft.MoltenObsidian.Tool)***
