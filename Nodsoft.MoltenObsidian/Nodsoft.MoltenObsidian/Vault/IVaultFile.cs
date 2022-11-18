namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Represents a generic file, within an obsidian vault.
/// </summary>
/// <remarks>
///	This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IReadOnlyVault"/>
[PublicAPI]
public interface IVaultFile
{
	/// <summary>
	/// The name of the file.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The full path of the file, relative to the vault root.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// The folder that contains this file.
	/// </summary>
	IVaultFolder Parent { get; }
	
	/// <summary>
	/// Reads the contents of the file, as a buffer.
	/// </summary>
	/// <returns>A buffer containing the file contents.</returns>
	/// <exception cref="IOException">An error occurred while reading the file.</exception>
	/// <exception cref="UnauthorizedAccessException">The file could not be accessed.</exception>
	byte[] ReadAllBytes();
	
	/// <summary>
	/// Opens the file for reading, as a stream.
	/// </summary>
	/// <returns>A stream containing the file contents.</returns>
	/// <exception cref="IOException">An error occurred while opening a stream to the file.</exception>
	/// <exception cref="UnauthorizedAccessException">The file could not be accessed.</exception>
	Stream OpenRead();
}