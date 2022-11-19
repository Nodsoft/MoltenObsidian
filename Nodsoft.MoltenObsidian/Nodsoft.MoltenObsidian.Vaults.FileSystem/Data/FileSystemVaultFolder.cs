using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

/// <summary>
/// Provides a file system based implementation of the <see cref="IVaultFolder"/> interface.
/// </summary>
internal sealed class FileSystemVaultFolder : FileSystemVaultEntityBase, IVaultFolder
{
	private readonly FileSystemVaultFolder[] _subfolders;
	private readonly FileSystemVaultFile[] _files;

	public FileSystemVaultFolder(DirectoryInfo entity, IVaultFolder? parent, IVault vault) : base(entity, parent)
	{
		Vault = vault;

		_subfolders = entity.GetDirectories().Select(d => new FileSystemVaultFolder(d, this, vault)).ToArray();
		_files = entity.GetFiles().Select(f => FileSystemVaultFile.Create(f, this)).ToArray();
	}

	public IVault Vault { get; }

	public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;

	public IReadOnlyList<IVaultFile> Files => _files;
}