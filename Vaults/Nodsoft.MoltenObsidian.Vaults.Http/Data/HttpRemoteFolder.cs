using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Http.Data;

/// <summary>
/// Represents a folder stored in a remote Molten Obsidian vault, accessible via HTTP.
/// </summary>
public sealed class HttpRemoteFolder : IVaultFolder
{
	private HttpRemoteFolder() { }

	internal static HttpRemoteFolder FromRoot(string name, IVault vault) => new()
	{
		Name = name,
		Path = "",
		Vault = vault
	};

	/// <summary>
	/// Initializes a new instance of the <see cref="HttpRemoteFolder"/> class.
	/// </summary>
	/// <param name="part">The part of the path to represent.</param>
	/// <param name="currentFolder">The parent folder of the folder.</param>
	public HttpRemoteFolder(string part, IVaultFolder currentFolder)
	{
		Name = part.Split('/').Last();
		Path = part.Replace('\\', '/');
		Vault = currentFolder.Vault;
	}

	/// <inheritdoc />
	public string Name { get; set; }

	/// <inheritdoc />
	public string Path { get; set; }

	/// <inheritdoc />
	public IVaultFolder? Parent { get; private init; }

	/// <inheritdoc />
	public IVault Vault { get; private init; }

	/// <inheritdoc />
	public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;
	private readonly List<HttpRemoteFolder> _subfolders = [];

	/// <inheritdoc />
	public IReadOnlyList<IVaultFile> Files => _files;
	private readonly List<HttpRemoteFile> _files = [];
	
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