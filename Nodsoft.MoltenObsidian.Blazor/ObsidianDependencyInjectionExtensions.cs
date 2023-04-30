using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor;

/// <summary>
/// Extension methods for setting up a MoltenObsidian vault display on Blazor.
/// </summary>
[PublicAPI]
public static class ObsidianDependencyInjectionExtensions
{
	/// <summary>
	/// Adds the MoltenObsidian vault display to the Blazor application.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddMoltenObsidianBlazorIntegration(this IServiceCollection services)
	{
		services.AddSingleton<VaultRouterFactory>();
		return services;
	}
}