using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Nodsoft.MoltenObsidian.Blazor.Helpers;

/// <summary>
/// Provides helper methods for working with Vault implementation components.
/// </summary>
public static class VaultComponentHelpers
{
	/// <summary>
	/// Regex for matching a slug segment at the end of a string. (e.g. "/{*slugName}")
	/// This is the same regex used by the Blazor Router to match slug segments.
	/// </summary>
	private static readonly Regex _slugRegex = new(@"\/\{\*[\w\d]+\}$", RegexOptions.Compiled);
	
	/// <summary>
	/// A dictionary containing the base vault path for a component type.
	/// </summary>
	/// <example>
	/// Key: The component type.
	/// <br />
	/// Value: The base vault path for the component type.
	/// </example>
	private static readonly Dictionary<string, string> _baseVaultPaths = new();

	/// <inheritdoc cref="GetCallingBaseVaultPath(Type)"/>
	/// <typeparam name="TComponent">The type of the component to get the base vault path for.</typeparam>
	public static string GetCallingBaseVaultPath<TComponent>() where TComponent : IComponent => GetCallingBaseVaultPath(typeof(TComponent));

	/// <inheritdoc cref="GetCallingBaseVaultPath(Type)"/>
	/// <param name="component">The component to get the base vault path for.</param>
	public static string GetCallingBaseVaultPath(this IComponent component) => GetCallingBaseVaultPath(component.GetType());
	
	/// <summary>
	/// Returns the BasePath of the Vault, through resolving the calling component's path.
	/// </summary>
	/// <remarks>
	/// This is a workaround for the fact that the <see cref="NavigationManager"/> does not provide a way to get the base path of the current page.
	/// This function resolves the base path by looking at the vault page component's <see cref="RouteAttribute"/>, and removing the slug segment at the end.
	/// This function cannot substitute any route parameters, so it is only useful for getting the base path of the current page.
	/// </remarks>
	/// <returns>The BasePath of the Vault.</returns>
	private static string GetCallingBaseVaultPath(Type componentType)
	{
		// Check if the base vault path has already been resolved for this component type.
		if (_baseVaultPaths.TryGetValue(componentType.FullName!, out string? resolved))
		{
			return resolved;
		}
		
		// First. Does the component have a RouteAttribute?
		RouteAttribute routeAttribute = componentType.GetCustomAttribute<RouteAttribute>() 
			?? throw new ArgumentException("The calling component does not have a RouteAttribute.", nameof(componentType));
		
		// Second. Does the RouteAttribute have a slug segment at the end?
		// If so, remove it.
		string basePath = _slugRegex.IsMatch(routeAttribute.Template) 
			? _slugRegex.Replace(routeAttribute.Template, string.Empty) 
			: routeAttribute.Template;
		
		// Third. Does the base path end with slashes?
		// If not, add it.
		if (!basePath.EndsWith('/'))
		{
			basePath = $"{basePath}/";
		}
		
		// Finally. Cache the resolved base path and return it.
		_baseVaultPaths.Add(componentType.FullName, basePath);
		return basePath;
	}
}