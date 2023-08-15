using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal abstract class InMemoryVaultEntityBase : IVaultEntity
{
    private protected InMemoryVaultEntityBase(FileSystemInfo entity, IVaultFolder? parent, IVault vault)
    {
        Name = entity.Name;
        Parent = parent;
        Path = Parent is null ? "" : System.IO.Path.Combine(Parent.Path, Name).Replace('\\', '/');
        Vault = vault;
    }
    public string Name { get; }
    public IVaultFolder? Parent { get; }
    public string Path { get; }
    public IVault Vault { get; }
}