using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

public sealed class FtpRemoteFolder : IVaultFolder
{
    private List<FtpRemoteFile> _files = new();
    private List<FtpRemoteFolder> _subfolders = new();

    private FtpRemoteFolder()
    {
    }

    public FtpRemoteFolder(string part, IVaultFolder currentFolder)
    {
        Name = part.Split('/').Last();
        Path = part.Replace('\\', '/');
        Vault = currentFolder.Vault;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public IVaultFolder? Parent { get; private init; }
    public IVault Vault { get; private init; }
    public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;
    public IReadOnlyList<IVaultFile> Files => _files;

    internal static FtpRemoteFolder FromRoot(string name, IVault vault) =>
        new()
        {
            Name = name,
            Path = "",
            Vault = vault
        };

    internal void AddSubFolder(FtpRemoteFolder folder)
    {
        _subfolders.Add(folder);
    }

    public void AddFile(FtpRemoteFile file)
    {
        _files.Add(file);
    }
}