using System.Diagnostics.CodeAnalysis;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor.Helpers;

/// <summary>
/// Provides a set of extensions to work with vault navigation.
/// </summary>
public static class VaultNavigationHelpers
{
	/// <summary>
	/// Gets the index note of a vault folder,
	/// if it exists, and if the file was not unmmarked as a folder index file.
	/// </summary>
	/// <param name="folder">The vault folder.</param>
	/// <returns>The readme file, or null if not available.</returns>
	public static async ValueTask<IVaultNote?> GetIndexNoteAsync(this IVaultFolder folder)
	{
		// Find an index note file in the folder.
		if (folder.GetNotes(SearchOption.TopDirectoryOnly)
			.FirstOrDefault(static n
				=> n.Key.Equals("README.md", StringComparison.OrdinalIgnoreCase)
				|| n.Key.Equals("index.md", StringComparison.OrdinalIgnoreCase)) 
			is not { Value: { } note })
		{
			// No index note file found.
			return null;
		}
		
		// Does it have a front matter? Does it have the "moltenobsidian:index:enabled" key? Is it set to false?
		if (await note.ReadDocumentAsync() is { Frontmatter: { } frontMatter } document
			&& frontMatter.TryGetValue("moltenobsidian:index:enabled", out object? value)
			&& value is false)
		{
			// The index note file was unmarked at file level.
			return null;
		}
		
		// All good, return the note.
		return note;
	}
}