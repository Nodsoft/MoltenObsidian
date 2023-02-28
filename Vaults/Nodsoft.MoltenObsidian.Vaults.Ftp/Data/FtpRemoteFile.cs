using FluentFTP.Exceptions;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

public class FtpRemoteFile : IVaultFile
{
    private readonly ManifestFile _manifestFile;

    protected FtpRemoteFile(ManifestFile file, string name, IVaultFolder Parent)
    {
        _manifestFile = file;
        Name = name;
        this.Parent = Parent;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public IVaultFolder? Parent { get; }
    public IVault Vault { get; private set; }

    public string ContentType { get; }

    public async ValueTask<byte[]> ReadBytesAsync()
    {
        using var client = ((FtpRemoteVault)Vault).AsyncFtpClient;
        try
        {
            await client.Connect();
        }
        catch (FtpException ex)
        {
            await Console.Error.WriteLineAsync(ex.StackTrace);
        }
        var bytes = await client.DownloadBytes(_manifestFile.Path, default);
        await client.Disconnect();
        return bytes;
    }

    public async ValueTask<Stream> OpenReadAsync()
    {
        var client = ((FtpRemoteVault)Vault).AsyncFtpClient;
        await client.Connect();
        var stream = new MemoryStream();
        await client.DownloadStream(stream, _manifestFile.Path);
        await client.Disconnect();
        return stream;
    }

    internal static FtpRemoteFile FromManifest(ManifestFile file, string name, IVaultFolder parent) =>
        file.ContentType?.StartsWith("text/markdown", StringComparison.OrdinalIgnoreCase)
        ?? name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? new FtpRemoteNote(file, name, parent)
            : new FtpRemoteFile(file, name, parent);
}