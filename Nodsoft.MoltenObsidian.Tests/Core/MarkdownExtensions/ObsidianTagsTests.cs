using JetBrains.Annotations;
using Markdig;
using Nodsoft.MoltenObsidian.Infrastructure.Markdown;

namespace Nodsoft.MoltenObsidian.Tests.Core.MarkdownExtensions;

/*
 * See: https://help.obsidian.md/Editing+and+formatting/Tags#Tag+format
 */

/// <summary>
/// Provides tests for the Obsidian Tags parser and renderer.
/// </summary>
[Collection("ObsidianTags")]
public sealed class ObsidianTagsTests
{
    private readonly MarkdownPipeline _pipeline;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ObsidianTagsTests"/> class.
    /// </summary>
    public ObsidianTagsTests()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseObsidianTags()
            .Build();
    }
    
    [Pure]
    private static string GetHtmlTagSnippet(string tagName) => /*lang=html*/$"<span class=\"tag moltenobsidian-tag\" data-name=\"{tagName.ToLowerInvariant()}\">{tagName}</span>";

    /// <summary>
    /// Provides valid tags for the Obsidian Tags tests.
    /// </summary>
    public static TheoryData<string, string> ValidTags => new()
    {
        { "#tag", GetHtmlTagSnippet("tag") },
        { "#tag-name", GetHtmlTagSnippet("tag-name") },
        { "#tag_name", GetHtmlTagSnippet("tag_name") },
        { "#123tag", GetHtmlTagSnippet("123tag") },
        { "#tag/with/slashes", GetHtmlTagSnippet("tag/with/slashes") },
        { "#tag/with/slashes-and-dashes", GetHtmlTagSnippet("tag/with/slashes-and-dashes") },
        { "#tag/with/slashes-and-dashes_and_underscores", GetHtmlTagSnippet("tag/with/slashes-and-dashes_and_underscores") },
        { "#tag/with/slashes-and-dashes_and_underscores_and_numbers-123", GetHtmlTagSnippet("tag/with/slashes-and-dashes_and_underscores_and_numbers-123") },
        { "#tag/with/slashes-and-dashes_and_underscores_and_numbers-123/and/uppercase/ABC", GetHtmlTagSnippet("tag/with/slashes-and-dashes_and_underscores_and_numbers-123/and/uppercase/ABC") },
    };
    
    /// <summary>
    /// Tests that a valid tag is correctly parsed and rendered.
    /// </summary>
    /// <param name="markdown">The Markdown to parse.</param>
    /// <param name="expectedHtml">The expected HTML output.</param>
    [Theory, MemberData(nameof(ValidTags))]
    public void ParseAndRender_Standalone_Nominal(string markdown, string expectedHtml)
    {
        string result = Markdown.ToHtml(markdown, _pipeline);
        Assert.Equal(/*lang=html*/$"<p>{expectedHtml}</p>\n", result);
    }
    
    /// <summary>
    /// Provides invalid tags for the Obsidian Tags tests.
    /// </summary>
    public static TheoryData<string, string> InvalidTags => new()
    {
        { "#tag with spaces", $"{GetHtmlTagSnippet("tag")} with spaces" },
        { "#1337", "#1337" }
    };
    
    /// <summary>
    /// Tests that an invalid tag is correctly parsed and rendered.
    /// </summary>
    /// <param name="markdown">The Markdown to parse.</param>
    /// <param name="expectedHtml">The expected HTML output.</param>
    [Theory, MemberData(nameof(InvalidTags))]
    public void ParseAndRender_Standalone_Ignored(string markdown, string expectedHtml)
    {
        string result = Markdown.ToHtml(markdown, _pipeline);
        Assert.Equal(/*lang=html*/$"<p>{expectedHtml}</p>\n", result);
    }
    
    /// <summary>
    /// Tests that a tag is correctly parsed and rendered when it is part of a larger text block.
    /// </summary>
    /// <param name="markdown">The Markdown to parse.</param>
    /// <param name="expectedHtml">The expected HTML output.</param>
    [Theory, MemberData(nameof(ValidTags))]
    public void ParseAndRender_InTextBlock_Nominal(string markdown, string expectedHtml)
    {
        string result = Markdown.ToHtml($"This is a test {markdown} of tags.", _pipeline);
        Assert.Equal($"<p>This is a test {expectedHtml} of tags.</p>\n", result);
    }
    
    /// <summary>
    /// Tests that an invalid tag is correctly parsed and rendered when it is part of a larger text block.
    /// </summary>
    /// <param name="markdown">The Markdown to parse.</param>
    /// <param name="expectedHtml">The expected HTML output.</param>
    [Theory, MemberData(nameof(InvalidTags))]
    public void ParseAndRender_InTextBlock_Ignored(string markdown, string expectedHtml)
    {
        string result = Markdown.ToHtml($"This is a test {markdown} of tags.", _pipeline);
        Assert.Equal($"<p>This is a test {expectedHtml} of tags.</p>\n", result);
    }
}

