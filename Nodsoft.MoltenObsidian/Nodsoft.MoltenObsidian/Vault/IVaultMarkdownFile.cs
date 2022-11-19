namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a Markdown file, within an obsidian vault.
/// Once read, this file yields a <see cref="ObsidianText"/> instance.
/// </summary>
/// <remarks>
///	This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault"/>
/// <seealso cref="IVaultFile"/>
[PublicAPI]
public interface IVaultMarkdownFile : IVaultFile
{
	/// <summary>
	/// Reads the file as an ObsidianText instance.
	/// </summary>
	/// <returns>An ObsidianText instance.</returns>
	/// <exception cref="IOException">Thrown if the file cannot be read.</exception>
	/// <exception cref="InvalidDataException">Thrown if the file is not a valid Markdown file.</exception>
	/// <exception cref="UnauthorizedAccessException">Thrown if the file cannot be accessed.</exception>
	ObsidianText ReadDocument();
}