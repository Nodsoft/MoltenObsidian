using Bunit;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Components;

/// <summary>
/// Provides tests for the <see cref="ObsidianVaultNavigation"/> Razor component.
/// </summary>
/// <seealso cref="ObsidianVaultNavigation"/>
public sealed class ObsidianVaultNavigationTests : IDisposable
{
	private readonly BunitContext _ctx;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianVaultNavigationTests"/> class.
	/// </summary>
	public ObsidianVaultNavigationTests()
	{
		_ctx = new BunitContext();
	}

	/// <inheritdoc/>
	public void Dispose() => _ctx.Dispose();

	/// <summary>
	/// Tests that the navigation component renders a &lt;nav&gt; element for an empty vault.
	/// </summary>
	[Fact]
	public void Render_EmptyVault_RendersNavElement()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – a <nav> element must be present at the root
		cut.Find("nav");
	}

	/// <summary>
	/// Tests that notes in the root of the vault are rendered as anchor links.
	/// </summary>
	[Fact]
	public async Task Render_WithRootNote_RendersNoteLink()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("my-note.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – a link to "my-note" (without .md) must be present
		AngleSharp.Dom.IElement link = cut.Find("a[href='my-note']");
		Assert.Equal("my-note", link.TextContent);
	}

	/// <summary>
	/// Tests that multiple notes are all rendered as links.
	/// </summary>
	[Fact]
	public async Task Render_WithMultipleNotes_RendersAllNoteLinks()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("alpha.md", Stream.Null);
		await vault.WriteNoteAsync("beta.md", Stream.Null);
		await vault.WriteNoteAsync("gamma.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert
		cut.Find("a[href='alpha']");
		cut.Find("a[href='beta']");
		cut.Find("a[href='gamma']");
	}

	/// <summary>
	/// Tests that a subfolder is rendered inside a wrapping &lt;div&gt; element.
	/// </summary>
	[Fact]
	public async Task Render_WithSubfolder_RendersSubfolderDiv()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/page.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – a nested note link for "section/page" must be present
		cut.Find("a[href='section/page']");
	}

	/// <summary>
	/// Tests that the ".obsidian" system folder is hidden from the navigation tree.
	/// </summary>
	[Fact]
	public async Task Render_ObsidianSystemFolder_IsHidden()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync(".obsidian/config.md", Stream.Null);
		await vault.WriteNoteAsync("visible-note.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – the ".obsidian" folder link must not appear
		Assert.Throws<ElementNotFoundException>(
			() => cut.Find("a[href='.obsidian/']")
		);

		// But the visible note must still appear
		cut.Find("a[href='visible-note']");
	}

	/// <summary>
	/// Tests that a subfolder with an index note is rendered as a link (with trailing slash).
	/// </summary>
	[Fact]
	public async Task Render_SubfolderWithIndexNote_RendersFolderAsLink()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/index.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – the folder should have a link ending with "/"
		cut.Find("a[href='section/']");
	}

	/// <summary>
	/// Tests that a subfolder without an index note is rendered as a non-link div.
	/// </summary>
	[Fact]
	public async Task Render_SubfolderWithoutIndexNote_RendersFolderAsDiv()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/page.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
		);

		// Assert – the folder should NOT have a direct link (no <a> for the folder itself)
		Assert.Throws<ElementNotFoundException>(
			() => cut.Find("a[href='section/']")
		);
	}

	/// <summary>
	/// Tests that custom navigation attributes are applied to the &lt;nav&gt; element.
	/// </summary>
	[Fact]
	public void Render_WithCustomNavigationAttributes_AppliesAttributes()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
				.Add(c => c.NavigationAttributes, _ => new() { ["class"] = "my-nav", ["id"] = "sidebar" })
		);

		// Assert
		AngleSharp.Dom.IElement nav = cut.Find("nav");
		Assert.Equal("my-nav", nav.GetAttribute("class"));
		Assert.Equal("sidebar", nav.GetAttribute("id"));
	}

	/// <summary>
	/// Tests that custom note attributes are applied to the note link elements.
	/// </summary>
	[Fact]
	public async Task Render_WithCustomNoteAttributes_AppliesAttributesToLinks()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("page.md", Stream.Null);

		// Act
		IRenderedComponent<ObsidianVaultNavigation> cut = _ctx.Render<ObsidianVaultNavigation>(
			p => p.Add(c => c.Vault, vault)
				.Add(c => c.NoteAttributes, _ => new() { ["class"] = "note-link" })
		);

		// Assert
		AngleSharp.Dom.IElement link = cut.Find("a[href='page']");
		Assert.Equal("note-link", link.GetAttribute("class"));
	}
}
