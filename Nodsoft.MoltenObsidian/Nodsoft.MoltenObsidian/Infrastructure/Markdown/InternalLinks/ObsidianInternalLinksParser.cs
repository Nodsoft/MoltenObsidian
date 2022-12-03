using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax.Inlines;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;

/// <summary>
/// Provides parsing for Obsidian internal links for Markdig.
/// </summary>
/// <seealso cref="InternalLink" />
public sealed class ObsidianInternalLinksParser : InlineParser
{
	/// <summary>
	/// Regex pattern with named variables for internal links.
	/// This pattern is based on the Obsidian internal link syntax, and supports links (and/or anchors), display titles, and tooltips.
	/// </summary>
	/// <example>
	///	The following are all valid internal links:
	/// [[mySecretNote]]
	/// [[my-other-note#over-there]]
	/// [[note_one|not note one]]
	/// [[note#section|display|tooltip]]
	/// </example>
	private static readonly Regex InternalLinkRegex = new(
		@"\[\[(
			# link, and optional anchor
			(?<link>[^\|\]]+)(\#(?<anchor>[^\|\]]+))? 
			# display title (optional)
			(\|(?<title>[^|\]]+))? 
			# tooltip (optional)
			(\|(?<tooltip>[^|\]]+))? 
		)\]\]",
		RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianInternalLinksParser"/> class.
	/// </summary>
	public ObsidianInternalLinksParser()
	{
		OpeningCharacters = new[] { '[', ']' };
	}

	/// <inheritdoc />
	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		// Seek to the first two opening brackets
		if (slice.CurrentChar is not '[')
		{
			return false;
		}
		
		// Grab the remainder of the slice, and check if it matches the internal link pattern.
		string? remainder = slice.Text[slice.Start..];
		Match match = InternalLinkRegex.Match(slice.Text, slice.Start);

		if (!match.Success)
		{
			return false;
		}

//		// Adjust the slice to account for the matched text
		slice.Start = match.Index + match.Length;
//		slice.End = slice.Start + match.Length;
		
			
		
		InternalLink internalLink = new()
		{
			TargetNote = match.Groups["link"].Value,
			TargetSection = match.Groups["anchor"].Value,
			Display = match.Groups["title"].Value, 
			Tooltip = match.Groups["tooltip"].Value,
			IsClosed = true
		};

		if (processor.Context?.Properties.GetValueOrDefault("currentFile") as IVaultMarkdownFile is { } currentFile 
			&& internalLink.ResolveVaultLink(currentFile) is { } resolved)
		{
			internalLink.Url = internalLink switch
			{
				{ TargetNote: not null, TargetSection: { } section and not "" } when resolved.Path is [.. var path, '.', 'm', 'd']
					=> $"{path}#{section.ToLowerInvariant().Replace(' ', '-')}",
				{ TargetNote: not null } when resolved.Path is [.. var path, '.', 'm', 'd'] => path,
				{ TargetSection: not (null or "") } => $"#{internalLink.TargetSection.ToLowerInvariant().Replace(' ', '-')}",
				_ => "#"
			};

			internalLink.Title = internalLink switch
			{
				{ Display: { } display and not "" } => display,
				{ TargetNote: { } note and not "", TargetSection: { } section and not "" } => $"{note}#{section}",
				{ TargetNote: not (null or "") } => internalLink.TargetNote,
				{ TargetSection: not (null or "") } => $"#{internalLink.TargetSection}",
				_ => string.Empty
			};
			
			internalLink.AppendChild(new LiteralInline(internalLink.Title));
			processor.Inline = internalLink;
			
			return true;
		}

		return false;
	}
}