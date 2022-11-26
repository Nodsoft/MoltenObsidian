using ColorCode.Styling;
using Markdig;

#if NET6_0_OR_GREATER
using Markdown.ColorCode;
#else
using Markdig.SyntaxHighlighting;
#endif


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
#if NET6_0_OR_GREATER
			.UseColorCode(darkTheme ? StyleDictionary.DefaultDark : StyleDictionary.DefaultLight)
#else
			.UseSyntaxHighlighting()
#endif
			;

		// Configure the pipeline for Bootstrap support, if requested.
		if (useBootstrap)
		{
			this.UseBootstrap();
		}
	}
}