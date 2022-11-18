namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Represents a folder, within an Obsidian vault.
/// </summary>
/// <remarks>
/// This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IReadOnlyVault" />
[PublicAPI]
public interface IVaultFolder
{
	/// <summary>
	/// Gets the name of the folder.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the path of the folder.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the parent folder of the folder.
	/// </summary>
	IVaultFolder? Parent { get; }

	/// <summary>
	/// Gets the subfolders of the folder.
	/// </summary>
	IReadOnlyList<IVaultFolder> Subfolders { get; }

	/// <summary>
	/// Gets the files in the folder.
	/// </summary>
	IReadOnlyList<IVaultFile> Files { get; }

	/// <summary>
	/// Gets the vault that the folder belongs to.
	/// </summary>
	IReadOnlyVault Vault { get; }
}