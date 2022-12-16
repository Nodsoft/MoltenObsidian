namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a read-only Obsidian vault, which is a collection of folders and Markdown files.
/// </summary>
/// <remarks>
///	This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
[PublicAPI]
public interface IVault
{
	/// <summary>
	/// The name of the vault.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The root folder of the vault.
	/// </summary>
	IVaultFolder Root { get; }

	/// <summary>
	/// Gets the folder with the specified path.
	/// </summary>
	/// <param name="path">The path of the folder to retrieve.</param>
	/// <returns>The folder with the specified path.</returns>
	IVaultFolder? GetFolder(string? path) => path is null or "" ? Root : Folders.TryGetValue(path, out IVaultFolder? folder) ? folder : null;

	/// <summary>
	/// Gets the file with the specified path, or null if no such file exists.
	/// </summary>
	/// <param name="path">The path of the file to retrieve.</param>
	/// <returns>The file with the specified path.</returns>
	IVaultFile? GetFile(string path) => Files.TryGetValue(path, out IVaultFile? file) ? file : null;

	/// <summary>
	/// A dictionary of all files in the vault, keyed by their relative paths.
	/// </summary>
	/// <remarks>
	/// The paths are relative to the vault root, and do not include the vault root's name.
	/// </remarks>
	IReadOnlyDictionary<string, IVaultFile> Files { get; }
	
	/// <summary>
	/// A dictionary of all folders in the vault, keyed by their relative paths.
	/// </summary>
	/// <remarks>
	/// The paths are relative to the vault root, and do not include the vault root's name.
	/// </remarks>
	IReadOnlyDictionary<string, IVaultFolder> Folders { get; }
	
	/// <summary>
	/// A dictionary of all markdown files (called notes) in the vault, keyed by their relative paths.
	/// </summary>
	/// <remarks>
	/// The paths are relative to the vault root, and do not include the vault root's name.
	/// </remarks>
	IReadOnlyDictionary<string, IVaultNote> Notes { get; }
}