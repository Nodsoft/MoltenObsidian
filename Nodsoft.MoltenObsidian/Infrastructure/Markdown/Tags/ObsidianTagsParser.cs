using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.Tags;

/// <summary>
/// Provides parsing for Obsidian Tags for Markdig.
/// </summary>
public class ObsidianTagsParser : InlineParser
{
    private static readonly Regex _tagRegex = new(
        @"\#(?<tag>[a-zA-Z_/-][\w_/-]*)",
        RegexOptions.Compiled
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.ExplicitCapture
#if NET8_0_OR_GREATER
        | RegexOptions.NonBacktracking
#endif
        
    );
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObsidianTagsParser"/> class.
    /// </summary>
    public ObsidianTagsParser()
    {
        OpeningCharacters = ['#'];
    }

    /// <inheritdoc />
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Seek to the first hash character
        if (slice.CurrentChar is not '#')
        {
            return false;
        }

        // Grab the remainder of the slice, and check if it matches the tag pattern.
        Match match = _tagRegex.Match(slice.Text[slice.Start..slice.End]);

        if (!match.Success)
        {
            return false;
        }

        // Create a new tag inline, and add it to the processor.
        processor.Inline = new Tag(match.Groups["tag"].Value)
        {
            Span = { Start = slice.Start, End = slice.Start + match.Length }
        };

        slice.Start += match.Length;
        return true;
    }
}