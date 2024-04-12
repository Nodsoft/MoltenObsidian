using System.Runtime.Versioning;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for adding MoltenObsidian Filesystem vaults to the service collection.
/// </summary>
[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
	/// <summary>
	/// Adds a Filesystem-based Vault to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add the Vault to.</param>
	/// <param name="rootDirectoryInfo">The root directory of the Vault.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMoltenObsidianFileSystemVault(this IServiceCollection services, DirectoryInfo rootDirectoryInfo)
	{
		services.AddSingleton<IWritableVault>(s => FileSystemVault.FromDirectory(rootDirectoryInfo));
		services.AddTransient<IVault>(s => s.GetRequiredService<IWritableVault>());
		
		return services;
	}

	/// <summary>
	/// Adds a Filesystem-based Vault to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add the Vault to.</param>
	/// <param name="rootDirectoryInfoProvider">A function that returns the root directory of the Vault, given a service provider.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMoltenObsidianFileSystemVault(this IServiceCollection services, Func<IServiceProvider, DirectoryInfo> rootDirectoryInfoProvider)
	{
		services.AddSingleton<IWritableVault>(s => FileSystemVault.FromDirectory(rootDirectoryInfoProvider(s)));
		services.AddTransient<IVault>(s => s.GetRequiredService<IWritableVault>());
		
		return services;
	}
}