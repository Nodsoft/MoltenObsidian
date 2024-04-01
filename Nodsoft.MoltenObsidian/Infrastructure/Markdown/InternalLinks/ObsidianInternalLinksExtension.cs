using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Renderers.Normalize;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;

/// <summary>
/// Provides an extension for the Markdig parser to support Obsidian internal links.
/// </summary>
public sealed class InternalLinksExtension : IMarkdownExtension
{
	public InternalLinksExtension()
	{
		
	}
	
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<ObsidianInternalLinksParser>())
		{
			// Insert the parser before the link inline parser
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new ObsidianInternalLinksParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}