using Markdig;
using Markdig.SyntaxHighlighting;

namespace Nodsoft.MoltenObsidian.Converter;

/// <summary>
/// Provides a pipeline builder for Markdown to HTML conversion.
/// </summary>
public sealed class ObsidianPipelineBuilder : MarkdownPipelineBuilder
{
	 /// <summary>
    /// Initializes a new instance of the <see cref="ObsidianPipelineBuilder"/> class.
    /// </summary>
    public ObsidianPipelineBuilder(bool useBootstrap = false)
	{
		// Configure the pipeline for all features. This should enable 90% of all Obsidian MD features.
		this.UseAdvancedExtensions()
			.UseSyntaxHighlighting();

		// Configure the pipeline for Bootstrap support, if requested.
		if (useBootstrap)
		{
			this.UseBootstrap();
		}
	}
}