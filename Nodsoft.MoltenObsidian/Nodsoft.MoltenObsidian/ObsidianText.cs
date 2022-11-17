using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Converter;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nodsoft.MoltenObsidian;

/// <summary>
/// Represents a string of Obsian-flavoured Markdown text.
/// </summary>
public readonly record struct ObsidianText
{
	/// <summary>
	/// YAML deserializer used to parse the YAML frontmatter, if present.
	/// </summary>
	private static readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.Build();
	
	/// <summary>
	/// The text of the ObsidianText.
	/// </summary>
	public string Text { get; }
	
	/// <summary>
	/// The YAML header (frontmatter) found at the beginning of the text,
	/// parsed into a key-value dictionary.
	/// </summary>
	public Dictionary<string, object> FrontMatter { get; }

	/// <summary>
	/// The length of the ObsidianText.
	/// </summary>
	public int Length => Text.Length;

	/// <summary>
	/// Creates a new <see cref="ObsidianText"/> from the specified string.
	/// </summary>
	/// <param name="obsidianText">The Obsidian-flavoured Markdown string.</param>
	public ObsidianText(string obsidianText)
	{
		// First, check if the text starts with a YAML header, and split it off if so.
		(string? frontMatter, Text) = SplitYamlFrontMatter(obsidianText);

		// If there is a YAML header, parse it into a dictionary.
		// Otherwise, set the frontmatter to an empty dictionary.
		FrontMatter = frontMatter is null ? new() : ParseYamlFrontMatter(frontMatter);
	}

	/// <summary>
	/// Creates a new ObsidianText from the specified string.
	/// </summary>
	/// <param name="text">The text to create the ObsidianText from.</param>
	public static implicit operator ObsidianText(string text) => new(text);

	/// <summary>
	/// Creates a new ObsidianText from the specified string.
	/// </summary>
	/// <param name="text">The text to create the ObsidianText from.</param>
	public static implicit operator string(ObsidianText text) => text.Text;

	/// <summary>
	/// Returns the text of the ObsidianText.
	/// </summary>
	/// <returns>The text of the ObsidianText.</returns>
	public override string ToString() => Text;

	/// <summary>
	/// Returns the text of the ObsidianText.
	/// </summary>
	/// <returns>The text of the ObsidianText.</returns>
	public MarkupString ToHtml() => new(ObsidianHtmlConverter.Default.Convert(Text));
	
	/// <inheritdoc cref="ToHtml()"/>
	/// <param name="converter">The converter to use.</param>
	public MarkupString ToHtml(ObsidianHtmlConverter converter) => new(converter.Convert(Text));


	/// <summary>
	/// Splits the YAML front matter from the Markdown content.
	/// </summary>
	/// <param name="obsidianMarkdown">The original Obsidian Markdown, with maybe a YAML front matter at the beginning.</param>
	/// <returns>A tuple containing the YAML front matter and the remaining Obsidian Markdown content.</returns>
	private static (string? frontMatter, string content) SplitYamlFrontMatter(string obsidianMarkdown)
	{
		// The front matter is a YAML document at the beginning of the file, delimited by three dashes.
		static bool _LineDelimiterPredicate(string line) => line is "---";
		
		// We need to find the first line with three dashes, and then the next line with three dashes.
		// The content between the two lines is the front matter.
		// If there is no front matter, the content is the whole file.
		string[] lines = obsidianMarkdown.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

		int firstLine = Array.FindIndex(lines, _LineDelimiterPredicate);
		int secondLine = Array.FindIndex(lines, firstLine + 1, _LineDelimiterPredicate);

		if (firstLine is 0 && secondLine is 0)
		{
			return (null, obsidianMarkdown);
		}

		string frontMatter = string.Join(Environment.NewLine, lines[1..secondLine]);
		string content = string.Join(Environment.NewLine, lines[(secondLine + 1)..]);

		return (frontMatter, content);
	}

	/// <summary>
	/// Parses a YAML front matter into a dictionary of key-value pairs.
	/// </summary>
	/// <param name="frontMatter">The YAML front matter to parse.</param>
	/// <returns>A dictionary of key-value pairs.</returns>
	/// <exception cref="YamlException">Thrown when the YAML front matter is invalid.</exception>
	public static Dictionary<string, object> ParseYamlFrontMatter(string frontMatter) => _yamlDeserializer.Deserialize<Dictionary<string, object>>(frontMatter);
}