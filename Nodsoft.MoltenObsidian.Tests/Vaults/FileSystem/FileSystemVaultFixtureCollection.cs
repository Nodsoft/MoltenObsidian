using Nodsoft.MoltenObsidian.Vaults.FileSystem;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.FileSystem;

/// <summary>
/// Provides a test fixture for the <see cref="FileSystemVault"/> class.
/// </summary>
[CollectionDefinition(nameof(FileSystemVault))]
public abstract class FileSystemVaultFixtureCollection : ICollectionFixture<FileSystemVaultFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}