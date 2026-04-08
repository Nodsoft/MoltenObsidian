using System.Text;
using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Services;

/// <summary>
/// Provides tests for the <see cref="VaultRouter"/> class.
/// </summary>
/// <seealso cref="VaultRouter"/>
public sealed class VaultRouterTests
{
	/// <summary>
	/// Tests that routing to an existing folder returns the correct folder.
	/// </summary>
	[Fact]
	public async Task RouteTo_ExistingFolder_ReturnsFolder()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.CreateFolderAsync("notes");
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("notes");

		// Assert
		Assert.NotNull(result);
		Assert.IsAssignableFrom<IVaultFolder>(result);
		Assert.Equal("notes", result.Path);
	}

	/// <summary>
	/// Tests that routing to an existing note (without ".md" extension) returns the correct note.
	/// </summary>
	[Fact]
	public async Task RouteTo_ExistingNote_ReturnsNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("hello.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("hello");

		// Assert
		Assert.NotNull(result);
		Assert.IsAssignableFrom<IVaultNote>(result);
		Assert.Equal("hello.md", result.Path);
	}

	/// <summary>
	/// Tests that routing to an existing note in a subfolder returns the correct note.
	/// </summary>
	[Fact]
	public async Task RouteTo_NoteInSubfolder_ReturnsNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("notes/page.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("notes/page");

		// Assert
		Assert.NotNull(result);
		Assert.IsAssignableFrom<IVaultNote>(result);
		Assert.Equal("notes/page.md", result.Path);
	}

	/// <summary>
	/// Tests that routing to a path that doesn't exist returns null.
	/// </summary>
	[Fact]
	public void RouteTo_NonExistentPath_ReturnsNull()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("does-not-exist");

		// Assert
		Assert.Null(result);
	}

	/// <summary>
	/// Tests that case-sensitive routing is preferred when an exact match is found.
	/// </summary>
	[Fact]
	public async Task RouteTo_CaseSensitiveMatch_ReturnsEntity()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("MyNote.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("MyNote");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("MyNote.md", result.Path);
	}

	/// <summary>
	/// Tests that case-insensitive fallback routing works when no exact match is found.
	/// </summary>
	[Fact]
	public async Task RouteTo_CaseInsensitiveFallback_ReturnsEntity()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("MyNote.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act – path differs in casing
		IVaultEntity? result = router.RouteTo("mynote");

		// Assert
		Assert.NotNull(result);
		Assert.Equal("MyNote.md", result.Path);
	}

	/// <summary>
	/// Tests that the routing table is rebuilt when the vault raises an update event
	/// for an entity addition.
	/// </summary>
	[Fact]
	public async Task RouteTo_AfterVaultUpdate_ReflectsNewNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		VaultRouter router = new(vault);

		// Router is built before the note exists – should return null.
		Assert.Null(router.RouteTo("new-note"));

		// Act – add a note after the router was created.
		await vault.WriteNoteAsync("new-note.md", Stream.Null);

		// Assert – the router should pick up the new note after the vault update event.
		IVaultEntity? result = router.RouteTo("new-note");
		Assert.NotNull(result);
		Assert.IsAssignableFrom<IVaultNote>(result);
	}

	/// <summary>
	/// Tests that adding multiple notes are all registered in the routing table.
	/// </summary>
	[Fact]
	public async Task RouteTo_MultipleNotes_AllRoutable()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("alpha.md", Stream.Null);
		await vault.WriteNoteAsync("beta.md", Stream.Null);
		await vault.WriteNoteAsync("gamma.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act & Assert
		Assert.NotNull(router.RouteTo("alpha"));
		Assert.NotNull(router.RouteTo("beta"));
		Assert.NotNull(router.RouteTo("gamma"));
	}

	/// <summary>
	/// Tests that routing to a note path that still has ".md" extension returns null,
	/// since the routing table strips ".md" from note paths.
	/// </summary>
	[Fact]
	public async Task RouteTo_NotePathWithExtension_ReturnsNull()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("page.md", Stream.Null);
		VaultRouter router = new(vault);

		// Act – path still has ".md"
		IVaultEntity? result = router.RouteTo("page.md");

		// Assert – should be null because ".md" is stripped from routing table keys
		Assert.Null(result);
	}

	/// <summary>
	/// Tests that a note with content can be read through the vault after routing.
	/// </summary>
	[Fact]
	public async Task RouteTo_NoteWithContent_ContentReadable()
	{
		// Arrange
		const string content = "# Hello World";
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("readme.md", Encoding.UTF8.GetBytes(content));
		VaultRouter router = new(vault);

		// Act
		IVaultEntity? result = router.RouteTo("readme");

		// Assert
		Assert.NotNull(result);
		IVaultNote note = Assert.IsAssignableFrom<IVaultNote>(result);
		string readContent = await note.ReadDocumentAsync(TestContext.Current.CancellationToken);
		Assert.Equal(content, readContent);
	}
}
