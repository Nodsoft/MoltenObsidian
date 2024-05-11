using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http.Data;

/// <summary>
/// Represents a file stored in a remote Molten Obsidian vault, accessible via HTTP.
/// </summary>
public class HttpRemoteFile : IVaultFile
{
	private readonly ManifestFile _manifestFile;

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpRemoteFile"/> class.
	/// </summary>
	/// <param name="file">The manifest file to represent.</param>
	/// <param name="name">The name of the file.</param>
	/// <param name="parent">The parent folder of the file.</param>
	protected HttpRemoteFile(ManifestFile file, string name, IVaultFolder parent)
	{
		_manifestFile = file;
		
		Name = name;
		Parent = parent;
	}

	internal static HttpRemoteFile FromManifest(ManifestFile file, string name, IVaultFolder parent) 
		=> file.ContentType?.StartsWith("text/markdown", StringComparison.OrdinalIgnoreCase)
		?? name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
			? new HttpRemoteNote(file, name, parent) 
			: new HttpRemoteFile(file, name, parent);

	/// <inheritdoc />
	public string Name { get; }

	/// <inheritdoc />
	public string Path => _manifestFile.Path;

	/// <inheritdoc />
	public string ContentType => _manifestFile.ContentType ?? "application/octet-stream";

	/// <inheritdoc />
	public async ValueTask<Stream> OpenReadAsync()
	{
		HttpClient httpClient = ((HttpRemoteVault)Vault).HttpClient;
		using HttpResponseMessage response = await httpClient.GetAsync(_manifestFile.Path);
		return await response.Content.ReadAsStreamAsync();
	}

	/// <inheritdoc />
	public IVaultFolder Parent { get; }

	/// <inheritdoc />
	public IVault Vault => Parent.Vault;
	
	
}