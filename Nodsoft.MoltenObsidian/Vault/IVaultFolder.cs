namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a folder, within an Obsidian vault.
/// </summary>
/// <remarks>
/// This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault" />
[PublicAPI]
public interface IVaultFolder : IVaultEntity
{
	/// <summary>
	/// The immediate child folders of this folder.
	/// </summary>
	IReadOnlyList<IVaultFolder> Subfolders { get; }

	/// <summary>
	/// The files inside this folder, excluding subfolders.
	/// </summary>
	IReadOnlyList<IVaultFile> Files { get; }
}