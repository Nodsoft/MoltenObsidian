using FluentFTP;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

public sealed class FtpRemoteVault : IVault
{
    internal RemoteVaultManifest? Manifest { get; private init; }
    
    internal AsyncFtpClient AsyncFtpClient { get; private init; }
    
    private FtpRemoteVault() {}
    
    public string Name { get; set; }
    
    public IVaultFolder Root => _root;
    private FtpRemoteFolder _root;

    public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;
    private readonly Dictionary<string, IVaultFolder> _folders;
    public IReadOnlyDictionary<string, IVaultFile> Files => _files;
    private readonly Dictionary<string,IVaultFile> _files;
    
    public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;
    private readonly Dictionary<string, IVaultNote> _notes;
    
    public static IVault FromManifest(RemoteVaultManifest? manifest, AsyncFtpClient ftpClient)
    {
        FtpRemoteVault vault = new()
        {
            Manifest = manifest,
            AsyncFtpClient = ftpClient
        };

        vault._root = FtpRemoteFolder.FromRoot(manifest.Name, vault);

        foreach (ManifestFile manifestFile in manifest.Files)
        {
            if (manifestFile.Path.Split('/') is not [.. var folderParts, var fileName]) continue;
            
            IVaultFolder? currentFolder = vault._root;
            IVaultFolder? parentFolder = vault._root;

            for (int i = 0; i < folderParts.Length; i++)
            {
                string pathPart = string.Join('/', folderParts.Take(i + 1));

                if (!vault._folders.TryGetValue(pathPart, out currentFolder))
                {
                    currentFolder = new FtpRemoteFolder(pathPart, parentFolder);
                    ((FtpRemoteFolder)parentFolder).AddSubFolder((FtpRemoteFolder)currentFolder);
                    vault._folders.Add(pathPart, currentFolder);
                }

                parentFolder = currentFolder;
            }
            
            FtpRemoteFile file = FtpRemoteFile.FromManifest(manifestFile, fileName, currentFolder);
            ((FtpRemoteFolder)currentFolder).AddFile(file);
            vault._files.Add(manifestFile.Path, file);
        }

        return vault;
    }
}