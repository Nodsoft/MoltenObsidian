namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Represents the event arguments for when a vault is updated.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VaultUpdateEventArgs"/> class.
/// </remarks>
/// <param name="type">The type of update that occurred.</param>
/// <param name="entity">The entity that was updated.</param>
public class VaultUpdateEventArgs(UpdateType type, IVaultEntity entity) : EventArgs
{

    /// <summary>
    /// Gets the type of update that occurred.
    /// </summary>
    public UpdateType Type { get; } = type;

    /// <summary>
    /// Gets the entity that was updated.
    /// </summary>
    public IVaultEntity Entity { get; } = entity;
    
    /// <summary>
    /// Gets the type of entity that was updated.
    /// </summary>
    public EntityType EntityType { get; } = entity switch
    {
        IVaultFolder => EntityType.Folder,
        IVaultNote => EntityType.Note,
        IVaultFile => EntityType.File,
        _ => throw new ArgumentOutOfRangeException(nameof(entity), "Invalid entity type.")
    };
}

/// <summary>
/// Represents the type of update that occurred in a vault.
/// </summary>
public enum UpdateType
{
    /// <summary>
    /// An item was added to the vault.
    /// </summary>
    Add,
    
    /// <summary>
    /// An item was removed from the vault.
    /// </summary>
    Remove,
    
    /// <summary>
    /// An item was updated in the vault.
    /// </summary>
    Update,
    
    /// <summary>
    /// An item was moved within the vault.
    /// </summary>
    Move
}

/// <summary>
/// Represents the Vault entity type that was updated.
/// </summary>
/// <seealso cref="IVaultEntity"/>
public enum EntityType
{
    /// <summary>
    /// The entity is a folder.
    /// </summary>
    Folder,
    
    /// <summary>
    /// The entity is a file.
    /// </summary>
    File,
    
    /// <summary>
    /// The entity is a note.
    /// </summary>
    Note
}