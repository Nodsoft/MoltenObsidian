using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal abstract class InMemoryVaultEntityBase : IVaultEntity
{
    private protected InMemoryVaultEntityBase(string name, InMemoryVaultFolder? parent, InMemoryVault vault)
    {
        Name = name;
        Parent = parent;
        Path = Parent is null ? "" : System.IO.Path.Combine(Parent.Path, Name).Replace('\\', '/');
        Vault = vault;
    }
    
    public string Name { get; }
    
    public InMemoryVaultFolder? Parent { get; }
    
    // This is the path relative to the vault root.
    public string Path { get; }
    
    public InMemoryVault Vault { get; }
    
    
    IVault IVaultEntity.Vault => Vault;
    IVaultFolder? IVaultEntity.Parent => Parent;
}