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
		"""
			^\[\[(
				# link, and optional anchor
				(?<link>[^\|\#\]]+)?(\#(?<anchor>[^\|\]]+))?
				# display title (optional)
				(\|(?<title>[^|\]]+))?
				# tooltip (optional)
				(\|(?<tooltip>[^|\]]+))?
			)\]\]
		""",
		RegexOptions.Compiled 
		| RegexOptions.IgnorePatternWhitespace 
		| RegexOptions.ExplicitCapture 
		| RegexOptions.Multiline
	);
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianInternalLinksParser"/> class.
	/// </summary>
	public ObsidianInternalLinksParser()
	{
		OpeningCharacters = ['['];
	}

	/// <inheritdoc />
	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		// Seek to the first two opening brackets
		if (slice.CurrentChar is not '[' && slice.PeekChar() is not '[')
		{
			return false;
		}

		// Grab the remainder of the slice, and check if it matches the internal link pattern.
		Match match = InternalLinkRegex.Match(slice.Text[slice.Start..slice.End]);

		if (match is { Groups: [{ Name: "0" }]})
		{
			return false;
		}


		InternalLink internalLink = new()
		{
			TargetNote = match.Groups["link"].Value,
			TargetSection = match.Groups["anchor"].Value,
			Display = match.Groups["title"].Value, 
			Tooltip = match.Groups["tooltip"].Value,

			Span =
			{
				Start = processor.GetSourcePosition(slice.Start, out int line, out int column),
				End = processor.GetSourcePosition(slice.Start + match.Length, out _, out _)
			},
			Line = line,
			Column = column,
		};

		if ((processor.Context?.Properties.GetValueOrDefault("currentFile") as IVaultNote) is not { } currentFile
		    || internalLink.ResolveVaultLink(currentFile) is not { } resolved)
		{
			return false;
		}

		internalLink.Url = internalLink switch
		{
			{ TargetNote: not (null or ""), TargetSectionLinkFragment: { } section and not "" } 
				when resolved.Path is [.. var path, '.', 'm', 'd']
				=> $"{path}#{section.ToLowerInvariant().Replace(' ', '-')}",
				
			{ TargetNote: not (null or "") } when resolved.Path is [.. var path, '.', 'm', 'd'] => path,
			{ TargetSection: not (null or "") } => $"{currentFile.Path}#{internalLink.TargetSectionLinkFragment}",
			_ => "#"
		};

		internalLink.Title = internalLink switch
		{
			{ Display: { } display and not "" } => display,
			{ TargetNote: { } note and not "", TargetSection: { } section and not "" } => $"{note.Split("/").Last()} > {section}",
			{ TargetNote: not (null or "") } => internalLink.TargetNote.Split("/").Last(),
			{ TargetSection: not (null or "") } => internalLink.TargetSection,
			_ => string.Empty
		};
			
		internalLink.AppendChild(new LiteralInline(internalLink.Title));
		processor.Inline = internalLink;
			
		// Adjust the slice to account for the matched text
		slice.Start += match.Length;
			
		return true;
	}
}