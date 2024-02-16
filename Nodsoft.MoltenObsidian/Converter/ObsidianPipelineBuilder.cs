using ColorCode.Styling;
using Markdig;
using Nodsoft.Markdig.SyntaxHighlighting;
using Nodsoft.MoltenObsidian.Infrastructure.Markdown;

namespace Nodsoft.MoltenObsidian.Converter;

/// <summary>
/// Provides a pipeline builder for Markdown to HTML conversion.
/// </summary>
public sealed class ObsidianPipelineBuilder : MarkdownPipelineBuilder
{
	 /// <summary>
    /// Initializes a new instance of the <see cref="ObsidianPipelineBuilder"/> class.
    /// </summary>
    public ObsidianPipelineBuilder(bool useBootstrap = false, bool darkTheme = false)
	{
		// Configure the pipeline for all features. This should enable 90% of all Obsidian MD features.
		this.UseAdvancedExtensions()
			.UseInternalLinks()
			.UseSyntaxHighlighting(darkTheme ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight);

		// Configure the pipeline for Bootstrap support, if requested.
		if (useBootstrap)
		{
			this.UseBootstrap();
		}
	}
}