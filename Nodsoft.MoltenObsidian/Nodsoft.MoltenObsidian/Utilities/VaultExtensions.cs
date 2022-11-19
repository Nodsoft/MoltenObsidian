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
	/// Gets all folders in this folder, and all subfolders if <paramref name="searchOption"/> is set to <see cref="SearchOption.AllDirectories"/>.
	/// </summary>
	/// <param name="searchOption">The search option to use.</param>
	/// <returns>A dictionary of all corresponding folders, keyed by their vault-relative path.</returns>
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
}