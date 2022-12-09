namespace Nodsoft.MoltenObsidian.Vaults.Http.Common;

/// <summary>
/// Represents a vault manifest, used to list the contents of a Molten Obsidian vault stored remotely.
/// </summary>
public class RemoteVaultManifest
{
	/// <summary>
	/// The name of the vault.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// The relative paths of the files contained in the vault.
	/// </summary>
	public IReadOnlyList<ManifestFile> Files { get; init; }
}