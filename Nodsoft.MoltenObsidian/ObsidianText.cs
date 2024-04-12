using System.Text;
using Nodsoft.MoltenObsidian.Converter;
using Nodsoft.MoltenObsidian.Vault;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Nodsoft.MoltenObsidian;

/// <summary>
/// Represents a string of Obsian-flavoured Markdown text.
/// </summary>
[PublicAPI]
public readonly record struct ObsidianText
{
	/// <summary>
	/// The Obsidian vault file that this text was loaded from.
	/// </summary>
	private readonly IVaultNote? _vaultFile;

	/// <summary>
	/// Creates a new <see cref="ObsidianText"/> from the specified string.
	/// </summary>
	/// <param name="obsidianText">The Obsidian-flavoured Markdown string.</param>
	/// <param name="vaultFile">The Obsidian vault file that this text was loaded from.</param>
	public ObsidianText(string obsidianText, IVaultNote? vaultFile = null)
	{
		_vaultFile = vaultFile;
		
		// First, check if the text starts with a YAML header, and split it off if so.
		(string? frontmatter, Text) = SplitYamlFrontMatter(obsidianText);

		// If there is a YAML header, parse it into a dictionary.
		// Otherwise, set the frontmatter to an empty dictionary.
		Frontmatter = frontmatter is null ? [] : ParseYamlFrontMatter(frontmatter);
	}


	/// <summary>
	/// YAML deserializer used to parse the YAML frontmatter, if present.
	/// </summary>
	private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.Build();
	
	/// <summary>
	/// The Obsidian-flavoured Markdown text.
	/// </summary>
	public string Text { get; }
	
	/// <summary>
	/// The YAML header (frontmatter) found at the beginning of the text,
	/// parsed into a key-value dictionary.
	/// </summary>
	public Dictionary<string, object> Frontmatter { get; }

	/// <summary>
	/// The length of <see cref="Text"/>.
	/// </summary>
	public int Length => Text.Length;
	
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
	public string ToHtml() => _vaultFile is null 
		? ObsidianHtmlConverter.Default.Convert(Text) 
		: ObsidianHtmlConverter.Default.Convert(Text, _vaultFile);
	
	/// <inheritdoc cref="ToHtml()"/>
	/// <param name="converter">The converter to use.</param>
	public string ToHtml(ObsidianHtmlConverter converter) => _vaultFile is null 
		? converter.Convert(Text) 
		: converter.Convert(Text, _vaultFile);


	/// <summary>
	/// Splits the YAML front matter from the Markdown content.
	/// </summary>
	/// <param name="obsidianMarkdown">The original Obsidian Markdown, with maybe a YAML front matter at the beginning.</param>
	/// <returns>A tuple containing the YAML front matter and the remaining Obsidian Markdown content.</returns>
	private static (string? frontMatter, string content) SplitYamlFrontMatter(string obsidianMarkdown)
	{
		// We need to find the first line with three dashes, and then the next line with three dashes.
		// The content between the two lines is the front matter.
		// If there is no front matter, the content is the whole file.
		
		string[] lines = SplitByNewline(obsidianMarkdown);
//		string[] lines = obsidianMarkdown.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
		TrimBom(ref lines[0]);
		
		int firstLine = Array.FindIndex(lines, _LineDelimiterPredicate);
		int secondLine = Array.FindIndex(lines, firstLine + 1, _LineDelimiterPredicate);

		if (firstLine is not 0 || secondLine is -1)
		{
			return (null, obsidianMarkdown);
		}

		string frontMatter = string.Join(Environment.NewLine, lines[1..secondLine]);
		string content = string.Join(Environment.NewLine, lines[(secondLine + 1)..]);

		return (frontMatter, content);

		// The front matter is a YAML document at the beginning of the file, delimited by three dashes.
		static bool _LineDelimiterPredicate(string line) => line is "---";
	}

	/// <summary>
	/// Parses a YAML front matter into a dictionary of key-value pairs.
	/// </summary>
	/// <param name="frontMatter">The YAML front matter to parse.</param>
	/// <returns>A dictionary of key-value pairs.</returns>
	/// <exception cref="YamlException">Thrown when the YAML front matter is invalid.</exception>
	public static Dictionary<string, object> ParseYamlFrontMatter(string frontMatter) => YamlDeserializer.Deserialize<Dictionary<string, object>>(frontMatter);
	
	private static void TrimBom(scoped ref string text)
	{
		if (text.Length is not 0 && text[0] is '\uFEFF')
		{
			text = text[1..];
		}
	}
	
	/// <summary>
	/// Splits the specified string by newlines, handling first \r\n, then \r, then \n.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string[] SplitByNewline(ReadOnlySpan<char> input)
	{
		var result = new List<string>();

		int previousIndex = 0;
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] is '\n' || (input[i] is '\r' && (i == input.Length - 1 || input[i + 1] is not '\n')))
			{
				result.Add(input[previousIndex..i].ToString());

				previousIndex = i + 1;
			}
			else if (input[i] is '\r' && i < input.Length - 1 && input[i + 1] is '\n')
			{
				result.Add(input[previousIndex..i].ToString());

				previousIndex = i + 2;
				i++;
			}
		}

		if (previousIndex < input.Length)
		{
			result.Add(input[previousIndex..].ToString());
		}

		return result.ToArray();
	}
}