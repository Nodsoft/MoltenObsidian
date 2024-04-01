using FluentFTP;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

/// <summary>
/// Defines a remotely-accessible Molten Obisidan vault, via FTP.
/// </summary>
public sealed class FtpRemoteVault : IVault
{
    private readonly Dictionary<string, IVaultFile> _files = new();
    private readonly Dictionary<string, IVaultNote> _notes = new();
    private Dictionary<string, IVaultFolder> _folders = new();
    private FtpRemoteFolder _root;

    private FtpRemoteVault()
    {
    }

    internal RemoteVaultManifest? Manifest { get; private init; }

    internal AsyncFtpClient AsyncFtpClient { get; private init; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public IVaultFolder Root => _root;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultFile> Files => _files;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;

    /// <inheritdoc />
    /// <remarks>
    /// There is no change detection implemented within the FTP Vault (yet).
    /// </remarks>
    public event IVault.VaultUpdateEventHandler? VaultUpdate;

    /// <summary>
	/// Builds a new Vault from a remote FTP manifest.
	/// </summary>
	/// <param name="manifest">The manifest to build the vault from.</param>
	/// <param name="ftpClient">The FTP client to use for remote requests.</param>
	/// <returns>A new FTP remote vault.</returns>
	public static IVault FromManifest(RemoteVaultManifest manifest, AsyncFtpClient ftpClient)
    {
        FtpRemoteVault vault = new()
        {
            Manifest = manifest,
            AsyncFtpClient = ftpClient
        };

        vault._root = FtpRemoteFolder.FromRoot(manifest.Name, vault);
        vault._folders = new Dictionary<string, IVaultFolder>();
        foreach (var manifestFile in manifest.Files)
        {
            if (manifestFile.Path.Split('/') is not [.. var folderParts, var fileName]) continue;

            IVaultFolder? currentFolder = vault._root;
            IVaultFolder? parentFolder = vault._root;

            for (var i = 0; i < folderParts.Length; i++)
            {
                var pathPart = string.Join('/', folderParts.Take(i + 1));

                if (!vault._folders.TryGetValue(pathPart, out currentFolder))
                {
                    currentFolder = new FtpRemoteFolder(pathPart, parentFolder);
                    ((FtpRemoteFolder)parentFolder).AddSubFolder((FtpRemoteFolder)currentFolder);
                    vault._folders.Add(pathPart, currentFolder);
                }

                parentFolder = currentFolder;
            }

            var file = FtpRemoteFile.FromManifest(manifestFile, fileName, currentFolder);
            ((FtpRemoteFolder)currentFolder).AddFile(file);

            vault._files.Add(file.Path, file);

            if(file.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
				vault._notes.Add(file.Path, (IVaultNote)file);
            }
        }

        return vault;
    }
}