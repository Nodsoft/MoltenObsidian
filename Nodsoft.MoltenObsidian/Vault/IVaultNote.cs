namespace Nodsoft.MoltenObsidian.Vault;

/// <summary>
/// Specifies a Markdown file, within an obsidian vault.
/// Once read, this file yields a <see cref="ObsidianText"/> instance.
/// </summary>
/// <remarks>
///	This interface is storage-agnostic, and should be able to be implemented using any storage mechanism.
/// </remarks>
/// <seealso cref="IVault"/>
/// <seealso cref="IVaultFile"/>
[PublicAPI]
public interface IVaultNote : IVaultFile
{
	/// <summary>
	/// The name of the obsidian note. This usually corresponds to the file name, without the extension.
	/// </summary>
	/// <remarks>
	/// This is a convenience property, and is equivalent to <see cref="IVaultEntity.Name"/> without the extension.
	/// </remarks>
	string NoteName => Name.EndsWith(".md") ? Name[..^3] : Name;
}