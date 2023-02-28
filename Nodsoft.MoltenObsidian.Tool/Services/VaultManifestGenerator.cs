using System.Security.Cryptography;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Manifest;
using MimeTypes = Nodsoft.MoltenObsidian.Manifest.MimeTypes;

namespace Nodsoft.MoltenObsidian.Tool.Services;

/// <summary>
/// Provides manifest generation for a filesystem-based Molten Obsidian vault.
/// </summary>
public static class VaultManifestGenerator
{
	/// <summary>
	/// Generates a manifest for the specified filesystem vault.
	/// </summary>
	/// <param name="vault">The path to the vault.</param>
	/// <returns>A <see cref="RemoteVaultManifest"/> instance.</returns>
	public static async Task<RemoteVaultManifest> GenerateManifestAsync(FileSystemVault vault)
	{
		List<ManifestFile> files = new();

		// Grab all the files in the vault
		foreach ((_, IVaultFile file) in vault.Files)
		{
			if (file.Name is not RemoteVaultManifest.ManifestFileName)
			{
				files.Add(await ExtractManifestAsync(file));
			}
		}

		// Build and return the manifest
		return new()
		{
			Name = vault.Name,
			Files = files.ToArray(),
		};
	}

	/// <summary>
	/// Extracts manifest information from a vault file.
	/// </summary>
	/// <param name="file">The file to extract information from.</param>
	/// <returns>A <see cref="ManifestFile"/> instance.</returns>
	private static async Task<ManifestFile> ExtractManifestAsync(IVaultFile file)
	{
		// Read the file. We'll need it later.
		await using Stream stream = await file.OpenReadAsync();

		// Build a new file object
		return new()
		{
			Path = file.Path,
			Hash = Convert.ToBase64String(await SHA256.HashDataAsync(stream)), // Create a SHA256 hash of the file
			Size = stream.Length,
			ContentType = MimeTypes.GetMimeType(file.Name),
			Metadata = file is IVaultNote note ? (await note.ReadDocumentAsync()).Frontmatter : new()
		};
	}
}