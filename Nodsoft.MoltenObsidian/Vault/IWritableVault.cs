namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies an editable Obisidian vault, which is a collection of folders and Markdown files.
/// </summary>
/// <remarks>
/// This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault"/>
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
	/// Creates a new file with the specified path.
	/// </summary>
	/// <remarks>
	/// If the file's path hits a missing folder, the folder will be created.
	/// </remarks>
	/// <param name="path">The path of the file to create.</param>
	/// <param name="content">The content of the file to create.</param>
	/// <returns>A task that holds the newly created file as its result.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="InvalidOperationException">Thrown if a file with the specified path already exists.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	ValueTask<IVaultFile> CreateFileAsync(string path, byte[] content);
	
	/// <summary>
	/// Creates a new note with the specified path.
	/// </summary>
	/// <remarks>
	/// If the note's path hits a missing folder, the folder will be created.
	/// </remarks>
	/// <param name="path">The path of the note to create.</param>
	/// <param name="content">The content of the note to create.</param>
	/// <returns>A task that holds the newly created note as its result.</returns>
	/// <exception cref="ArgumentException">Thrown if the specified path is invalid.</exception>
	/// <exception cref="InvalidOperationException">Thrown if a note with the specified path already exists.</exception>
	/// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
	/// <seealso cref="CreateFileAsync(string, byte[])"/>
	ValueTask<IVaultNote> CreateNoteAsync(string path, byte[] content);
}