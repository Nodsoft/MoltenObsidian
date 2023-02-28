using System.Text;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

internal class FtpRemoteNote : FtpRemoteFile, IVaultNote
{
    internal FtpRemoteNote(ManifestFile file, string name, IVaultFolder parent) : base(file, name, parent)
    {
    }

    public async ValueTask<ObsidianText> ReadDocumentAsync()
    {
        return new ObsidianText(Encoding.UTF8.GetString(await ReadBytesAsync()), this);
    }
}