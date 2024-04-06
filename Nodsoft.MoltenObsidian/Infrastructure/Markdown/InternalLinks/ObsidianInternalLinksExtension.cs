using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;

/// <summary>
/// Provides an extension for the Markdig parser to support Obsidian internal links.
/// </summary>
public sealed class InternalLinksExtension : IMarkdownExtension
{
	/// <summary>
	/// Initializes a new instance of the <see cref="InternalLinksExtension"/> class.
	/// </summary>
	public InternalLinksExtension() { }

	/// <inheritdoc />
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<ObsidianInternalLinksParser>())
		{
			// Insert the parser before the link inline parser
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new ObsidianInternalLinksParser());
		}
	}

	/// <inheritdoc />
	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}