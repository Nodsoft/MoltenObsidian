using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor.Services;

/// <summary>
/// Provides a factory for creating <see cref="VaultRouter"/> instances.
/// Instances are cached and reused, dependent on the <see cref="IVault"/> instance.
/// </summary>
/// <seealso cref="VaultRouter"/>
public sealed class VaultRouterFactory
{
	private readonly Dictionary<IVault, VaultRouter> _routers = new();

	/// <summary>
	/// Gets the router for the specified vault.
	/// </summary>
	/// <param name="vault">The vault to get the router for.</param>
	/// <returns>The router for the specified vault.</returns>
	public VaultRouter GetRouter(IVault vault)
	{
		if (_routers.TryGetValue(vault, out VaultRouter? router))
		{
			return router;
		}

		router = new(vault);
		_routers.Add(vault, router);
		return router;
	}
}