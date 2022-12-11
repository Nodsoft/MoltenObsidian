namespace Nodsoft.MoltenObsidian.Manifest;

/// <summary>
/// Represents a vault manifest, used to list the contents of a Molten Obsidian vault stored remotely.
/// </summary>
public class RemoteVaultManifest
{
	public const string ManifestFileName = "moltenobsidian.manifest.json";
	
	/// <summary>
	/// The name of the vault.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// The relative paths of the files contained in the vault.
	/// </summary>
	public IReadOnlyList<ManifestFile> Files { get; init; }
}