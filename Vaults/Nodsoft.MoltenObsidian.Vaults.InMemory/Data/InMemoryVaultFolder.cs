using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

/// <summary>
/// Provides an in-memory implementation of the <see cref="IVaultFolder"/> interface.
/// </summary>
/// <seealso cref="InMemoryVault"/>
/// <seealso cref="InMemoryVaultFile"/>
internal sealed class InMemoryVaultFolder : InMemoryVaultEntityBase, IVaultFolder
{
    internal List<InMemoryVaultFolder> Subfolders { get; } = [];
    internal List<InMemoryVaultFile> Files { get; } = [];

    internal InMemoryVaultFolder(string name, InMemoryVaultFolder? parent, InMemoryVault vault) : base(name, parent, vault) { }

    
    IReadOnlyList<IVaultFolder> IVaultFolder.Subfolders => Subfolders;
    IReadOnlyList<IVaultFile> IVaultFolder.Files => Files;

    /// <summary>
    /// Creates a new <see cref="InMemoryVaultFolder"/> instance and adds it to the specified parent folder.
    /// </summary>
    /// <remarks>
    /// The expected path should be relative to the vault root.
    /// </remarks>
    /// <param name="path">The path of the folder to create.</param>
    /// <param name="parent">The parent folder of the folder to create.</param>
    /// <param name="vault">The vault that the folder belongs to.</param>
    /// <returns>The newly created <see cref="InMemoryVaultFolder"/> instance.</returns>
    internal static InMemoryVaultFolder CreateFolder(string path, InMemoryVaultFolder? parent, InMemoryVault vault)
    {
        InMemoryVaultFolder folder = new(path, parent, vault);
        parent?.AddChildReference(folder);
        
        return folder;
    }
    
    /// <summary>
    /// Deletes a folder and contents from memory.
    /// </summary>
    internal void DeleteFolder()
    {
        foreach (InMemoryVaultFile file in Files)
        {
            file.DeleteFile();
        }
        
        Subfolders.Clear();
        Files.Clear();
        
        Parent?.DeleteChildReference(this);
    }
    
    /// <summary>
    /// Adds a child element to the folder.
    /// </summary>
    /// <param name="child">The child element to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the specified entity is not a child of this folder.</exception>
    internal void AddChildReference(InMemoryVaultEntityBase child)
    {
        if (child.Parent != this)
        {
            throw new InvalidOperationException("The specified entity is not a child of this folder.");
        }
        
        if (child is InMemoryVaultFolder folder)
        {
            Subfolders.Add(folder);
        }
        else if (child is InMemoryVaultFile file)
        {
            Files.Add(file);
        }
    }
    
    /// <summary>
    /// Deletes a child element from the folder.
    /// </summary>
    /// <param name="child">The child element to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown when the specified entity is not a child of this folder.</exception>
    internal void DeleteChildReference(InMemoryVaultEntityBase child)
    {
        if (child.Parent != this)
        {
            throw new InvalidOperationException("The specified entity is not a child of this folder.");
        }
        
        if (child is InMemoryVaultFolder folder)
        {
            Subfolders.Remove(folder);
        }
        else if (child is InMemoryVaultFile file)
        {
            Files.Remove(file);
        }
    }
}