using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Converter;

namespace Nodsoft.MoltenObsidian;

/// <summary>
/// Represents a string of Obsian-flavoured Markdown text.
/// </summary>
public readonly record struct ObsidianText
{
	/// <summary>
	/// The text of the ObsidianText.
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// The length of the ObsidianText.
	/// </summary>
	public int Length => Text.Length;

	/// <summary>
	/// Creates a new ObsidianText from the specified string.
	/// </summary>
	/// <param name="text">The text to create the ObsidianText from.</param>
	public ObsidianText(string text)
	{
		Text = text;
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

	public bool Equals(ObsidianText other) => Text.Equals(other.Text);
}