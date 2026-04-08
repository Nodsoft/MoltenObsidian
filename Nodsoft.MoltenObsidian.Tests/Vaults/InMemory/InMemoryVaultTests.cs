using System.Text;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;
using Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.InMemory;

/// <summary>
/// Provides tests for the <see cref="InMemoryVault"/> class.
/// </summary>
/// <seealso cref="InMemoryVault"/>
[Collection(nameof(InMemoryVault))]
public sealed class InMemoryVaultTests
{
    private readonly InMemoryVaultFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryVaultTests"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use for the tests.</param>
    public InMemoryVaultTests(InMemoryVaultFixture fixture)
    {
        _fixture = fixture;
    }
    
    /// <summary>
    /// Tests the creation of a new <see cref="InMemoryVault"/>.
    /// </summary>
    [Fact]
    public void CreateVault_Empty_Nominal()
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        InMemoryVaultFolder root = (InMemoryVaultFolder)vault.Root;
        
        // Act
        // Assert
        Assert.NotNull(vault);
        Assert.NotNull(vault.Root);
        Assert.Equal("TestVault", vault.Name);
        Assert.Equal("/", root.Name);
        Assert.Equal("", root.Path);
    }
    
    /// <summary>
    /// Tests the retrieval of a newly-created file from a <see cref="InMemoryVault"/>.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    [Theory]
    [InlineData("README.md")]
    [InlineData("VeryNiceFolder/Hidden Note.md")]
    public async Task CreateAndGetFile_Nominal(string path)
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        await vault.WriteNoteAsync(path, Stream.Null);
        
        // Act
        IVaultFile? file = (vault as IVault).GetFile(path);
        
        // Assert
        Assert.NotNull(file);
        Assert.Equal(path, file.Path);
        
        // Cleanup
        await vault.DeleteFileAsync(path);
    }
    
    /// <summary>
    /// Tests the retrieval of a non-existent file from a <see cref="InMemoryVault"/>.
    /// </summary>
    [Fact]
    public void GetFile_FileDoesNotExist()
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        
        // Act
        IVaultFile? file = (vault as IVault).GetFile("NonExistentFile.txt");
        
        // Assert
        Assert.Null(file);
    }
    
    /// <summary>
    /// Tests the retrieval of a newly-created directory from a <see cref="InMemoryVault"/>.
    /// </summary>
    /// <param name="path">The path of the directory to retrieve.</param>
    [Theory]
    [InlineData("NiceFolder")]
    public async Task CreateAndGetDirectory_Nominal(string path)
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        await vault.CreateFolderAsync(path);
        
        // Act
        IVaultFolder? folder = (vault as IVault).GetFolder(path);
        
        // Assert
        Assert.NotNull(folder);
        Assert.Equal(path, folder.Path);
        
        // Cleanup
        await vault.DeleteFolderAsync(path);
    }
    
    /// <summary>
    /// Tests the retrieval of a non-existent directory from a <see cref="InMemoryVault"/>.
    /// </summary>
    [Fact]
    public void GetDirectory_NonExistent_Nominal()
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        
        // Act
        IVaultFolder? folder = (vault as IVault).GetFolder("NonExistentFolder");
        
        // Assert
        Assert.Null(folder);
    }
    
    // /// <summary>
    // /// Tests the retrieval of an ignored folder from a <see cref="InMemoryVault"/>.
    // /// </summary>
    // [Fact]
    // public void GetDirectory_IgnoredFolder_Nominal()
    // {
    //     // Arrange
    //     InMemoryVault vault = _fixture.Vault;
    //     
    //     // Act
    //     const string path = ".obsidian";
    //     IVaultFolder? folder = (vault as IVault).GetFolder(path);
    //     
    //     // Assert
    //     Assert.Null(folder);
    //     Assert.Contains(path, vault.ExcludedFolders);
    //     
    // }
    
    /// <summary>
    /// Tests the correct typing of a Markdown file as a note.
    /// </summary>
    [Theory]
    [InlineData("mynote.md")]
    public async Task CreateAndGetFile_FileIsNote_Nominal(string path)
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        await vault.WriteNoteAsync(path, Stream.Null);
        
        // Act
        IVaultFile? file = (vault as IVault).GetFile(path);
        
        // Assert
        Assert.NotNull(file);
        Assert.Equal("text/markdown", file.ContentType);
        Assert.IsAssignableFrom<IVaultNote>(file);
        Assert.IsType<InMemoryVaultNote>(file);
    }
    
    /// <summary>
    /// Tests the content of a note file.
    /// </summary>
    [Fact]
    public async Task GetNoteContent_SimpleNote_Nominal()
    {
        string markdownText = /*lang=md*/$"This is a hidden note!{Environment.NewLine}Not at all.{Environment.NewLine}It's just in a folder.";
        
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        await vault.WriteNoteAsync("VeryNiceFolder/Hidden Note.md", Encoding.UTF8.GetBytes(markdownText));
        
        // Act
        IVaultNote note = vault.Notes["VeryNiceFolder/Hidden Note.md"];
        string content = await note.ReadDocumentAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(note);
        Assert.Equal(markdownText, content);
        
        // Cleanup
        await vault.DeleteFileAsync("VeryNiceFolder/Hidden Note.md");
    }
    
    /// <summary>
    /// Tests the HTML conversion of a note, containing tags and internal links.
    /// </summary>
    [Fact]
    public async Task GetNoteContent_ComplexNote_Nominal()
    {
        // Arrange
        string markdownText = /*lang=md*/
            $"# README{Environment.NewLine}{Environment.NewLine}" +
            $"This is a test vault, meant to be used with the `Nodsoft.MoltenObsidian.Tests` project's test suite.{Environment.NewLine}{Environment.NewLine}" +
            $"## See also :{Environment.NewLine}{Environment.NewLine}Check out this #cool_tag and #cooler_tag, and my [[Hidden Note]].";

        const string htmlText =
            /*lang=html*/"<h1 id=\"readme\">README</h1>\n<p>This is a test vault, meant to be used with the <code>Nodsoft.MoltenObsidian.Tests</code> project's test suite.</p>\n" +
            /*lang=html*/"<h2 id=\"see-also\">See also :</h2>\n<p>Check out this <span class=\"tag moltenobsidian-tag\" data-name=\"cool_tag\">cool_tag</span> and <span class=\"tag moltenobsidian-tag\" data-name=\"cooler_tag\">cooler_tag</span>, " +
            /*lang=html*/"and my <a href=\"VeryNiceFolder/Hidden%20Note\" title=\"Hidden Note\">Hidden Note</a>.</p>\n";
        
        InMemoryVault vault = _fixture.Vault;
        
        // Create both tested note and related hidden note.
        await vault.WriteNoteAsync("VeryNiceFolder/Hidden Note.md", Stream.Null);
        await vault.WriteNoteAsync("README.md", Encoding.UTF8.GetBytes(markdownText));
        
        
        // Act
        IVaultNote note = vault.Notes["README.md"];
        ObsidianText content = await note.ReadDocumentAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(note);
        Assert.Equal(markdownText, content);
        Assert.Equal(htmlText, content.ToHtml());
        
        // Cleanup
        await vault.DeleteFileAsync("README.md");
        await vault.DeleteFileAsync("VeryNiceFolder/Hidden Note.md");
    }
    
    /// <summary>
    /// Tests that a file is not added twice to the parent folder's Files collection.
    /// This test verifies the fix for a potential race condition where line 58 adds the file
    /// directly to parent.Files, and line 59 calls AddChildReference which also adds to parent.Files.
    /// </summary>
    [Fact]
    public async Task CreateFile_FileNotAddedTwice_Nominal()
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        const string filePath = "test-duplicate.md";
        
        // Act
        await vault.WriteNoteAsync(filePath, Stream.Null);
        InMemoryVaultFolder root = (InMemoryVaultFolder)vault.Root;
        
        // Assert - count occurrences of the file in the Files collection
        int fileCount = root.Files.Count(f => f.Path == filePath);
        Assert.Equal(1, fileCount);
        
        // Cleanup
        await vault.DeleteFileAsync(filePath);
    }
    
    /// <summary>
    /// Tests that a file in a subfolder is not added twice to the parent folder's Files collection.
    /// </summary>
    [Fact]
    public async Task CreateFile_InSubfolderFileNotAddedTwice_Nominal()
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        const string filePath = "SubFolder/test-duplicate.md";
        
        // Act
        await vault.WriteNoteAsync(filePath, Stream.Null);
        IVaultFolder? folder = (vault as IVault).GetFolder("SubFolder");
        
        // Assert
        Assert.NotNull(folder);
        InMemoryVaultFolder inMemoryFolder = (InMemoryVaultFolder)folder;
        int fileCount = inMemoryFolder.Files.Count(f => f.Path == filePath);
        Assert.Equal(1, fileCount);
        
        // Cleanup
        await vault.DeleteFileAsync(filePath);
        await vault.DeleteFolderAsync("SubFolder");
    }
    
    /// <summary>
    /// Tests the error handling when creating folders on invalid paths
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/")]
    public async Task CreateFolder_InvalidPath_ThrowsArgumentException(string path)
    {
        // Arrange
        InMemoryVault vault = _fixture.Vault;
        
        // Act
        // ReSharper disable once ConvertToLocalFunction
        Func<Task<IVaultFolder>> action = async () => await vault.CreateFolderAsync(path!);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);
    }
}