using System.Text;
using Nodsoft.MoltenObsidian.Blazor.Helpers;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Helpers;

/// <summary>
/// Provides tests for the <see cref="VaultNavigationHelpers"/> class.
/// </summary>
/// <seealso cref="VaultNavigationHelpers"/>
public sealed class VaultNavigationHelpersTests
{
	/// <summary>
	/// Tests that a folder containing a "README.md" file returns it as the index note.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_WithReadmeMd_ReturnsNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Stream.Null);

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal("README.md", result.Path);
	}

	/// <summary>
	/// Tests that a folder containing an "index.md" file returns it as the index note.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_WithIndexMd_ReturnsNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("index.md", Stream.Null);

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal("index.md", result.Path);
	}

	/// <summary>
	/// Tests that "README.md" detection is case-insensitive.
	/// </summary>
	[Theory]
	[InlineData("readme.md")]
	[InlineData("Readme.md")]
	[InlineData("README.MD")]
	public async Task GetIndexNoteAsync_ReadmeMdCaseInsensitive_ReturnsNote(string filename)
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync(filename, Stream.Null);

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
	}

	/// <summary>
	/// Tests that "index.md" detection is case-insensitive.
	/// </summary>
	[Theory]
	[InlineData("index.md")]
	[InlineData("Index.md")]
	[InlineData("INDEX.MD")]
	public async Task GetIndexNoteAsync_IndexMdCaseInsensitive_ReturnsNote(string filename)
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync(filename, Stream.Null);

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
	}

	/// <summary>
	/// Tests that a folder with no index note returns null.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_NoIndexNote_ReturnsNull()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("other-note.md", Stream.Null);

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.Null(result);
	}

	/// <summary>
	/// Tests that an empty folder returns null.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_EmptyFolder_ReturnsNull()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.Null(result);
	}

	/// <summary>
	/// Tests that an index note with frontmatter setting "moltenobsidian:index:enabled" to false
	/// is not returned as the index note.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_DisabledByFrontmatter_ReturnsNull()
	{
		// Arrange
		const string content = """
			---
			moltenobsidian:index:enabled: false
			---

			# Hidden Index
			""";

		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(content));

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.Null(result);
	}

	/// <summary>
	/// Tests that an index note with frontmatter not setting "moltenobsidian:index:enabled"
	/// is returned normally.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_FrontmatterWithoutIndexKey_ReturnsNote()
	{
		// Arrange
		const string content = """
			---
			title: Home
			---

			# Home
			""";

		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(content));

		// Act
		IVaultNote? result = await vault.Root.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
	}

	/// <summary>
	/// Tests that an index note inside a subfolder is also correctly detected.
	/// </summary>
	[Fact]
	public async Task GetIndexNoteAsync_IndexNoteInSubfolder_ReturnsNote()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("section/README.md", Stream.Null);

		IVaultFolder? subfolder = (vault as IVault).GetFolder("section");

		// Act
		IVaultNote? result = await subfolder!.GetIndexNoteAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal("section/README.md", result.Path);
	}
}
