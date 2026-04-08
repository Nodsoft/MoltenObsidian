using Nodsoft.MoltenObsidian.Blazor.Services;

namespace Nodsoft.MoltenObsidian.Blazor.Tests;

/// <summary>
/// Provides tests for the <see cref="ObsidianDependencyInjectionExtensions"/> class.
/// </summary>
/// <seealso cref="ObsidianDependencyInjectionExtensions"/>
public sealed class ObsidianDependencyInjectionExtensionsTests
{
	/// <summary>
	/// Tests that <see cref="ObsidianDependencyInjectionExtensions.AddMoltenObsidianBlazorIntegration"/>
	/// registers <see cref="VaultRouterFactory"/> as a singleton service.
	/// </summary>
	[Fact]
	public void AddMoltenObsidianBlazorIntegration_RegistersVaultRouterFactory()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		services.AddMoltenObsidianBlazorIntegration();
		ServiceProvider provider = services.BuildServiceProvider();

		// Assert
		VaultRouterFactory? factory = provider.GetService<VaultRouterFactory>();
		Assert.NotNull(factory);
	}

	/// <summary>
	/// Tests that <see cref="VaultRouterFactory"/> is registered as a singleton
	/// (the same instance is returned on successive resolutions).
	/// </summary>
	[Fact]
	public void AddMoltenObsidianBlazorIntegration_VaultRouterFactory_IsSingleton()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddMoltenObsidianBlazorIntegration();
		ServiceProvider provider = services.BuildServiceProvider();

		// Act
		VaultRouterFactory first = provider.GetRequiredService<VaultRouterFactory>();
		VaultRouterFactory second = provider.GetRequiredService<VaultRouterFactory>();

		// Assert – singleton: same instance every time
		Assert.Same(first, second);
	}

	/// <summary>
	/// Tests that <see cref="ObsidianDependencyInjectionExtensions.AddMoltenObsidianBlazorIntegration"/>
	/// returns the same <see cref="IServiceCollection"/> instance to allow call chaining.
	/// </summary>
	[Fact]
	public void AddMoltenObsidianBlazorIntegration_ReturnsServiceCollection()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		IServiceCollection result = services.AddMoltenObsidianBlazorIntegration();

		// Assert
		Assert.Same(services, result);
	}
}
