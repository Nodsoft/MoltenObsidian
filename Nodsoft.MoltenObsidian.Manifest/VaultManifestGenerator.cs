using System.Security.Cryptography;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Manifest;

/// <summary>
/// Provides manifest generation for a filesystem-based Molten Obsidian vault.
/// </summary>
public static class VaultManifestGenerator
{
	/// <summary>
	/// Generates a manifest for the specified vault.
	/// </summary>
	/// <param name="vault">The path to the vault.</param>
	/// <returns>A <see cref="RemoteVaultManifest"/> instance.</returns>
	public static async Task<RemoteVaultManifest> GenerateManifestAsync(IVault vault)
	{
		List<ManifestFile> files = new();

		// Grab all the files in the vault
		foreach ((_, IVaultFile file) in vault.Files)
		{
			if (file.Name is not RemoteVaultManifest.ManifestFileName)
			{
				files.Add(await ExtractManifestInfoAsync(file));
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
	private static async Task<ManifestFile> ExtractManifestInfoAsync(IVaultFile file)
	{
		// Read the file. We'll need it later.
		await using Stream stream = await file.OpenReadAsync();

#if !NET7_0_OR_GREATER
		byte[] fileBytes = new byte[stream.Length];
		int readBytesCount = await stream.ReadAsync(fileBytes);
		
		if (readBytesCount != stream.Length)
		{
			throw new InvalidOperationException("The file could not be read.");
		}
#endif

		// Build a new file object
		return new()
		{
			Path = file.Path,
			
#if NET7_0_OR_GREATER
			Hash = Convert.ToBase64String(await SHA256.HashDataAsync(stream)), // Create a SHA256 hash of the file 
#else
			Hash = Convert.ToBase64String(SHA256.HashData(fileBytes)), // Create a SHA256 hash of the file
#endif
			
			Size = stream.Length,
			ContentType = Manifest.MimeTypes.GetMimeType(file.Name), // A fuller namespace is required here because of a conflict with the other apparitions of MimeTypes.
			Metadata = file is IVaultNote note ? (await note.ReadDocumentAsync()).Frontmatter : new()
		};
	}
}