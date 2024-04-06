using Markdig.Helpers;
using Markdig.Parsers;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.Tags;

/// <summary>
/// Provides parsing for Obsidian Tags for Markdig.
/// </summary>
public class ObsidianTagsParser : InlineParser
{
//     private static readonly Regex _tagRegex = new(
//         @"\#(?<tag>[a-zA-Z_/-][\w_/-]*)",
//         RegexOptions.Compiled
//         | RegexOptions.IgnorePatternWhitespace
//         | RegexOptions.ExplicitCapture
// #if NET8_0_OR_GREATER
//         | RegexOptions.NonBacktracking
// #endif
//     );
    
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
        if (slice.CurrentChar is not '#' || !slice.PeekChar(-1).IsWhiteSpaceOrZero() || slice.PeekChar() is '#')
        {
            return false;
        }

        // Grab the remainder of the slice, and check if it matches the tag pattern.
        
        // Set the end position to the next blank space, forbidden character, or end of the slice.
        // Allowed chars: a-z, A-Z, 0-9, _, /, -
        int endPos = slice.Start;
        while (endPos < slice.Text.Length && !slice[endPos].IsWhiteSpaceOrZero())
        {
            endPos++;
        }

        if (slice.Text[slice.Start..endPos] is not ['#', .. [_, ..] match])
        {
            return false;
        }
        
        // Create a new tag inline, and add it to the processor.
        processor.Inline = new Tag(match)
        {
            Span = { Start = slice.Start, End = endPos }
        };

        slice.Start = endPos + 1;
        return true;
    }
}