using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal class InMemoryVaultFile : InMemoryVaultEntityBase, IVaultFile
{
    protected InMemoryVaultFile(FileInfo file, IVaultFolder parent, IVault vault) : base(file, parent, vault)
    {
        ContentType = MimeTypes.GetMimeType(fileName: file.Name);
    }

    public virtual string ContentType { get; }

    public string FullPath =>
        System.IO.Path.Join(((InMemoryVaultFolder)Vault.Root).PhysicalDirectoryInfo.FullName, Path);

    public async ValueTask<byte[]> ReadBytesAsync()
    {
        string fullPath = FullPath;
        return await File.ReadAllBytesAsync(fullPath);
    }

    public ValueTask<Stream> OpenReadAsync()
        => ValueTask.FromResult<Stream>(File.OpenRead(FullPath));
    
    public static InMemoryVaultFile Create(FileInfo file, IVaultFolder parent, IVault vault)
        => file.Extension is ".md"
            ? new InMemoryVaultNote(file, parent, vault)
            : new InMemoryVaultFile(file, parent, vault);
}