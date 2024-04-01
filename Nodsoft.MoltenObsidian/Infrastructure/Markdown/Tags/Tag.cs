using Markdig.Syntax.Inlines;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.Tags;

/// <summary>
/// Defines an Obsidian Tag within a Markdown document.
/// </summary>
/// <inheritdoc />
public sealed class Tag(string name) : Inline
{
    /// <summary>
    /// The name of the tag.
    /// </summary>
    public string Name { get; } = name;
}