using System.Text;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

/// <summary>
/// Represents a note in a remote FTP vault.
/// </summary>
/// <seealso cref="FtpRemoteFile" />
public sealed class FtpRemoteNote : FtpRemoteFile, IVaultNote
{
    internal FtpRemoteNote(ManifestFile file, string name, IVaultFolder parent) : base(file, name, parent) { }
}