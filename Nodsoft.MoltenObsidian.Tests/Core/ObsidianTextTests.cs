namespace Nodsoft.MoltenObsidian.Tests.Core;

/// <summary>
/// Provides tests for the <see cref="ObsidianText"/> struct.
/// </summary>
/// <seealso cref="ObsidianText"/>
[Collection(nameof(ObsidianText))]
public sealed class ObsidianTextTests
{
    private const string PlainText = "This is a plain text string.";

    private const string MarkdownText = 
        """
        # This is a Markdown header

        This is a Markdown paragraph.
        """;

    private const string MarkdownFrontmatterText = 
        """
        ---
        title: This is a title
        date: 2021-09-01
        ---
        
        """ + MarkdownText;

    /// <summary>
    /// Tests the HTML parsing of an <see cref="ObsidianText"/> from a plain text string.
    /// </summary>
    [Fact]
    public void ToHtml_PlainText_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(PlainText);

        // Act
        string html = obsidianText.ToHtml();

        // Assert
        Assert.Equal("<p>This is a plain text string.</p>\n", html);
    }
    
    /// <summary>
    /// Tests the HTML parsing of an <see cref="ObsidianText"/> from a Markdown string.
    /// </summary>
    [Fact]
    public void ToHtml_Markdown_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownText);

        // Act
        string html = obsidianText.ToHtml();

        // Assert
        Assert.Equal(
            "<h1 id=\"this-is-a-markdown-header\">This is a Markdown header</h1>\n" +
            "<p>This is a Markdown paragraph.</p>\n", html
        );
    }
    
    /// <summary>
    /// Tests the HTML parsing of an <see cref="ObsidianText"/> from a Markdown string with frontmatter.
    /// </summary>
    [Fact]
    public void ToHtml_MarkdownWithFrontmatter_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownFrontmatterText);

        // Act
        string html = obsidianText.ToHtml();

        // Assert
        Assert.Equal(
            "<h1 id=\"this-is-a-markdown-header\">This is a Markdown header</h1>\n" +
            "<p>This is a Markdown paragraph.</p>\n", html
        );
    }
    
    /// <summary>
    /// Tests the HTML parsing of an <see cref="ObsidianText"/> from an empty string.
    /// </summary>
    [Fact]
    public void ToHtml_EmptyText_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new("");

        // Act
        string html = obsidianText.ToHtml();

        // Assert
        Assert.Equal("", html);
    }
    
    /// <summary>
    /// Tests the HTML parsing of an <see cref="ObsidianText"/> that contains a BOM.
    /// </summary>
    [Fact]
    public void ToHtml_BomText_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new('\uFEFF' + MarkdownText);

        // Act
        string html = obsidianText.ToHtml();

        // Assert
        Assert.Equal(
            "<h1 id=\"this-is-a-markdown-header\">This is a Markdown header</h1>\n" +
            "<p>This is a Markdown paragraph.</p>\n", html
        );
    }
    
    /// <summary>
    /// Tests the retrieval of Frontmatter from an <see cref="ObsidianText"/> with frontmatter.
    /// </summary>
    [Fact]
    public void GetFrontmatter_MarkdownWithFrontmatter_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownFrontmatterText);

        // Act
        Dictionary<string, object> frontmatter = obsidianText.Frontmatter;

        // Assert
        Assert.Equal(2, frontmatter.Count);
        Assert.Equal("This is a title", frontmatter["title"]);
        Assert.Equal(new DateTime(2021, 9, 1).ToString("yyyy-MM-dd"), frontmatter["date"]);
    }
    
    /// <summary>
    /// Tests the retrieval of Frontmatter from an <see cref="ObsidianText"/> without frontmatter.
    /// </summary>
    [Fact]
    public void GetFrontmatter_MarkdownWithoutFrontmatter_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownText);

        // Act
        Dictionary<string, object> frontmatter = obsidianText.Frontmatter;

        // Assert
        Assert.Empty(frontmatter);
    }
    
    /// <summary>
    /// Tests the retrieval of Frontmatter from an <see cref="ObsidianText"/> with an empty frontmatter.
    /// </summary>
    [Fact]
    public void GetFrontmatter_EmptyFrontmatter_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(
            """
            ---
            ---
            """ + MarkdownText
        );

        // Act
        Dictionary<string, object> frontmatter = obsidianText.Frontmatter;

        // Assert
        Assert.Empty(frontmatter);
    }
    
    /// <summary>
    /// Tests the string representation of an <see cref="ObsidianText"/> with a Markdown string.
    /// </summary>
    [Fact]
    public void ToString_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownText);

        // Act
        string text = obsidianText.ToString();

        // Assert
        Assert.Equal(MarkdownText, text);
    }
    
    /// <summary>
    /// Tests the string representation of an <see cref="ObsidianText"/> with a frontmatter Markdown string.
    /// </summary>
    [Fact]
    public void ToString_Frontmatter_Nominal()
    {
        // Arrange
        ObsidianText obsidianText = new(MarkdownFrontmatterText);

        // Act
        string text = obsidianText.ToString();

        // Assert
        Assert.Equal(MarkdownText, text);
    }
}