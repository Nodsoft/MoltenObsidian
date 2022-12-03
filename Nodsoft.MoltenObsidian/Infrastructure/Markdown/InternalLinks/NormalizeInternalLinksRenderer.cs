using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Renderers.Normalize;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;

/// <summary>
/// Provides a derived renderer for the <see cref="InternalLink"/> inline element.
/// </summary>
public sealed class NormalizeInternalLinksRenderer : NormalizeObjectRenderer<InternalLink>
{
	/// <inheritdoc/>
	protected override void Write(NormalizeRenderer renderer, InternalLink obj)
	{
		renderer.Write(obj);
	}
}