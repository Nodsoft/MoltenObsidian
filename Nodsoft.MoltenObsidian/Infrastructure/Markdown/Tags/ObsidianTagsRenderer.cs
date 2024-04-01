using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.Tags;

/// <summary>
/// Provides rendering for Obsidian Tags for Markdig.
/// </summary>
public class ObsidianTagsRenderer : HtmlObjectRenderer<Tag>
{
    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, Tag obj)
    {
        if (!renderer.EnableHtmlForInline)
        {
            return;
        }

        renderer.Write(/*lang=html*/$"<span class=\"tag moltenobsidian-tag\" data-name=\"{obj.Name.ToLower()}\">");
        renderer.Write(obj.Name);
        renderer.WriteAttributes(obj);
        renderer.Write(/*lang=html*/"</span>");
    }
}