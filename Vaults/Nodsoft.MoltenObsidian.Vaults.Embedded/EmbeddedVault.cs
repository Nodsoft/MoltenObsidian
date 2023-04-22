using System.Reflection;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Embedded;

/// <summary>
/// Defines a vault that is embedded inside an assembly.
/// </summary>
public class EmbeddedVault : IVault
{
	private readonly Assembly _assembly;

	/// <summary>
	/// Initializes a new instance of the <see cref="EmbeddedVault"/> class.
	/// </summary>
	/// <param name="assembly">The assembly to load the vault from.</param>
	public EmbeddedVault(Assembly assembly)
	{
		_assembly = assembly;
		Name = assembly.GetName().Name ?? "Embedded Vault";
	}
	
	/// <inheritdoc />
	public string Name { get; }
	public IVaultFolder Root { get; }
	public IReadOnlyDictionary<string, IVaultFile> Files { get; }
	public IReadOnlyDictionary<string, IVaultFolder> Folders { get; }
	public IReadOnlyDictionary<string, IVaultNote> Notes { get; }
}