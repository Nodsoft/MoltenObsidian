using Markdig;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Nodsoft.MoltenObsidian.Converter;

/// <summary>
/// Provides conversion between Obsidian-flavoured Markdown and HTML.
/// </summary>
public sealed class ObsidianHtmlConverter
{
	/// <summary>
	/// Singleton instance of <see cref="ObsidianHtmlConverter"/>,
	/// with default settings mimicking Obsidian's behaviour as closely as possible.
	/// </summary>
	public static ObsidianHtmlConverter Default { get; } = new(new ObsidianPipelineBuilder(true).Build());
	
	private readonly MarkdownPipeline _pipeline;

	/// <summary>
	/// Initializes a new instance of the <see cref="ObsidianHtmlConverter"/> class.
	/// </summary>
	public ObsidianHtmlConverter(MarkdownPipeline pipeline)
	{
		_pipeline = pipeline;
	}

	/// <summary>
	/// Converts Obsidian-flavoured Markdown to HTML.
	/// </summary>
	/// <param name="markdown">The Markdown to convert.</param>
	/// <param name="parseYamlFrontMatter">Whether to parse YAML front matter and exclude it from the Markdown HTML output.</param>
	/// <returns>The converted Markdown, in HTML format.</returns>
	// ReSharper disable once RedundantNameQualifier
	public string Convert(string markdown) => Markdig.Markdown.ToHtml(markdown, _pipeline);
}