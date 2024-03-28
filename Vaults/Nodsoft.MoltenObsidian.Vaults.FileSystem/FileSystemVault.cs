using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Utilities;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem;

/// <summary>
/// Provides a filesystem based implementation of the <see cref="IVault"/> interface.
/// </summary>
public sealed class FileSystemVault : IWritableVault
{
	private readonly Dictionary<string, IVaultFile> _files;
	private readonly Dictionary<string, IVaultFolder> _folders;
	private readonly Dictionary<string, IVaultNote> _notes;


	/// <inheritdoc />
	public string Name { get; }

	/// <inheritdoc />
	public IVaultFolder Root { get; }

	/// <inheritdoc />
	public IVaultFolder? GetFolder(string? path) => path is null or "" ? Root : Folders.GetValueOrDefault(path);

	/// <inheritdoc />
	public IVaultFile? GetFile(string path) => Files.GetValueOrDefault(Path.HasExtension(path) ? path : Path.ChangeExtension(path, ".md"));

	public IReadOnlyDictionary<string, IVaultFile> Files => _files;

	public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;

	public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;


	public static IEnumerable<string> DefaultIgnoredFolders { get; } = new[] { ".obsidian", ".git", ".vs", ".vscode", "node_modules" };
	public static IEnumerable<string> DefaultIgnoredFiles { get; } = new[] { ".DS_Store" };

	/// <summary>
	/// Creates a new <see cref="FileSystemVault"/> instance from the specified path.
	/// </summary>
	/// <param name="directoryInfo">The path to the vault directory.</param>
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
	) => new(directory, excludedFolders, excludedFiles);

	private FileSystemVault(
		DirectoryInfo directory,
		IEnumerable<string> excludedFolders,
		IEnumerable<string> excludedFiles
	) {
		// First make sure the directory exists
		if (!directory.Exists)
		{
			throw new DirectoryNotFoundException("The specified directory does not exist.");
		}

		Name = directory.Name;
		Root = new FileSystemVaultFolder(directory, null, this);

		// Now we need to load all the files and folders
		_folders = new(Root
			.GetFolders(SearchOption.AllDirectories)
			// Take into account any exclusions
			.Where(folder =>
			{
				string[] segments = folder.Key.Split("/");
				return !segments.Any(excludedFolders.Contains);
			})
		);

		// Load the files from the above folders
		_files = new(_folders.Values.Concat(new[] { Root })
			.SelectMany(f => f.GetFiles(SearchOption.TopDirectoryOnly))
			// Take into account any exclusions
			.Where(f =>
			{
				string lastSegment = f.Key.Split("/").Last();
				return !excludedFiles.Contains(lastSegment);
			})
		);

		// Now we need to get all the markdown files.
		// We can do this by filtering the Files dictionary, grabbing all the files that end with ".md".
		_notes = Files.Where(static x => x.Key.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
			.ToDictionary(static x => x.Key, static x => (IVaultNote)x.Value);
	}

	/// <inheritdoc />
	public ValueTask<IVaultFolder> CreateFolderAsync(string path)
	{
		// Small starters : Strip the leading slash if present
		if (path.StartsWith('/'))
		{
			path = path[1..];
		}
		
		// First checks : Is this a valid path?
		if (path is null or "" or "/")
		{
			throw new ArgumentException("The specified path is invalid.", nameof(path));
		}

		// Second checks : Does the folder already exist?
		if (Folders.TryGetValue(path, out IVaultFolder? existing))
		{
			return ValueTask.FromResult(existing);
		}
		
		// Find the furthest folder that exists, and create the rest.
		
		// Slice the path into segments
		string[] segments = path.Split('/');
		
		// Find the furthest folder that exists
		int furthestDepth = 0;
		IVaultFolder furthestFolder = Root;
		
		for (int i = 0; i < segments.Length; i++)
		{
			string currentPath = string.Join('/', segments[..i]);
			
			// Check if the folder exists
			if (!Folders.TryGetValue(currentPath, out IVaultFolder? folder))
			{
				break;
			}

			furthestDepth = i;
			furthestFolder = folder;
		}
		
		// Now we need to create the rest of the folders
		for (int i = furthestDepth; i < segments.Length; i++)
		{
			string currentPath = string.Join('/', segments[..i]);
			
			// Create the folder
			furthestFolder = FileSystemVaultFolder.CreateFolder(currentPath, furthestFolder, this);
		}
		
		return new(furthestFolder);
	}

	/// <inheritdoc />
	public async ValueTask<IVaultFile> WriteFileAsync(string path, byte[] content)
	{
		// Small step: Strip the leading slash if present
		if (path.StartsWith('/'))
		{
			path = path[1..];
		}
		
		// First checks : Is this a valid path?
		if (path is null or "")
		{
			throw new ArgumentException("The specified path is invalid.", nameof(path));
		}
		
		// Does the parent folder exist? If not, create it.
		// Luckily, we can use the CreateFolderAsync method for this.
		IVaultFolder parent = path.Contains('/') ? await CreateFolderAsync(path[..path.LastIndexOf('/')]) : Root;
		
		// Write to file
		return await FileSystemVaultFile.WriteFileAsync(path, content, parent, this);
	}

	/// <inheritdoc />
	public async ValueTask<IVaultNote> WriteNoteAsync(string path, byte[] content)
	{
		// This is just a wrapper around CreateFileAsync with a check for the extension.
		if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("The specified path does not point to a Markdown file.", nameof(path));
		}
		
		return (IVaultNote)await WriteFileAsync(path, content);
	}

	/// <inheritdoc />
	public ValueTask DeleteFolderAsync(string path)
	{
		// Small step: Strip the leading slash if present
		if (path.StartsWith('/'))
		{
			path = path[1..];
		}
		
		// First checks : Is this a valid path?
		if (path is null or "")
		{
			throw new ArgumentException("The specified path is invalid.", nameof(path));
		}

		// Second checks : Does the folder exist?
		if (!Folders.TryGetValue(path, out IVaultFolder? folder))
		{
			throw new DirectoryNotFoundException("The specified directory does not exist.");
		}
		
		// Delete the folder
		((FileSystemVaultFolder)folder).DeleteFolder();
		return new();
	}
	
	/// <inheritdoc />
	public ValueTask DeleteFileAsync(string path)
	{
		// Small step: Strip the leading slash if present
		if (path.StartsWith('/'))
		{
			path = path[1..];
		}
		
		// First checks : Is this a valid path?
		if (path is null or "")
		{
			throw new ArgumentException("The specified path is invalid.", nameof(path));
		}

		// Second checks : Does the file exist?
		if (!Files.TryGetValue(path, out IVaultFile? file))
		{
			throw new FileNotFoundException("The specified file does not exist.");
		}
		
		// Delete the file
		((FileSystemVaultFile)file).DeleteFile();
		return new();
	}
	
	/// <inheritdoc />
	public ValueTask DeleteNoteAsync(string path)
	{
		// This is just a wrapper around DeleteFileAsync with a check for the extension.
		if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("The specified path does not point to a Markdown file.", nameof(path));
		}
		
		return DeleteFileAsync(path);
	}
}