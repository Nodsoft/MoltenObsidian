using System.Text;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http.Data;

internal sealed class HttpRemoteNote : HttpRemoteFile, IVaultNote
{
	internal HttpRemoteNote(ManifestFile file, string name, IVaultFolder parent) : base(file, name, parent) { }
	
	public async ValueTask<ObsidianText> ReadDocumentAsync() => new(Encoding.UTF8.GetString(await ReadBytesAsync()), this);
}