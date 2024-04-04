using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Utilities;

/// <summary>
/// Verious extension methods for the <see cref="IVault"/> interface and its dependents.
/// </summary>
/// <seealso cref="IVault"/>
/// <seealso cref="IVaultFolder"/>
/// <seealso cref="IVaultFile"/>
public static class VaultExtensions
{
	/// <summary>
	/// Gets all files in this folder, and all subfolders if <paramref name="searchOption"/> is set to <see cref="SearchOption.AllDirectories"/>.
	/// </summary>
	/// <param name="folder">The folder to search.</param>
	/// <param name="searchOption">The search option to use.</param>
	/// <returns>A dictionary of all corresponding files, keyed by their vault-relative path.</returns>
	/// <seealso cref="IVaultFile" />
	public static IReadOnlyDictionary<string, IVaultFile> GetFiles(this IVaultFolder folder, SearchOption searchOption)
	{
		if (searchOption is SearchOption.TopDirectoryOnly)
		{
			return folder.Files.ToDictionary(static f => f.Path);
		}
		
		// All directories? Okay. Get ready for quite a bit of recursion.
		// Traverse the subfolders.
		Dictionary<string, IVaultFile> files = new();
		_FolderTraversal(folder, ref files);
		
		return files;
		
		
		// Local function to add files from a directory tree.
		static void _FolderTraversal(IVaultFolder folder, ref Dictionary<string, IVaultFile> files)
		{
			foreach (IVaultFile file in folder.Files)
			{
				files.TryAdd(file.Path, file);
			}
			
			foreach (IVaultFolder subfolder in folder.Subfolders)
			{
				_FolderTraversal(subfolder, ref files);
			}
		}
	}
	
	/// <summary>
	/// Gets all note files in this folder, and all subfolders if <paramref name="searchOption"/> is set to <see cref="SearchOption.AllDirectories"/>.
	/// </summary>
	/// <param name="folder">The folder to search.</param>
	/// <param name="searchOption">The search option to use.</param>
	/// <returns>A dictionary of all corresponding note files, keyed by their vault-relative path.</returns>
	/// <remarks>
	/// Note files are files with the extension <c>.md</c>.
	/// </remarks>
	/// <seealso cref="GetFiles(IVaultFolder, SearchOption)" />
	/// <seealso cref="IVaultNote" />
	public static IReadOnlyDictionary<string, IVaultNote> GetNotes(this IVaultFolder folder, SearchOption searchOption) 
		=> folder.GetFiles(searchOption)
			.Where(static f => f.Key.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
			.ToDictionary(static f => f.Key, static f => (IVaultNote)f.Value);

	/// <summary>
	/// Gets all folders in this folder, and all subfolders if <paramref name="searchOption"/> is set to <see cref="SearchOption.AllDirectories"/>.
	/// </summary>
	/// <param name="searchOption">The search option to use.</param>
	/// <returns>A dictionary of all corresponding folders, keyed by their vault-relative path.</returns>
	/// <seealso cref="IVaultFolder" />
	public static IReadOnlyDictionary<string, IVaultFolder> GetFolders(this IVaultFolder folder, SearchOption searchOption)
	{
		if (searchOption is SearchOption.TopDirectoryOnly)
		{
			return folder.Subfolders.ToDictionary(static f => f.Path);
		}
		
		// All directories? Okay. Get ready for quite a bit of recursion.
		// Traverse the subfolders.
		Dictionary<string, IVaultFolder> folders = new();
		_FolderTraversal(folder, ref folders);
		
		return folders;
		
		
		// Local function to add files from a directory tree.
		static void _FolderTraversal(IVaultFolder folder, ref Dictionary<string, IVaultFolder> folders)
		{
			foreach (IVaultFolder subfolder in folder.Subfolders)
			{
				folders.TryAdd(subfolder.Path, subfolder);
				_FolderTraversal(subfolder, ref folders);
			}
		}
	}

	/// <summary>
	/// Resolves a relative path's target file, relative to the current file.
	/// </summary>
	/// <param name="file">The file to resolve the path from.</param>
	/// <param name="relativePath">The relative path to resolve.</param>
	/// <returns>The resolved file (that could be a <see cref="IVaultNote" />), or <see langword="null"/> if the file could not be found.</returns>
	public static IVaultFile? ResolveRelativeLink(this IVaultNote file, string relativePath)
	{
		// First split the path into its components.
		string[] pathComponents = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
		
		// Is the path a single component? If so, it's a file in the same folder, or inferred path.
		if (pathComponents is [{ } fileName])
		{
			// First try to resolve in the file's current folder (if it has one, else it's the vault itself).
			IVaultFile? resolvedFile = _FromFolderFiles(file.Parent, fileName)
				// If that didn't work, try to resolve by traversing the vault, upstream first.
				?? _TraverseUpstream(file.Parent, fileName)
				// ...and if that didn't work, try to resolve by traversing the vault, downstream.
				?? _TraverseDownstream(file.Parent, fileName)
				// ...and how about downstream from vault root?
				?? _TraverseDownstream(file.Vault.Root, fileName);
			
			// If we found a file, return it.
			
			if (resolvedFile is not null)
			{
				return resolvedFile;
			}
		}
		
		/*
		 * TODO: Resolve relative links with folder definitions.
		 */

		// Last ditch attempt: Could it be a full path?
		// Check against the vault Files dictionary.
		else if (file.Vault.GetFile(relativePath) is { } resolvedFile)
		{
			return resolvedFile;
		}
		
		return null;
		
		static IVaultFile? _FromFolderFiles(IVaultFolder? folder, string fileName) 
			=> folder?.Files.FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
			?? folder?.Files.FirstOrDefault(f => f is IVaultNote { NoteName: var noteName } 
				&& noteName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
		
		static IVaultFile? _TraverseUpstream(IVaultFolder folder, string fileName)
		{
			// First try to resolve in the current folder.
			IVaultFile? resolvedFile = _FromFolderFiles(folder, fileName);

			resolvedFile = resolvedFile switch
			{
				// If that didn't work, try to resolve by traversing the vault, upstream first.
				null when folder.Parent is not null => _TraverseUpstream(folder.Parent, fileName),
				_ => resolvedFile
			};

			return resolvedFile;
		}
		
		static IVaultFile? _TraverseDownstream(IVaultFolder parent, string fileName)
		{
			foreach (IVaultFolder? folder in parent.Subfolders)
			{
				// Does that folder contain the file?
				if ((_FromFolderFiles(folder, fileName) ?? _TraverseDownstream(folder, fileName)) is { } resolvedFile)
				{
					return resolvedFile;
				}
				
				// If not, try the next folder.
			}
			
			// If we got here, the file wasn't found.
			return null;
		}
	}

	/// <summary>
	/// Resolves a relative path's furthest target folder, relative to the current folder.
	/// </summary>
	/// <param name="parent">The folder to resolve the path from.</param>
	/// <param name="path">The relative path to resolve.</param>
	/// <returns>The furthest resolved folder, or <see cref="parent"/> if no suitable folder could be found.</returns>
	public static IVaultFolder FindFurthestParent(this IVaultFolder parent, string path)
	{
		// Get the specified parent's relative path from the vault root.
		return _TraverseUpstream(Path.Join(parent.Path, path).Replace('\\', '/')) ?? parent;
		
		
		IVaultFolder? _TraverseUpstream(string? p)
		{
			if (p is null or "" or "/")
			{
				return null;
			}
			
			if (parent.Vault.Folders.TryGetValue(p, out IVaultFolder? folder))
			{
				return folder;
			}
			
			// Get the parent folder's path (chop off the last folder).
			if (p.Contains('/'))
			{
				_TraverseUpstream(p[..^p.LastIndexOf('/')]);
			}
		
			// And when all else fails...
			return null;
		}
	}
}