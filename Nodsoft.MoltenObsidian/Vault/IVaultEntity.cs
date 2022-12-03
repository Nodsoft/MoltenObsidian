namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Represents a common interface for all vault objects, may they be folders or files.
/// </summary>
/// <remarks>
///	This interface is not intended to be implemented by any class other than the ones provided by the library.
/// </remarks>
public interface IVaultEntity
{
	/// <summary>
	/// The name of the entity.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The full path of the entity, relative to the vault root.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// The folder that contains this entity.
	/// </summary>
	IVaultFolder? Parent { get; }
	
	/// <summary>
	/// The vault that this entity belongs to.
	/// </summary>
	IVault Vault { get; }
}