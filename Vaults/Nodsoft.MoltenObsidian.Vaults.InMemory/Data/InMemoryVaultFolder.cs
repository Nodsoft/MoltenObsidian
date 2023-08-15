using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal sealed class InMemoryVaultFolder : InMemoryVaultEntityBase, IVaultFolder
{
    internal DirectoryInfo PhysicalDirectoryInfo { get; }
    
    private readonly InMemoryVaultFolder[] _subfolders;
    private readonly InMemoryVaultFile[] _files;

    internal InMemoryVaultFolder(DirectoryInfo entity, IVaultFolder? parent, IVault vault) : base(entity, parent, vault)
    {
        PhysicalDirectoryInfo = entity;
        _subfolders = entity.GetDirectories().Select(d => new InMemoryVaultFolder(d, this, vault)).ToArray();
        _files = entity.GetFiles().Select(f => InMemoryVaultFile.Create(f, this, vault)).ToArray();
    }

    public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;
    public IReadOnlyList<IVaultFile> Files => _files;
}