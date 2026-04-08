using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Tests.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;


namespace Nodsoft.MoltenObsidian.Tests.Manifest;


/// <summary>
/// Provides tests for the vault manifest system
/// </summary>
[Collection("VaultManifest")]
public sealed class VaultManifestTests : IClassFixture<FileSystemVaultFixture>
{
    private readonly FileSystemVaultFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="VaultManifestTests"/> class.
    /// </summary>
    /// <param name="fixture">The vault fixture to use for testing.</param>
    public VaultManifestTests(FileSystemVaultFixture fixture)
    {
        _fixture = fixture;
    }
    
    /// <summary>
    /// Tests that a vault manifest can be generated from a vault.
    /// </summary>
    [Fact]
    public async Task GenerateManifest_Created()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        RemoteVaultManifest manifest = await VaultManifestGenerator.GenerateManifestAsync(vault, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(manifest);
        Assert.Equal(vault.Name, manifest.Name);
    }
    
    /// <summary>
    /// Tests that a vault manifest is equivalent in file paths to its source vault.
    /// </summary>
    [Fact]
    public async Task GenerateManifest_EquivalentPaths()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        RemoteVaultManifest manifest = await VaultManifestGenerator.GenerateManifestAsync(vault, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(manifest);
        Assert.Equal(vault.Files.Count, manifest.Files.Count);
        
        Assert.Equivalent(
            vault.Files.Select(f => f.Key), 
            manifest.Files.Select(f => f.Path)
        );
    }
    
    /// <summary>
    /// Tests that a vault manifest is equivalent in file sizes to its source vault.
    /// </summary>
    [Fact]
    public async Task GenerateManifest_EquivalentSizes()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        RemoteVaultManifest manifest = await VaultManifestGenerator.GenerateManifestAsync(vault, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(manifest);
        Assert.Equal(vault.Files.Count, manifest.Files.Count);
        await Assert.AllAsync(vault.Files, async f =>
        {
            ManifestFile manifestFile = manifest.Files.FirstOrDefault(mf => mf.Path == f.Key);
            await using Stream stream = await f.Value.OpenReadAsync();

            Assert.Equal(stream.Length, manifestFile.Size);
        });
    }
    
    /// <summary>
    /// Tests that a vault manifest is equivalent in file hashes to its source vault.
    /// </summary>
    [Fact]
    public async Task GenerateManifest_EquivalentHashes()
    {
        // Arrange
        FileSystemVault vault = _fixture.Vault;
        
        // Act
        RemoteVaultManifest manifest = await VaultManifestGenerator.GenerateManifestAsync(vault, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.NotNull(manifest);
        Assert.Equal(vault.Files.Count, manifest.Files.Count);
        await Assert.AllAsync(vault.Files, async f =>
        {
            ManifestFile manifestFile = manifest.Files.FirstOrDefault(mf => mf.Path == f.Key);
            await using Stream stream = await f.Value.OpenReadAsync();
            string hash = await VaultManifestGenerator.HashDataAsync(stream, TestContext.Current.CancellationToken);

            Assert.Equal(hash, manifestFile.Hash);
        });
    }
}