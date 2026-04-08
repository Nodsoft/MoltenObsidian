using System.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Components;

/// <summary>
/// Provides tests for the <see cref="ObsidianVaultDisplay"/> Razor component.
/// </summary>
/// <seealso cref="ObsidianVaultDisplay"/>
public sealed class ObsidianVaultDisplayTests : IDisposable
{
	private readonly BunitContext _ctx;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianVaultDisplayTests"/> class.
	/// </summary>
	public ObsidianVaultDisplayTests()
	{
		_ctx = new BunitContext();

		// Use loose JSInterop mode so that eval() calls in OnAfterRenderAsync do not throw.
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;

		// Register the required VaultRouterFactory service.
		_ctx.Services.AddSingleton<VaultRouterFactory>();
	}

	/// <inheritdoc/>
	public void Dispose() => _ctx.Dispose();

	/// <summary>
	/// Renders the <see cref="ObsidianVaultDisplay"/> component and returns it.
	/// </summary>
	private IRenderedComponent<ObsidianVaultDisplay> RenderDisplay(
		InMemoryVault vault,
		string currentPath = "/",
		string basePath = "/",
		ObsidianVaultDisplayOptions? options = null)
		=> _ctx.Render<ObsidianVaultDisplay>(p => p
			.Add(c => c.Vault, vault)
			.Add(c => c.CurrentPath, currentPath)
			.Add(c => c.BasePath, basePath)
			.Add(c => c.Options, options ?? new ObsidianVaultDisplayOptions()));

	/// <summary>
	/// Tests that navigating to the vault root (empty path) renders the vault root template
	/// when there is no index note.
	/// </summary>
	[Fact]
	public async Task Render_RootPathNoIndex_RendersVaultRootTemplate()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("some-note.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "/");

		// Assert – the default FoundVaultRoot template renders a <nav> with the vault name
		AngleSharp.Dom.IElement nav = cut.Find("nav");
		Assert.Contains("TestVault", nav.GetAttribute("name") ?? string.Empty);
	}

	/// <summary>
	/// Tests that navigating to the vault root renders the index note template
	/// when an index note (README.md) is present.
	/// </summary>
	[Fact]
	public async Task Render_RootPathWithIndexNote_RendersFoundIndexNoteTemplate()
	{
		// Arrange
		const string content = "# Welcome";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(content));

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "/");

		// Assert – FoundIndexNote template renders an <article> element
		cut.Find("article");
	}

	/// <summary>
	/// Tests that navigating to an existing note renders the found-note template.
	/// </summary>
	[Fact]
	public async Task Render_NotePath_RendersFoundNoteTemplate()
	{
		// Arrange
		const string content = "# My Note";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("my-note.md", Encoding.UTF8.GetBytes(content));

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "my-note");

		// Assert – the default FoundNote template renders an <article> element
		AngleSharp.Dom.IElement article = cut.Find("article");
		Assert.Equal("my-note.md", article.GetAttribute("name"));
	}

	/// <summary>
	/// Tests that navigating to a folder path without an index note renders the found-folder template.
	/// </summary>
	[Fact]
	public async Task Render_FolderPathNoIndex_RendersFoundFolderTemplate()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/page.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "section");

		// Assert – the default FoundFolder template renders a <nav> element with folder name
		AngleSharp.Dom.IElement nav = cut.Find("nav");
		Assert.Equal("section", nav.GetAttribute("name"));
	}

	/// <summary>
	/// Tests that navigating to a folder that has an index note renders the found-index-note template.
	/// </summary>
	[Fact]
	public async Task Render_FolderPathWithIndexNote_RendersFoundIndexNoteTemplate()
	{
		// Arrange
		const string content = "# Section Index";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/index.md", Encoding.UTF8.GetBytes(content));

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "section");

		// Assert – FoundIndexNote template renders an <article class="moltenobsidian-index-note"> element
		cut.Find("article.moltenobsidian-index-note");
	}

	/// <summary>
	/// Tests that navigating to a non-existent path renders the not-found template.
	/// </summary>
	[Fact]
	public void Render_NonExistentPath_RendersNotFoundTemplate()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "does-not-exist");

		// Assert – the default NotFound template renders a <div> with a "No result." heading
		AngleSharp.Dom.IElement heading = cut.Find("h1");
		Assert.Equal("No result.", heading.TextContent);
	}

	/// <summary>
	/// Tests that a custom <see cref="ObsidianVaultDisplay.NotFound"/> render fragment is invoked
	/// when the path does not match any entity.
	/// </summary>
	[Fact]
	public void Render_CustomNotFoundFragment_IsRendered()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		RenderFragment<Templates.NotFound.NotFoundRenderContext> customNotFound =
			ctx => builder =>
			{
				builder.OpenElement(0, "section");
				builder.AddAttribute(1, "id", "custom-not-found");
				builder.CloseElement();
			};

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = _ctx.Render<ObsidianVaultDisplay>(p => p
			.Add(c => c.Vault, vault)
			.Add(c => c.CurrentPath, "missing")
			.Add(c => c.BasePath, "/")
			.Add(c => c.NotFound, customNotFound));

		// Assert – the custom element is rendered (confirming custom fragment is used)
		cut.Find("section#custom-not-found");
	}

	/// <summary>
	/// Tests that a custom <see cref="ObsidianVaultDisplay.FoundNote"/> render fragment is invoked
	/// when a note is found.
	/// </summary>
	[Fact]
	public async Task Render_CustomFoundNoteFragment_IsRendered()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("doc.md", Stream.Null);

		RenderFragment<Templates.FoundNote.FoundNoteRenderContext> customFoundNote =
			ctx => builder =>
			{
				builder.OpenElement(0, "section");
				builder.AddAttribute(1, "id", "custom-note");
				builder.AddContent(2, ctx.Note.Name);
				builder.CloseElement();
			};

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = _ctx.Render<ObsidianVaultDisplay>(p => p
			.Add(c => c.Vault, vault)
			.Add(c => c.CurrentPath, "doc")
			.Add(c => c.BasePath, "/")
			.Add(c => c.FoundNote, customFoundNote));

		// Assert
		AngleSharp.Dom.IElement section = cut.Find("section#custom-note");
		Assert.Equal("doc.md", section.TextContent);
	}

	/// <summary>
	/// Tests that with <see cref="ObsidianVaultDisplayOptions.DisplayIndexNoteNavigation"/> disabled,
	/// the index note template does not render the folder navigation tree.
	/// </summary>
	[Fact]
	public async Task Render_IndexNoteNavigationDisabled_OmitsFolderNav()
	{
		// Arrange
		const string content = "# Root";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(content));
		await vault.WriteNoteAsync("other.md", Stream.Null);

		ObsidianVaultDisplayOptions options = new() { DisplayIndexNoteNavigation = false };

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "/", options: options);

		// Assert – the folder nav should not be present
		Assert.Throws<ElementNotFoundException>(
			() => cut.Find("nav.moltenobsidian-folder-nav")
		);
	}

	/// <summary>
	/// Tests that with <see cref="ObsidianVaultDisplayOptions.DisplayIndexNoteNavigation"/> enabled,
	/// the index note template also renders the folder navigation.
	/// </summary>
	[Fact]
	public async Task Render_IndexNoteNavigationEnabled_IncludesFolderNav()
	{
		// Arrange
		const string content = "# Root";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(content));
		await vault.WriteNoteAsync("other.md", Stream.Null);

		ObsidianVaultDisplayOptions options = new() { DisplayIndexNoteNavigation = true };

		// Act
		IRenderedComponent<ObsidianVaultDisplay> cut = RenderDisplay(vault, currentPath: "/", options: options);

		// Assert – folder nav is rendered
		cut.Find("nav.moltenobsidian-folder-nav");
	}

	/// <summary>
	/// Tests that the display throws <see cref="InvalidOperationException"/> when no router is available.
	/// </summary>
	[Fact]
	public void Render_WithoutRouterFactory_ThrowsInvalidOperationException()
	{
		// Arrange – no VaultRouterFactory registered, and no Router set
		using BunitContext ctx = new();
		ctx.JSInterop.Mode = JSRuntimeMode.Loose;

		InMemoryVault vault = new("TestVault");

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() =>
			ctx.Render<ObsidianVaultDisplay>(p => p
				.Add(c => c.Vault, vault)
				.Add(c => c.CurrentPath, "doc")
				.Add(c => c.BasePath, "/"))
		);
	}
}
