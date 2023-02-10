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
	
	public IVaultFolder? GetFolder(string? path) => path is null or "" ? Root : Folders.TryGetValue(path, out IVaultFolder? folder) ? folder : null;

	public IVaultFile? GetFile(string path)
		=> Files.TryGetValue(Path.HasExtension(path) ? path : Path.ChangeExtension(path, ".md"), out IVaultFile? file)
			? file
			: null;

	public IReadOnlyDictionary<string, IVaultFile> Files { get; private set; } = null!;
	public IReadOnlyDictionary<string, IVaultFolder> Folders { get; private set; } = null!;
	public IReadOnlyDictionary<string, IVaultNote> Notes { get; private set; } = null!;

	
	public static IEnumerable<string> DefaultIgnoredFolders { get; } = new[] { ".obsidian", ".git", ".vs", ".vscode", "node_modules" };
	public static IEnumerable<string> DefaultIgnoredFiles { get; } = new[] { ".DS_Store" };

	/// <summary>
	/// Creates a new <see cref="FileSystemVault"/> instance from the specified path.
	/// </summary>
	/// <param name="directory">The path to the vault directory.</param>
	/// <returns>A new <see cref="FileSystemVault"/> instance.</returns>
	/// <exception cref="DirectoryNotFoundException">The specified directory does not exist.</exception>
	/// <exception cref="IOException">An I/O error occurred while reading the vault directory.</exception>
	/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission to access the vault directory.</exception>
	public static FileSystemVault FromDirectory(DirectoryInfo directoryInfo) => FromDirectory(directoryInfo, DefaultIgnoredFolders, DefaultIgnoredFiles);

	/// <summary>
	/// Creates a new <see cref="FileSystemVault"/> instance from the specified path.
	/// </summary>
	/// <param name="directory">The path to the vault directory.</param>
	/// <param name="excludedFolders">A list of folders to exclude from the vault.</param>
	/// <param name="excludedFiles">A list of files to exclude from the vault.</param>
	/// <returns>A new <see cref="FileSystemVault"/> instance.</returns>
	/// <exception cref="DirectoryNotFoundException">The specified directory does not exist.</exception>
	/// <exception cref="IOException">An I/O error occurred while reading the vault directory.</exception>
	/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission to access the vault directory.</exception>
	[PublicAPI]
	public static FileSystemVault FromDirectory(DirectoryInfo directory, 
		IEnumerable<string> excludedFolders,
		IEnumerable<string> excludedFiles
	) {
		// First make sure the directory exists
		if (!directory.Exists)
		{
			throw new DirectoryNotFoundException("The specified directory does not exist.");
		}
		
		FileSystemVault vault = new();
		vault.Root = new FileSystemVaultFolder(directory, null, vault);
		vault.Name = directory.Name;
		
		// Now we need to load all the files and folders
		vault.Folders = new Dictionary<string, IVaultFolder>(
			vault.Root.GetFolders(SearchOption.AllDirectories)
				// Take into account any exclusions
				.Where(folder =>
					{
						string[] segments = folder.Key.Split("/");
						return !segments.Any(excludedFolders.Contains);
					}
				));
		
		// Load the files from the above folders
		vault.Files = new Dictionary<string, IVaultFile>(
			vault.Folders.Values.Concat(new[] { vault.Root })
				.SelectMany(f => f.GetFiles(SearchOption.TopDirectoryOnly))
				// Take into account any exclusions
				.Where(f =>
					{
						string lastSegment = f.Key.Split("/").Last();
						return !excludedFiles.Contains(lastSegment);
					}
				));

		// Now we need to get all the markdown files.
		// We can do this by filtering the Files dictionary, grabbing all the files that end with ".md".
		vault.Notes = vault.Files.Where(static x => x.Key.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
			.ToDictionary(static x => x.Key, static x => (IVaultNote)x.Value);
		
		return vault;
	}
}