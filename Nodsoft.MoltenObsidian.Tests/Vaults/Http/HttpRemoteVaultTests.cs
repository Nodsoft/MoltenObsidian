using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Http;
using Nodsoft.MoltenObsidian.Vaults.Http.Data;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.Http;

/// <summary>
/// Provides tests for the <see cref="HttpRemoteVault"/> class.
/// </summary>
[Collection(nameof(HttpRemoteVault))]
public sealed class HttpRemoteVaultTests
{
	private readonly HttpVaultServerFixture _fixture;

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpRemoteVaultTests" /> class.
	/// </summary>
	public HttpRemoteVaultTests(HttpVaultServerFixture fixture)
	{
		_fixture = fixture;
	}
	
	/// <summary>
	/// Tests that the vault can be created.
	/// </summary>
	[Fact]
	public async Task CreateVaultFromManifest_Nominal()
	{
		// Arrange
		// Act
		IVault vault = await _fixture.CreateHttpRemoteVaultAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(vault);
		Assert.NotNull(vault.Root);
		Assert.Equal("TestVault", vault.Root.Name);
	}
	
	/// <summary>
	/// Tests the retrieval of a file from a <see cref="HttpRemoteVault" />.
	/// </summary>
	/// <param name="path">The path of the file to retrieve.</param>
	[Theory]
	[InlineData("README.md")]
	[InlineData("VeryNiceFolder/Hidden Note.md")]
	public void GetFile_Nominal(string path)
	{
		// Arrange
		IVault vault = _fixture.Vault;
        
		// Act
		IVaultFile? file = vault.Files.GetValueOrDefault(path);
        
		// Assert
		Assert.NotNull(file);
		Assert.Equal(path, file.Path);
	}
	
	/// <summary>
	/// Tests the retrieval of a non-existent file from a <see cref="HttpRemoteVault" />.
	/// </summary>
	[Fact]
	public void GetFile_FileDoesNotExist()
	{
		// Arrange
		IVault vault = _fixture.Vault;
        
		// Act
		IVaultFile? file = vault.GetFile("NonExistentFile.txt");
        
		// Assert
		Assert.Null(file);
	}
	
	/// <summary>
	/// Tests the retrieval of a directory from a <see cref="HttpRemoteVault" />.
	/// </summary>
	/// <param name="path">The path of the directory to retrieve.</param>
	[Theory]
	[InlineData("VeryNiceFolder")]
	public void GetDirectory_Nominal(string path)
	{
		// Arrange
		IVault vault = _fixture.Vault;
        
		// Act
		IVaultFolder? folder = vault.GetFolder(path);
        
		// Assert
		Assert.NotNull(folder);
		Assert.Equal(path, folder.Path);
	}
	
	/// <summary>
	/// Tests the retrieval of a non-existent directory from a <see cref="HttpRemoteVault" />.
	/// </summary>
	[Fact]
	public void GetDirectory_NonExistent_Nominal()
	{
		// Arrange
		IVault vault = _fixture.Vault;
        
		// Act
		IVaultFolder? folder = vault.GetFolder("NonExistentFolder");
        
		// Assert
		Assert.Null(folder);
	}
	
	/// <summary>
	/// Tests the correct typing of a markdown file as a note.
	/// </summary>
	[Fact]
	public void GetFile_FileIsNote_Nominal()
	{
		// Arrange
		IVault vault = _fixture.Vault;
        
		// Act
		IVaultFile? file = vault.GetFile("VeryNiceFolder/Hidden Note.md");
        
		// Assert
		Assert.NotNull(file);
		Assert.Equal("text/markdown", file.ContentType);
		Assert.IsAssignableFrom<IVaultNote>(file);
		Assert.IsType<HttpRemoteNote>(file);
	}
	
	/// <summary>
	/// Tests the content of a note file.
	/// </summary>
	[Fact]
	public async Task GetNoteContent_SimpleNote_Nominal()
	{
		// Arrange
		IVault vault = _fixture.Vault;
		IVaultNote note = vault.Notes["VeryNiceFolder/Hidden Note.md"];
        
		// Act
		string content = await note.ReadDocumentAsync(TestContext.Current.CancellationToken);
        
		// Assert
		Assert.NotNull(note);
		Assert.Equal(/*lang=md*/$"This is a hidden note!{Environment.NewLine}Not at all.{Environment.NewLine}It's just in a folder.", content);
	}
	
	/// <summary>
	/// Tests the HTML conversion of a note, containing tags and internal links.
	/// </summary>
	[Fact]
	public async Task GetNoteContent_ComplexNote_Nominal()
	{
		// Arrange
		IVault vault = _fixture.Vault;
		IVaultNote note = vault.Notes["README.md"];
        
		// Act
		ObsidianText content = await note.ReadDocumentAsync(TestContext.Current.CancellationToken);
        
		// Assert
		Assert.NotNull(note);
        
		Assert.Equal(
			/*lang=md*/$"# README{Environment.NewLine}{Environment.NewLine}" +
			$"This is a test vault, meant to be used with the `Nodsoft.MoltenObsidian.Tests` project's test suite.{Environment.NewLine}{Environment.NewLine}" +
			$"## See also :{Environment.NewLine}{Environment.NewLine}Check out this #cool_tag and #cooler_tag, and my [[Hidden Note]].", 
			content
		);
        
		Assert.Equal(
			/*lang=html*/"<h1 id=\"readme\">README</h1>\n<p>This is a test vault, meant to be used with the <code>Nodsoft.MoltenObsidian.Tests</code> project's test suite.</p>\n" +
			/*lang=html*/"<h2 id=\"see-also\">See also :</h2>\n<p>Check out this <span class=\"tag moltenobsidian-tag\" data-name=\"cool_tag\">cool_tag</span> and <span class=\"tag moltenobsidian-tag\" data-name=\"cooler_tag\">cooler_tag</span>, " +
			/*lang=html*/"and my <a href=\"VeryNiceFolder/Hidden%20Note\" title=\"Hidden Note\">Hidden Note</a>.</p>\n",
			content.ToHtml()
		);
	}
}