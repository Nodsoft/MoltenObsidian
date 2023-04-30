using FluentFTP.Exceptions;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

public class FtpRemoteFile : IVaultFile
{
    private readonly ManifestFile _manifestFile;

    protected FtpRemoteFile(ManifestFile file, string name, IVaultFolder parent)
    {
        _manifestFile = file;
        Name = name;
        Parent = parent;
        Vault = parent.Vault;
    }

    public string Name { get; }
    public string Path => _manifestFile.Path;
    public IVaultFolder? Parent { get; }
    public IVault Vault { get; }

    public string ContentType { get; }

    public async ValueTask<byte[]> ReadBytesAsync()
    {
        var client = ((FtpRemoteVault)Vault).AsyncFtpClient.EnsureConnected();
        var bytes = await client.DownloadBytes(_manifestFile.Path, default);
        return bytes;
    }

    public async ValueTask<Stream> OpenReadAsync()
    {
        var client = ((FtpRemoteVault)Vault).AsyncFtpClient.EnsureConnected();
        var stream = new MemoryStream();
        await client.DownloadStream(stream, _manifestFile.Path);
        return stream;
    }

    internal static FtpRemoteFile FromManifest(ManifestFile file, string name, IVaultFolder parent) =>
        file.ContentType?.StartsWith("text/markdown", StringComparison.OrdinalIgnoreCase)
        ?? name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? new FtpRemoteNote(file, name, parent)
            : new FtpRemoteFile(file, name, parent);
}