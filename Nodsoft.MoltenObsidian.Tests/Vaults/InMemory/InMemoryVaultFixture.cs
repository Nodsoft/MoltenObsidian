using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.InMemory;

/// <summary>
/// Provides a test fixture for the <see cref="InMemoryVault"/> class.
/// </summary>

[UsedImplicitly]
public sealed class InMemoryVaultFixture : IDisposable
{
    /// <summary>
    /// The name of the vault being tested.
    /// </summary>
    public const string VaultName = "TestVault";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryVaultFixture"/> class.
    /// </summary>
    public InMemoryVaultFixture()
    {
        Vault = new(VaultName);
    }

    /// <summary>
    /// The vault being tested.
    /// </summary>
    public InMemoryVault Vault { get; }
    
    /// <summary>
    /// Disposes of the resources used by the fixture.
    /// </summary>
    public void Dispose() { }
}