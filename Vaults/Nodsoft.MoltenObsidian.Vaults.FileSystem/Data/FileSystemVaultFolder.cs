using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

/// <summary>
/// Provides a file system based implementation of the <see cref="IVaultFolder"/> interface.
/// </summary>
internal sealed class FileSystemVaultFolder : FileSystemVaultEntityBase, IVaultFolder
{
	/// <summary>
	/// Gets the <see cref="DirectoryInfo"/> instance that represents the physical directory on disk.
	/// </summary>
	internal DirectoryInfo PhysicalDirectoryInfo { get; }
	
	private readonly List<FileSystemVaultFolder> _subfolders;
	private readonly List<FileSystemVaultFile> _files;

	public FileSystemVaultFolder(DirectoryInfo entity, IVaultFolder? parent, IVault vault) : base(entity, parent, vault)
	{
		PhysicalDirectoryInfo = entity;
		_subfolders = entity.GetDirectories().Select(d => new FileSystemVaultFolder(d, this, vault)).ToList();
		_files = entity.GetFiles().Select(f => FileSystemVaultFile.Create(f, this, vault)).ToList();
	}
	
	/// <summary>
	/// Creates a new <see cref="FileSystemVaultFolder"/> instance, and the physical directory on disk.
	/// </summary>
	/// <remarks>
	/// The expected path should be relative to the vault root.
	/// </remarks>
	/// <param name="path">The path of the folder to create.</param>
	/// <param name="parent">The parent folder of the folder to create.</param>
	/// <param name="vault">The vault that the folder belongs to.</param>
	/// <returns>The newly created <see cref="FileSystemVaultFolder"/> instance.</returns>
	internal static FileSystemVaultFolder CreateFolder(string path, IVaultFolder? parent, FileSystemVault vault)
	{
		DirectoryInfo entity = Directory.CreateDirectory(System.IO.Path.Combine(((FileSystemVaultFolder)vault.Root).PhysicalDirectoryInfo.FullName, path));
		return new(entity, parent, vault);
	}

	public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;

	public IReadOnlyList<IVaultFile> Files => _files;
}