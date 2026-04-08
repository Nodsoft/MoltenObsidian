using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Blazor.Helpers;

namespace Nodsoft.MoltenObsidian.Blazor.Tests.Helpers;

/// <summary>
/// Provides tests for the <see cref="VaultComponentHelpers"/> class.
/// </summary>
/// <seealso cref="VaultComponentHelpers"/>
public sealed class VaultComponentHelpersTests
{
	/// <summary>
	/// A fake component with a route that has a catch-all slug segment.
	/// </summary>
	[Route("/vault/{*slug}")]
	private sealed class ComponentWithSlugRoute : IComponent
	{
		public void Attach(RenderHandle renderHandle) { }
		public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
	}

	/// <summary>
	/// A fake component with a simple route that has no trailing slug.
	/// </summary>
	[Route("/docs")]
	private sealed class ComponentWithSimpleRoute : IComponent
	{
		public void Attach(RenderHandle renderHandle) { }
		public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
	}

	/// <summary>
	/// A fake component with a nested route that has a catch-all slug segment.
	/// </summary>
	[Route("/content/section/{*slug}")]
	private sealed class ComponentWithNestedSlugRoute : IComponent
	{
		public void Attach(RenderHandle renderHandle) { }
		public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
	}

	/// <summary>
	/// A fake component with no <see cref="RouteAttribute"/>.
	/// </summary>
	private sealed class ComponentWithoutRouteAttribute : IComponent
	{
		public void Attach(RenderHandle renderHandle) { }
		public Task SetParametersAsync(ParameterView parameters) => Task.CompletedTask;
	}

	/// <summary>
	/// Tests that a component with a slug segment at the end has the slug stripped,
	/// returning just the base path with a trailing slash.
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_ComponentWithSlugRoute_ReturnsBasePath()
	{
		// Act
		string basePath = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithSlugRoute>();

		// Assert
		Assert.Equal("/vault/", basePath);
	}

	/// <summary>
	/// Tests that a component with a simple route (no slug) returns the route with a trailing slash appended.
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_ComponentWithSimpleRoute_ReturnsRouteWithTrailingSlash()
	{
		// Act
		string basePath = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithSimpleRoute>();

		// Assert
		Assert.Equal("/docs/", basePath);
	}

	/// <summary>
	/// Tests that a component with a nested slug route returns the correct base path.
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_ComponentWithNestedSlugRoute_ReturnsNestedBasePath()
	{
		// Act
		string basePath = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithNestedSlugRoute>();

		// Assert
		Assert.Equal("/content/section/", basePath);
	}

	/// <summary>
	/// Tests that calling the method twice for the same component type returns the same value (caching).
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_SameComponentType_ReturnsCachedResult()
	{
		// Act
		string first = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithSlugRoute>();
		string second = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithSlugRoute>();

		// Assert
		Assert.Equal(first, second);
	}

	/// <summary>
	/// Tests that a component without a <see cref="RouteAttribute"/> throws <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_ComponentWithoutRouteAttribute_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(
			() => VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithoutRouteAttribute>()
		);
	}

	/// <summary>
	/// Tests that the base path always ends with a trailing slash.
	/// </summary>
	[Fact]
	public void GetCallingBaseVaultPath_ResultAlwaysHasTrailingSlash()
	{
		// Act
		string basePath = VaultComponentHelpers.GetCallingBaseVaultPath<ComponentWithSimpleRoute>();

		// Assert
		Assert.EndsWith("/", basePath);
	}
}
