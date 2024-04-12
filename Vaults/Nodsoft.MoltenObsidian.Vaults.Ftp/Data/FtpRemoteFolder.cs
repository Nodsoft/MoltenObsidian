using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

/// <summary>
/// Represents a folder in a remote FTP vault.
/// </summary>
public sealed class FtpRemoteFolder : IVaultFolder
{
    private List<FtpRemoteFile> _files = [];
    private List<FtpRemoteFolder> _subfolders = [];

    private FtpRemoteFolder() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FtpRemoteFolder"/> class.
    /// </summary>
    /// <param name="path">The path to the folder.</param>
    /// <param name="currentFolder">The parent folder of the new folder.</param>
    public FtpRemoteFolder(string path, IVaultFolder currentFolder)
    {
        Path = path.Replace('\\', '/');
        Name = Path.Split('/').Last();
        Vault = currentFolder.Vault;
    }

    /// <inheritdoc />
    public string Name { get; private init; }

    /// <inheritdoc />
    public string Path { get; private init; }

    /// <inheritdoc />
    public IVaultFolder? Parent { get; private init; }

    /// <inheritdoc />
    public IVault Vault { get; private init; }

    /// <inheritdoc />
    public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;

    /// <inheritdoc />
    public IReadOnlyList<IVaultFile> Files => _files;

    internal static FtpRemoteFolder FromRoot(string name, IVault vault) => new()
    {
        Name = name,
        Path = "",
        Vault = vault
    };

    internal void AddSubFolder(FtpRemoteFolder folder)
    {
        _subfolders.Add(folder);
    }

    
    internal void AttachFile(FtpRemoteFile file)
    {
        _files.Add(file);
    }
}