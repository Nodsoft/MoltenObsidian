using FluentFTP;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

public sealed class FtpRemoteVault : IVault
{
    private readonly Dictionary<string, IVaultFile> _files;
    private readonly Dictionary<string, IVaultFolder> _folders;
    private readonly Dictionary<string, IVaultNote> _notes;
    private FtpRemoteFolder _root;

    private FtpRemoteVault()
    {
    }

    internal RemoteVaultManifest? Manifest { get; private init; }

    internal AsyncFtpClient AsyncFtpClient { get; private init; }

    public string Name { get; set; }

    public IVaultFolder Root => _root;

    public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;
    public IReadOnlyDictionary<string, IVaultFile> Files => _files;

    public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;

    public static IVault FromManifest(RemoteVaultManifest? manifest, AsyncFtpClient ftpClient)
    {
        FtpRemoteVault vault = new()
        {
            Manifest = manifest,
            AsyncFtpClient = ftpClient
        };

        vault._root = FtpRemoteFolder.FromRoot(manifest.Name, vault);

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
            vault._files.Add(manifestFile.Path, file);
        }

        return vault;
    }
}