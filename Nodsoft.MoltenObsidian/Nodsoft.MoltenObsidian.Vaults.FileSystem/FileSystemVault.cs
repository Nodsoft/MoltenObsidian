using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Utilities;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem;

/// <summary>
/// Provides a filesystem based implementation of the <see cref="IVault"/> interface.
/// </summary>
public sealed class FileSystemVault : IVault
{
	private FileSystemVault() { }
	
	public string Name { get; private set; } = null!;
	public IVaultFolder Root { get; private set; } = null!;

	public IVaultFolder GetFolder(string path)
	{
		throw new NotImplementedException();
	}

	public IVaultFile? GetFile(string path)
	{
		throw new NotImplementedException();
	}

	public IReadOnlyDictionary<string, IVaultFile> Files { get; private set; } = null!;
	public IReadOnlyDictionary<string, IVaultFolder> Folders { get; private set; } = null!;
	public IReadOnlyDictionary<string, IVaultMarkdownFile> MarkdownFiles { get; private set; } = null!;

	/// <summary>
	/// Creates a new <see cref="FileSystemVault"/> instance from the specified path.
	/// </summary>
	/// <param name="directory">The path to the vault directory.</param>
	/// <returns>A new <see cref="FileSystemVault"/> instance.</returns>
	/// <exception cref="DirectoryNotFoundException">The specified directory does not exist.</exception>
	/// <exception cref="IOException">An I/O error occurred while reading the vault directory.</exception>
	/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission to access the vault directory.</exception>
	[PublicAPI]
	public static FileSystemVault FromDirectory(DirectoryInfo directory)
	{
		// First make sure the directory exists
		if (!directory.Exists)
		{
			throw new DirectoryNotFoundException("The specified directory does not exist.");
		}
		
		FileSystemVault vault = new();
		vault.Root = new FileSystemVaultFolder(directory, null, vault);
		vault.Name = directory.Name;
		
		// Now we need to load all the files and folders
		// The .obsidian directory is reserved for internal use, so we ignore it
		vault.Folders = new Dictionary<string, IVaultFolder>(
			vault.Root.GetFolders(SearchOption.AllDirectories).Where(static f => !f.Key.StartsWith(".obsidian", StringComparison.Ordinal))
		);
		
		// Load the files accordingly.
		vault.Files = new Dictionary<string, IVaultFile>(
			vault.Root.GetFiles(SearchOption.AllDirectories).Where(static f => !f.Key.StartsWith(".obsidian", StringComparison.Ordinal))
		);

			// Now we need to get all the markdown files.
		// We can do this by filtering the Files dictionary, grabbing all the files that end with ".md".
		vault.MarkdownFiles = vault.Files.Where(static x => x.Key.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
			.ToDictionary(static x => x.Key, static x => (IVaultMarkdownFile)x.Value);
		
		return vault;
	}
}