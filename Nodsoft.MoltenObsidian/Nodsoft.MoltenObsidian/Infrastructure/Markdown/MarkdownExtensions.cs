using Markdig;
using Nodsoft.MoltenObsidian.Infrastructure.Markdown.InternalLinks;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Infrastructure.Markdown;

/// <summary>
/// Provides extension methods for <see cref="MarkdownPipelineBuilder"/> to enable several Markdown extensions.
/// </summary>
public static class MarkdownExtensions
{
	/// <summary>
	/// Enables support for Obsidian wikilinks-style internal links.
	/// </summary>
	/// <remarks>
	/// This extension requires the ParserContext to contain the holding <see cref="IVaultMarkdownFile" /> on key <c>vaultFile</c> in order to resolve the links.
	/// </remarks>
	/// <param name="builder">The <see cref="MarkdownPipelineBuilder"/> to enable the extension on.</param>
	/// <returns>The <see cref="MarkdownPipelineBuilder"/> with the extension enabled.</returns>
	public static MarkdownPipelineBuilder UseInternalLinks(this MarkdownPipelineBuilder builder)
	{
		builder.Extensions.AddIfNotAlready<InternalLinksExtension>();
		return builder;
	}
}