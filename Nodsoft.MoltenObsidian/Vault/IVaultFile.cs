namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a generic file representation, within an obsidian vault.
/// </summary>
/// <remarks>
///	This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault"/>
[PublicAPI]
public interface IVaultFile : IVaultEntity
{
	/// <summary>
	/// The MIME type of the file's contents.
	/// </summary>
	/// <remarks>
	/// This is used to determine how to handle the file when it is opened.
	/// </remarks>
	string ContentType { get; }
	
	/// <summary>
	/// Opens the file for reading, as a stream.
	/// </summary>
	/// <returns>A stream containing the file contents.</returns>
	/// <exception cref="IOException">An error occurred while opening a stream to the file.</exception>
	/// <exception cref="UnauthorizedAccessException">The file could not be accessed.</exception>
	[MustUseReturnValue]
	ValueTask<Stream> OpenReadAsync();
}