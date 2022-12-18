using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

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
		=> services.AddSingleton<IVault>(FileSystemVault.FromDirectory(rootDirectoryInfo));

	/// <summary>
	/// Adds a Filesystem-based Vault to the service collection.
	/// </summary>
	/// <param name="services">The service collection to add the Vault to.</param>
	/// <param name="rootDirectoryInfoProvider">A function that returns the root directory of the Vault, given a service provider.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMoltenObsidianFileSystemVault(this IServiceCollection services, Func<IServiceProvider, DirectoryInfo> rootDirectoryInfoProvider) 
		=> services.AddSingleton<IVault>(s => FileSystemVault.FromDirectory(rootDirectoryInfoProvider(s)));
}