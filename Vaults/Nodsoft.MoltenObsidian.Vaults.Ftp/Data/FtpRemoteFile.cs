using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

/// <summary>
/// Represents a file in a remote FTP vault.
/// </summary>
public class FtpRemoteFile : IVaultFile
{
    private readonly ManifestFile _manifestFile;

    
    /// <summary>
    /// Initializes a new instance of the <see cref="FtpRemoteFile"/> class.
    /// </summary>
    /// <param name="file">The manifest file this remote file represents.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="parent">The parent folder of the file.</param>
    protected FtpRemoteFile(ManifestFile file, string name, IVaultFolder parent)
    {
        _manifestFile = file;
        Name = name;
        Parent = parent;
        Vault = parent.Vault;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Path => _manifestFile.Path;

    /// <inheritdoc />
    public IVaultFolder? Parent { get; }

    /// <inheritdoc />
    public IVault Vault { get; }

    /// <inheritdoc />
    public string ContentType { get; }

    /// <inheritdoc />
    public async ValueTask<byte[]> ReadBytesAsync()
    {
        var client = await ((FtpRemoteVault)Vault).AsyncFtpClient.EnsureConnected();
        var bytes = await client.DownloadBytes(_manifestFile.Path, default);
        return bytes;
    }

    /// <inheritdoc />
    public async ValueTask<Stream> OpenReadAsync()
    {
        var client = await ((FtpRemoteVault)Vault).AsyncFtpClient.EnsureConnected();
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