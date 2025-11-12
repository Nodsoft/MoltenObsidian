using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.InMemory;

/// <summary>
/// Provides a test fixture for the <see cref="InMemoryVault"/> class.
/// </summary>
[CollectionDefinition(nameof(InMemoryVault))]
public abstract class InMemoryVaultFixtureCollection : ICollectionFixture<InMemoryVaultFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}