using System.Text;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

internal sealed class FileSystemVaultNote : FileSystemVaultFile, IVaultNote
{
	public FileSystemVaultNote(FileInfo file, IVaultFolder parent, IVault vault) : base(file, parent, vault) { }
	
	public override string ContentType => "text/markdown";
	
	public async ValueTask<ObsidianText> ReadDocumentAsync() => new(Encoding.UTF8.GetString(await ReadBytesAsync()), this);
}