using System.Text;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http.Data;

/// <summary>
/// Represents a note stored in a remote Molten Obsidian vault, accessible via HTTP.
/// </summary>
public sealed class HttpRemoteNote : HttpRemoteFile, IVaultNote
{
	internal HttpRemoteNote(ManifestFile file, string name, IVaultFolder parent) : base(file, name, parent) { }
	
	/// <summary>
	/// Reads the document content of the note.
	/// </summary>
	/// <returns>A task containing the note's content.</returns>
	public async ValueTask<ObsidianText> ReadDocumentAsync() => new(Encoding.UTF8.GetString(await ReadBytesAsync()), this);
}