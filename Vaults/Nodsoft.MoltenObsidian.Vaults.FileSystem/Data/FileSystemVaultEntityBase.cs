using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

/// <summary>
/// Represents a file system vault entity, either a file or a directory.
/// </summary>
internal abstract class FileSystemVaultEntityBase : IVaultEntity
{
	private protected FileSystemVaultEntityBase(FileSystemInfo entity, IVaultFolder? parent, IVault vault)
	{
		Name = entity.Name;
		Parent = parent;
		Path = Parent is null ? "" : System.IO.Path.Combine(Parent.Path, Name).Replace('\\', '/');
		Vault = vault;
	}
	
	public string Name { get; set; }
	
	public IVaultFolder? Parent { get; }
	
	// This is the path relative to the vault root.
	public string Path { get; }
	
	public IVault Vault { get; }
}