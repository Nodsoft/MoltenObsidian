using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http;

public class HttpRemoteFile : IVaultFile
{
	private readonly ManifestFile _manifestFile;

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

	public string Name { get; }
	public string Path => _manifestFile.Path;
	public string ContentType => _manifestFile.ContentType ?? "application/octet-stream";
	
	public async ValueTask<byte[]> ReadBytesAsync()
	{
		HttpClient httpClient = ((HttpRemoteVault)Vault).HttpClient;
		using HttpResponseMessage response = await httpClient.GetAsync(_manifestFile.Path);
		return await response.Content.ReadAsByteArrayAsync();
		
	}

	public async ValueTask<Stream> OpenReadAsync()
	{
		HttpClient httpClient = ((HttpRemoteVault)Vault).HttpClient;
		using HttpResponseMessage response = await httpClient.GetAsync(_manifestFile.Path);
		return await response.Content.ReadAsStreamAsync();
	}

	public IVaultFolder Parent { get; }
	
	public IVault Vault => Parent.Vault;
	
	
}