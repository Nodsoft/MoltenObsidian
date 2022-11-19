namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a folder, within an Obsidian vault.
/// </summary>
/// <remarks>
/// This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault" />
[PublicAPI]
public interface IVaultFolder
{
	/// <summary>
	/// The name of this folder.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The path of this folder, relative to the vault root.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// The parent folder of this folder.
	/// This will be <see langword="null" /> if this folder is the root folder.
	/// </summary>
	IVaultFolder? Parent { get; }

	/// <summary>
	/// The immediate child folders of this folder.
	/// </summary>
	IReadOnlyList<IVaultFolder> Subfolders { get; }

	/// <summary>
	/// The files inside this folder, excluding subfolders.
	/// </summary>
	IReadOnlyList<IVaultFile> Files { get; }

	/// <summary>
	/// The vault that the folder belongs to.
	/// </summary>
	IVault Vault { get; }
}