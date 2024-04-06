namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies an editable Obisidian vault, which is a collection of folders and Markdown files.
/// </summary>
/// <remarks>
/// This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault"/>
[PublicAPI]
public interface IWritableVault : IVault
{
	/// <summary>
	/// Creates a new folder with the specified path.
	/// </summary>
	/// <param name="path">The path of the folder to create.</param>
	/// <returns>A task that holds the newly created folder as its result.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="InvalidOperationException">Thrown if a folder with the specified path already exists.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	ValueTask<IVaultFolder> CreateFolderAsync(string path);
	
	/// <summary>
	/// Deletes the folder with the specified path.
	/// </summary>
	/// <param name="path">The path of the folder to delete.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	ValueTask DeleteFolderAsync(string path);
	
	/// <summary>
	/// Writes the streamed content to a specified file, creating the file if it does not exist.
	/// </summary>
	/// <param name="path">The path of the file to write to.</param>
	/// <param name="content">The streamed content to write to the file.</param>
	/// <returns>A task that holds the file as its result.</returns>
	ValueTask<IVaultFile> WriteFileAsync(string path, Stream content);
	
	/// <summary>
	/// Deletes the file with the specified path.
	/// </summary>
	/// <param name="path">The path of the file to delete.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	ValueTask DeleteFileAsync(string path);
	
	/// <summary>
	/// Writes streamed content to a specified note, creating the note if it does not exist.
	/// </summary>
	/// <remarks>
	/// If the note already exists, it will be overwritten.
	/// If the note's path hits a missing folder, the folder will be created.
	/// </remarks>
	/// <param name="path">The path of the note to write to.</param>
	/// <param name="content">The streamed content to write to the note.</param>
	/// <returns>A task that holds the note as its result.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	/// <seealso cref="WriteFileAsync(string, Stream)"/>
	ValueTask<IVaultNote> WriteNoteAsync(string path, Stream content);

	/// <summary>
	/// Deletes the note with the specified path.
	/// </summary>
	/// <param name="path">The path of the note to delete.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	/// <seealso cref="DeleteFileAsync(string)"/>
	ValueTask DeleteNoteAsync(string path);
}