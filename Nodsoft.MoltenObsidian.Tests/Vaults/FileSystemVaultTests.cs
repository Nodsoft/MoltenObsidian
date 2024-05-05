using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

namespace Nodsoft.MoltenObsidian.Tests.Vaults;

/// <summary>
/// Provides tests for the <see cref="FileSystemVault"/> class.
/// </summary>
/// <seealso cref="FileSystemVault"/>
[Collection(nameof(FileSystemVault))]
public class FileSystemVaultTests
{
    private readonly FileSystemVaultFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemVaultTests"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use for the tests.</param>
    public FileSystemVaultTests(FileSystemVaultFixture fixture)
    {
        _fixture = fixture;
    }
    
    /// <summary>
    /// Tests the creation of a <see cref="FileSystemVault"/> from a directory.
    /// </summary>
    [Fact]
    public void FromDirectory_Existing_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        FileSystemVaultFolder root = (FileSystemVaultFolder)vault.Root;
        
        // Act
        // Assert
        Assert.NotNull(vault);
        Assert.NotNull(vault.Root);
        Assert.Equal("TestVault", vault.Name);
        Assert.Equal("TestVault", root.Name);
        Assert.Equal(_fixture.DirectoryInfo, root.PhysicalDirectoryInfo);
        Assert.Equal("", root.Path);
    }
    
    /// <summary>
    /// Tests the creation of a <see cref="FileSystemVault"/> from a directory that does not exist.
    /// </summary>
    [Fact]
    public void FromDirectory_NonExistent_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        DirectoryInfo nonExistentFolder = new("NonExistentFolder");
        
        // Act
        Action act = () => FileSystemVault.FromDirectory(nonExistentFolder);
        
        // Assert
        Assert.False(nonExistentFolder.Exists);
        Assert.Throws<DirectoryNotFoundException>(act);
    }
    
    /// <summary>
    /// Tests the retrieval of a file from a <see cref="FileSystemVault"/>.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    [Theory]
    [InlineData("README.md")]
    [InlineData("VeryNiceFolder/Hidden Note.md")]
    public void GetFile_Nominal(string path)
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        IVaultFile? file = vault.GetFile(path);
        
        // Assert
        Assert.NotNull(file);
        Assert.Equal(path, file.Path);
    }
    
    /// <summary>
    /// Tests the retrieval of a non-existent file from a <see cref="FileSystemVault"/>.
    /// </summary>
    [Fact]
    public void GetFile_FileDoesNotExist()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        IVaultFile? file = vault.GetFile("NonExistentFile.txt");
        
        // Assert
        Assert.Null(file);
    }
    
    /// <summary>
    /// Tests the retrieval of a directory from a <see cref="FileSystemVault"/>.
    /// </summary>
    /// <param name="path">The path of the directory to retrieve.</param>
    [Theory]
    [InlineData("VeryNiceFolder")]
    public void GetDirectory_Nominal(string path)
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        IVaultFolder? folder = vault.GetFolder(path);
        
        // Assert
        Assert.NotNull(folder);
        Assert.Equal(path, folder.Path);
    }
    
    /// <summary>
    /// Tests the retrieval of a non-existent directory from a <see cref="FileSystemVault"/>.
    /// </summary>
    [Fact]
    public void GetDirectory_NonExistent_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        IVaultFolder? folder = vault.GetFolder("NonExistentFolder");
        
        // Assert
        Assert.Null(folder);
    }
    
    /// <summary>
    /// Tests the retrieval of an ignored folder from a <see cref="FileSystemVault"/>.
    /// </summary>
    [Fact]
    public void GetDirectory_IgnoredFolder_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        const string path = ".obsidian";
        IVaultFolder? folder = vault.GetFolder(path);
        
        // Assert
        Assert.Null(folder);
        Assert.Contains(path, vault.ExcludedFolders);
        
    }
    
    /// <summary>
    /// Tests the correct typing of a markdown file as a note.
    /// </summary>
    [Fact]
    public void GetFile_FileIsNote_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        IVaultFile? file = vault.GetFile("VeryNiceFolder/Hidden Note.md");
        
        // Assert
        Assert.NotNull(file);
        Assert.Equal("text/markdown", file.ContentType);
        Assert.IsAssignableFrom<IVaultNote>(file);
        Assert.IsType<FileSystemVaultNote>(file);
    }
    
    /// <summary>
    /// Tests the content of a note file.
    /// </summary>
    [Fact]
    public async Task GetNoteContent_SimpleNote_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        IVaultNote note = vault.Notes["VeryNiceFolder/Hidden Note.md"];
        
        // Act
        string content = await note.ReadDocumentAsync();
        
        // Assert
        Assert.NotNull(note);
        Assert.Equal(/*lang=md*/"This is a hidden note!\nNot at all.\nIt's just in a folder.", content);
    }
    
    /// <summary>
    /// Tests the HTML conversion of a note, containing tags and internal links.
    /// </summary>
    [Fact]
    public async Task GetNoteContent_ComplexNote_Nominal()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        IVaultNote note = vault.Notes["README.md"];
        
        // Act
        ObsidianText content = await note.ReadDocumentAsync();
        
        // Assert
        Assert.NotNull(note);
        
        Assert.Equal(
            /*lang=md*/"# README\n\nThis is a test vault, meant to be used with the `Nodsoft.MoltenObsidian.Tests` project's test suite.\n\n" +
            /*lang=md*/"## See also :\n\nCheck out this #cool_tag and #cooler_tag, and my [[Hidden Note]].", 
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