using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

/// <summary>
/// Provides a file system based implementation of the <see cref="IVaultFile"/> interface.
/// </summary>
internal class FileSystemVaultFile : FileSystemVaultEntityBase, IVaultFile
{
	protected FileSystemVaultFile(FileInfo file, IVaultFolder parent, IVault vault) : base(file, parent, vault)
	{
		ContentType = MimeTypes.GetMimeType(fileName: file.Name);
	}

	public virtual string ContentType { get; }

	public async ValueTask<byte[]> ReadBytesAsync()
	{
		// Grab the full path to the file. Use the vault's root path as the base.
		string fullPath = FullPath;
		
		// Read the file's contents.
		return await File.ReadAllBytesAsync(fullPath);
	}

	private string FullPath => System.IO.Path.Join(((FileSystemVaultFolder)Vault.Root).PhysicalDirectoryInfo.FullName, Path);

	public ValueTask<Stream> OpenReadAsync() 
		=> ValueTask.FromResult<Stream>(File.OpenRead(FullPath));
	
	public static FileSystemVaultFile Create(FileInfo file, IVaultFolder parent, IVault vault) 
		=> file.Extension is ".md"
			? new FileSystemVaultNote(file, parent, vault) 
			: new FileSystemVaultFile(file, parent, vault);
}