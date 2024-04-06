using System.Text.RegularExpressions;
using Markdig.Syntax.Inlines;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;

/// <summary>
/// Specifies an an Obsidian internal link, used to link to other notes in the vault.
/// </summary>
/// <example>
///	Syntax: <c>[[link]] | [[link|display]] | [[link|display|tooltip]]</c>
/// The link can be a relative path, an absolute path, and/or a section anchor.
/// </example>
public sealed class InternalLink : LinkInline
{
	/// <summary>
	/// Initializes a new instance of the <see cref="InternalLink"/> class.
	/// </summary>
	public InternalLink()
	{
		IsClosed = true;
	}
	
	/// <summary>
	/// Regex substitution pattern for internal link anchors, to replace spaces with dashes and remove all other invalid characters.
	/// </summary>
	private static readonly Regex AnchorRegex = new(@"[^\w.]+", RegexOptions.Compiled);

	
	/// <summary>
	/// The targeted note of this internal link.
	/// </summary>
	public string? TargetNote { get; set; }

	/// <summary>
	/// The section anchor of this internal link.
	/// </summary>
	/// <remarks>
	/// The section anchor is the part of the link after the <c>#</c> character.
	/// It MUST be present if <see cref="TargetNote"/> is <see langword="null"/>.
	/// </remarks>
	public string? TargetSection { get; set; }

	/// <summary>
	/// The fragment of the section anchor, suitable for use in a URL.
	/// </summary>
	public string? TargetSectionLinkFragment => TargetSection is null ? null : AnchorRegex.Replace(TargetSection, "-").Trim('-').ToLowerInvariant();
	
	/// <summary>
	/// The display text of the internal link, if any.
	/// </summary>
	public string? Display { get; set; }

	/// <summary>
	/// The tooltip text of the internal link, if any.
	/// </summary>
	public string? Tooltip { get; set; }

	/// <summary>
	/// Resolves the internal link to a URL, relative to the specified vault.
	/// </summary>
	/// <param name="currentNote">The note that contains the link.</param>
	/// <returns>The resolved file</returns>
	public IVaultFile? ResolveVaultLink(IVaultNote currentNote) 
		// Resolve the note.
		// Is it an absolute path? If so, resolve it against the vault root.
		// First, try to resolve it as a relative path, gradually moving up the directory tree.
		// If that fails, try to resolve it as an absolute path.
		=> TargetNote switch
		{
			null or "" when TargetSection is not (null or "") => currentNote,

			['/', .. var target] when currentNote.Vault.GetFile(target) is { } file => file,
			[not '/', ..] note => currentNote.ResolveRelativeLink(note),
			_ => null
		};
}