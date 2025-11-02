using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up an in-memory vault in the service collection.
/// </summary>
[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
	/// <summary>
	/// Adds an in-memory vault to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add to the vault</param>
	/// <param name="name">The name of the vault to add</param>
	/// <returns></returns>
	public static IServiceCollection AddMoltenObsidianInMemoryVault(this IServiceCollection services, string name)
	{
		return services.AddSingleton<IVault>(new InMemoryVault(name));
	}
}

