using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor.Services;

/// <summary>
/// Provides Blazor routing for an Obsidian vault.
/// </summary>
public sealed class VaultRouter
{
	private readonly IVault _vault;
	private readonly Dictionary<string, IVaultEntity> _routes;

	/// <summary>
	/// Initializes a new instance of the <see cref="VaultRouter"/> class.
	/// </summary>
	/// <param name="vault">The vault to route.</param>
	public VaultRouter(IVault vault)
	{
		_vault = vault;
		_routes = BuildRoutingTable(vault);
	}
	
	/// <summary>
	/// Routes a path to a vault entity.
	/// </summary>
	/// <param name="path">The path to route.</param>
	/// <returns>The entity at the specified path.</returns>
	public IVaultEntity? RouteTo(string path)
	{
		// Find the route within the routing table.
		// First perform a case-sensitive search, falling back to a case-insensitive search if not found.
		
		if (_routes.TryGetValue(path, out IVaultEntity? caseSensitive))
		{
			return caseSensitive;
		}

		if (_routes.FirstOrDefault(x => x.Key.Equals(path, StringComparison.OrdinalIgnoreCase)).Value is { } caseInsensitive)
		{
			return caseInsensitive;
		}

		return null;
	}

	private static Dictionary<string, IVaultEntity> BuildRoutingTable(IVault vault)
	{
		// First. Initialize the routing table.
		Dictionary<string, IVaultEntity> routingTable = new();
		
		// Folders go first.
		foreach (IVaultFolder folder in vault.Folders.Values)
		{
			routingTable.Add(folder.Path, folder);
		}
		
		// Then markdown files.
		foreach (IVaultFile file in vault.Files.Values)
		{
			// Before adding the file, we clear the extension.
			// This is because we want to route to the file without the extension.
			// For example, if we have a file named "test.md", we want to route to "/test".
			routingTable.Add(file.Path.Replace(".md", ""), file);
		}

		return routingTable;
	}
}