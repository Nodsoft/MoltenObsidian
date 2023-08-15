using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class VaultDependencyInjectionExtensions
{

	private static readonly MemoryCacheOptions DefaultCacheOptions = new();

	private static readonly MemoryCacheEntryOptions DefaultEntryCacheOptions = new MemoryCacheEntryOptions()
		.AddExpirationToken(new CancellationChangeToken(CancellationToken.None))
		.SetSlidingExpiration(TimeSpan.FromSeconds(5));

	/// <summary>
	/// Adds an in memory vault service to the service collection
	/// </summary>
	/// <param name="services">The service collection to add to the vault</param>
	/// <param name="cacheEntryOptionsProvider">Configure the cache entry options <see cref="MemoryCacheOptions"/></param>
	/// <param name="cacheOptionsProvider"></param>
	/// <returns></returns>
	public static IServiceCollection AddMoltenObsidianInMemoryVault(this IServiceCollection services, 
		MemoryCacheEntryOptions cacheEntryOptionsProvider, 
		MemoryCacheOptions cacheOptionsProvider, 
		DirectoryInfo rootDirectoryInfo)
	{
		MemoryCache cache = new MemoryCache(cacheOptionsProvider);
		return services.AddSingleton<IVault>(InMemoryVault.FromDirectory(cache, rootDirectoryInfo));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="services"></param>
	/// <returns></returns>
	public static IServiceCollection AddMoltenObsidianInMemoryVault(this IServiceCollection services, DirectoryInfo rootDirectoryInfo)
		=> services.AddMoltenObsidianInMemoryVault(DefaultEntryCacheOptions, DefaultCacheOptions, rootDirectoryInfo);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="services"></param>
	/// <param name="cacheEntryOptionsProvider"></param>
	/// <param name="rootDirectoryInfo"></param>
	/// <returns></returns>
	public static IServiceCollection AddMoltenObsidianInMemoryVault(this IServiceCollection services, 
		MemoryCacheEntryOptions cacheEntryOptionsProvider,
        DirectoryInfo rootDirectoryInfo)
		=> services.AddMoltenObsidianInMemoryVault(cacheEntryOptionsProvider, DefaultCacheOptions, rootDirectoryInfo);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="services"></param>
	/// <param name="cacheOptionsProvider"></param>
	/// <param name="rootDirectoryInfo"></param>
	/// <returns></returns>
	public static IServiceCollection AddMoltenObsidianInMemoryVault(this IServiceCollection services, 
		MemoryCacheOptions cacheOptionsProvider,
		DirectoryInfo rootDirectoryInfo)
		=> services.AddMoltenObsidianInMemoryVault(DefaultEntryCacheOptions, cacheOptionsProvider, rootDirectoryInfo);
}

