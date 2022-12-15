using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http;

public sealed class HttpRemoteFolder : IVaultFolder
{
	private HttpRemoteFolder() { }

	internal static HttpRemoteFolder FromRoot(string name, IVault vault) => new()
	{
		Name = name,
		Path = "",
		Vault = vault
	};

	public HttpRemoteFolder(string part, IVaultFolder currentFolder)
	{
		Name = part.Split('/').Last();
		Path = part.Replace('\\', '/');
		Vault = currentFolder.Vault;
	}

	public string Name { get; set; }
	public string Path { get; set; }
	public IVaultFolder? Parent { get; private init; }
	public IVault Vault { get; private init; }
	
	public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;
	private readonly List<HttpRemoteFolder> _subfolders = new(); 
	
	public IReadOnlyList<IVaultFile> Files => _files;
	private readonly List<HttpRemoteFile> _files = new();
	
	/// <summary>
	/// Adds a subfolder to this folder's list of subfolders.
	/// </summary>
	/// <param name="folder">The folder to add.</param>
	internal void AddSubfolder(HttpRemoteFolder folder) => _subfolders.Add(folder);

	/// <summary>
	/// Adds a file to this folder's list of files.
	/// </summary>
	/// <param name="file">The file to add.</param>
	internal void AddFile(HttpRemoteFile file) => _files.Add(file);
}