using System.Text.Json.Serialization;

namespace Nodsoft.MoltenObsidian.Manifest;

/// <summary>
/// Represents a vault file within a manifest.
/// </summary>
public readonly record struct ManifestFile
{
	/// <summary>
	/// The path of the file relative to the vault root.
	/// </summary>
	[JsonRequired] public string Path { get; init; }

	/// <summary>
	/// The size of the file in bytes.
	/// </summary>
	public long? Size { get; init; }

	/// <summary>
	/// The SHA256 hash of the file, in Base64.
	/// </summary>
	public string? Hash { get; init; }
	
	/// <summary>
	/// The content type of the file, in MIME format.
	/// </summary>
	public string? ContentType { get; init; }

	/// <summary>
	/// Additional metadata about the file, usually reflecting the file's properties or a note's YAML front matter.
	/// </summary>
	[JsonExtensionData] public IDictionary<string, object> Metadata { get; init; }
}