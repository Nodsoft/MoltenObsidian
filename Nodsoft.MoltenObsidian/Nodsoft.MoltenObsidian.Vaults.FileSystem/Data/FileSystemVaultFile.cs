using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

/// <summary>
/// Provides a file system based implementation of the <see cref="IVaultFile"/> interface.
/// </summary>
internal class FileSystemVaultFile : FileSystemVaultEntityBase, IVaultFile
{
	protected FileSystemVaultFile(FileInfo file, IVaultFolder parent) : base(file, parent)
	{
		ContentType = MimeTypes.GetMimeType(fileName: file.Name);
	}

	public virtual string ContentType { get; }

	public byte[] ReadAllBytes() => File.ReadAllBytes(Path);

	public Stream OpenRead() => File.OpenRead(Path);
	
	public static FileSystemVaultFile Create(FileInfo file, IVaultFolder parent) 
		=> file.Extension is ".md" 
			? new FileSystemVaultMarkdownFile(file, parent) 
			: new FileSystemVaultFile(file, parent);
}