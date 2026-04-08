using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Services;

/// <summary>
/// Provides tests for the <see cref="VaultRouterFactory"/> class.
/// </summary>
/// <seealso cref="VaultRouterFactory"/>
public sealed class VaultRouterFactoryTests
{
	/// <summary>
	/// Tests that requesting a router for the same vault instance returns the same cached router.
	/// </summary>
	[Fact]
	public void GetRouter_SameVaultInstance_ReturnsCachedRouter()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		VaultRouterFactory factory = new();

		// Act
		VaultRouter first = factory.GetRouter(vault);
		VaultRouter second = factory.GetRouter(vault);

		// Assert – must be the exact same instance
		Assert.Same(first, second);
	}

	/// <summary>
	/// Tests that requesting routers for two distinct vault instances returns different routers.
	/// </summary>
	[Fact]
	public void GetRouter_DifferentVaultInstances_ReturnsDifferentRouters()
	{
		// Arrange
		InMemoryVault vault1 = new("Vault1");
		InMemoryVault vault2 = new("Vault2");
		VaultRouterFactory factory = new();

		// Act
		VaultRouter router1 = factory.GetRouter(vault1);
		VaultRouter router2 = factory.GetRouter(vault2);

		// Assert – must be different instances
		Assert.NotSame(router1, router2);
	}

	/// <summary>
	/// Tests that the router returned by the factory actually routes to the vault's contents.
	/// </summary>
	[Fact]
	public async Task GetRouter_RouterRoutesToVaultContents()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		await vault.WriteNoteAsync("doc.md", Stream.Null);
		VaultRouterFactory factory = new();

		// Act
		VaultRouter router = factory.GetRouter(vault);
		IVaultEntity? result = router.RouteTo("doc");

		// Assert
		Assert.NotNull(result);
		Assert.IsAssignableFrom<IVaultNote>(result);
	}

	/// <summary>
	/// Tests that the factory creates a new router for a previously unseen vault.
	/// </summary>
	[Fact]
	public void GetRouter_NewVault_ReturnsNewRouter()
	{
		// Arrange
		InMemoryVault vault = new("TestVault");
		VaultRouterFactory factory = new();

		// Act
		VaultRouter router = factory.GetRouter(vault);

		// Assert
		Assert.NotNull(router);
	}
}
