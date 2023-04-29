using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor.Services;

public interface IVaultRouter
{
	/// <summary>
	/// Routes a path to a vault entity.
	/// </summary>
	/// <param name="path">The path to route.</param>
	/// <returns>The entity at the specified path.</returns>
	IVaultEntity? RouteTo(string path);
}