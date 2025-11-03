using Nodsoft.MoltenObsidian.Vaults.FileSystem;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.FileSystem;

/// <summary>
/// Provides a test fixture for the <see cref="FileSystemVault"/> class.
/// </summary>

public sealed class FileSystemVaultFixture : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemVaultFixture"/> class.
    /// </summary>
    public FileSystemVaultFixture()
    {
        DirectoryInfo = new("Assets/TestVault");
        Vault = FileSystemVault.FromDirectory(DirectoryInfo);
    }

    /// <summary>
    /// The vault being tested.
    /// </summary>
    public FileSystemVault Vault { get; }

    /// <summary>
    /// The directory that the vault is based on.
    /// </summary>
    public DirectoryInfo DirectoryInfo { get; }

    /// <summary>
    /// Disposes of the resources used by the fixture.
    /// </summary>
    public void Dispose() { }
}