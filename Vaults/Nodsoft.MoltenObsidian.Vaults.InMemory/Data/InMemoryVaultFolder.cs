using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

public class InMemoryVaultFolder : IVaultFolder
{
    public string Name { get; }
    public string Path { get; }
    public IVaultFolder? Parent { get; }
    public IVault Vault { get; }
    public IReadOnlyList<IVaultFolder> Subfolders { get; }
    public IReadOnlyList<IVaultFile> Files { get; }
    
    public GetFolders()
}