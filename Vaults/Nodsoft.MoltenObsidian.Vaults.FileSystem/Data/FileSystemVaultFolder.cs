﻿using Nodsoft.MoltenObsidian.Vault;

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
		FileSystemVaultFolder folder = new(entity, parent, vault);
		(parent as FileSystemVaultFolder)?.AddChildReference(folder);
		return folder;
	}

	/// <summary>
	/// Deletes a folder and its contents from the file system.
	/// </summary>
	/// <exception cref="DirectoryNotFoundException">The specified directory does not exist.</exception>
	/// <exception cref="IOException">An I/O error occurred while deleting the directory.</exception>
	internal void DeleteFolder()
	{
		PhysicalDirectoryInfo.Delete(true);
		(Parent as FileSystemVaultFolder)?.DeleteChildReference(this);
	}

	internal void AddChildReference(IVaultEntity child)
	{
		if (child.Parent != this)
		{
			throw new InvalidOperationException("The specified entity is not a child of this folder.");
		}

		if (child is IVaultFolder folder)
		{
			_subfolders.Add((FileSystemVaultFolder)folder);
		}
		else if (child is IVaultFile file)
		{
			_files.Add((FileSystemVaultFile)file);
		}
	}

	internal void DeleteChildReference(IVaultEntity child)
	{
		if (child.Parent != this)
		{
			throw new InvalidOperationException("The specified entity is not a child of this folder.");
		}
		
		if (child is IVaultFolder folder)
		{
			_subfolders.Remove((FileSystemVaultFolder)folder);
		}
		else if (child is IVaultFile file)
		{
			_files.Remove((FileSystemVaultFile)file);
		}
	}
	
	public IReadOnlyList<IVaultFolder> Subfolders => _subfolders;

	public IReadOnlyList<IVaultFile> Files => _files;
}