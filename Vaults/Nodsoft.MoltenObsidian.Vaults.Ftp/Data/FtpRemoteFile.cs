using FluentFTP;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

public class FtpRemoteFile : IVaultFile
{
    private readonly ManifestFile _manifestFile;
    public string Name { get; set; }
    public string Path { get; set; }
    public IVaultFolder? Parent { get; private init; }
    public IVault Vault { get; private set; }

    protected FtpRemoteFile(ManifestFile file, string name, IVaultFolder Parent)
    {
        _manifestFile = file;
        Name = name;
        this.Parent = Parent;
    }

    internal static FtpRemoteFile FromManifest(ManifestFile file, string name, IVaultFolder parent)
        => file.ContentType?.StartsWith("text/markdown", StringComparison.OrdinalIgnoreCase)
           ?? name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? new FtpRemoteNote(file, name, parent)
            : new FtpRemoteFile(file, name, parent);
    
    public string ContentType { get; }
    public async ValueTask<byte[]> ReadBytesAsync()
    {
        AsyncFtpClient client = ((FtpRemoteVault) Vault).AsyncFtpClient;
        return await client.DownloadBytes(_manifestFile.Path, default);
    }

    public async ValueTask<Stream> OpenReadAsync()
    {
        AsyncFtpClient client = ((FtpRemoteVault)Vault).AsyncFtpClient;
        var stream = new MemoryStream();
        await client.DownloadStream(stream, _manifestFile.Path);
        return stream;
    }
}