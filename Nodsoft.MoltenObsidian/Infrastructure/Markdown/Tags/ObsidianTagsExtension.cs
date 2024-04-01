using Markdig;
using Markdig.Renderers;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.Tags;

/// <summary>
/// Provides an extension for the Markdig parser to support Obsidian tags.
/// </summary>
public class ObsidianTagsExtension : IMarkdownExtension
{
    /// <inheritdoc />
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<ObsidianTagsParser>())
        {
            pipeline.InlineParsers.Add(new ObsidianTagsParser());
        }
    }

    /// <inheritdoc />
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new ObsidianTagsRenderer());
        }
    }
}