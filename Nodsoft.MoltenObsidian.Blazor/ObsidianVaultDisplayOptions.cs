namespace Nodsoft.MoltenObsidian.Blazor;

/// <summary>
/// Defines options for the <see cref="ObsidianVaultDisplay" />.
/// </summary>
/// <seealso cref="ObsidianVaultDisplay" />
public readonly struct ObsidianVaultDisplayOptions
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianVaultDisplayOptions" /> class.
	/// </summary>
	public ObsidianVaultDisplayOptions() { }
	
	/// <summary>
	/// If enabled, any <c>README.md</c> or <c>index.md</c> file will be displayed as the default/index page.
	/// This setting can be individually overriden at file-level using the <c>moltenobsidian:index:enabled</c> front matter property.
	/// </summary>
	/// <remarks>
	/// Defaults to <see langword="true" />.
	/// </remarks>
	public bool DisplayIndexNoteOnFolderRoot { get; init; } = true;
	
	/// <summary>
	/// If enabled, a folder index note display will also render the navigation tree for the current folder.
	/// This setting can be individually overriden by the <c>moltenobsidian:index:navigation</c> front matter property.
	/// </summary>
	/// <remarks>
	/// Defaults to <see langword="true" />.
	/// </remarks>
	/// <seealso cref="DisplayIndexNoteOnFolderRoot" />
	public bool DisplayFolderNavigation { get; init; } = true;
}