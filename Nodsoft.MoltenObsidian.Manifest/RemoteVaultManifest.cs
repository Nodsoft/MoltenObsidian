namespace Nodsoft.MoltenObsidian.Manifest;

/// <summary>
/// Represents a vault manifest, used to list the contents of a Molten Obsidian vault stored remotely.
/// </summary>
public class RemoteVaultManifest
{
	/// <summary>
	/// Default name for a MoltenObsidian vault manifest file.
	/// </summary>
	public const string ManifestFileName = "moltenobsidian.manifest.json";
	
	/// <summary>
	/// The name of the vault.
	/// </summary>
	public string Name { get; init; } = string.Empty;

	/// <summary>
	/// The relative paths of the files contained in the vault.
	/// </summary>
	public IReadOnlyList<ManifestFile> Files { get; init; } = [];
}