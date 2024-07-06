# Markdown Conversion

Converting Obsidian-flavoured markdown to HTML in a standalone manner is a very straightforward task, using MoltenObsidian. Dealing with a standalone markdown file, nothing more than the base [`Nodsoft.MoltenObsidian`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian) library is needed. You won't be importing entire vaults, merely declaring your markdown text, and that's it.

## The Basics

It all starts with the [`ObsidianText`](https://github.com/Nodsoft/MoltenObsidian/blob/main/Nodsoft.MoltenObsidian/ObsidianText.cs) object.  
Using its `.ToHtml()` method provides a turnkey solution for rendering Markdown to HTML using sensible defaults similar to the parsing/rendering rules found in Obsidian. 
```csharp
using Nodsoft.MoltenObsidian;

// Create a new ObsidianText instance with the content to convert
ObsidianText markdown = new(@"
# This is a Markdown header  
  
This is a Markdown paragraph.
");

// Convert to HTML
string html = markdown.ToHtml()

// Our `html` string then equates to this :
/*
	<h1 id="this-is-a-markdown-header">This is a Markdown header</h1>  
	<p>This is a Markdown paragraph.</p>
*/
```

## Customizing the converter
MoltenObsidian uses [Markdig](https://github.com/xoofx/markdig/) as its Markdown engine. This allows us to provide downstream developers with great extensibility when it comes to Markdown parsing and rendering. On top of [implementing its own specs](https://github.com/xoofx/markdig/tree/master/src/Markdig.Tests/Specs) (some of which aren't implemented within Obsidian), it allows us to provide the appropriate extensions for any Obsidian-specific syntax, such as [Internal Links](https://help.obsidian.md/Linking+notes+and+files/Internal+links) and [Tags](https://help.obsidian.md/Editing+and+formatting/Tags).

However, there are instances where a downstream developer may want to further customize the pipeline, to add, remove, or alter certain conversion elements. 

Internally, all conversion done in `ObsidianText` instances is handled by the Default [`ObsidianHtmlConverter`](https://github.com/Nodsoft/MoltenObsidian/blob/main/Nodsoft.MoltenObsidian/Converter/ObsidianHtmlConverter.cs), which wraps around a `MarkdownPipeline`. Building this pipeline from Obsidian defaults can be achieved using the [`ObsidianPipelineBuilder`](https://github.com/Nodsoft/MoltenObsidian/blob/main/Nodsoft.MoltenObsidian/Converter/ObsidianPipelineBuilder.cs), which provides the base defaults for the entire library.  
Downstream developers wishing to customize further their converter whilst keeping in sync with future MoltenObsidian releases are encouraged to use the `ObsidianPipelineBuilder` as baseline for their custom pipeline needs.

> [!NOTE] 
> Internal Links support is only enabled on `ObsidianText` instances supplied by a vault, due to cross-references.  
> Without the vault validating an internal link, it will be ignored on rendering.

## Frontmatter parsing
Per Obsidian's markdown specifications, any markdown text can be prefaced with a [YAML](https://yaml.org) Frontmatter section (dubbed [Properties](https://help.obsidian.md/Editing+and+formatting/Properties) by Obsidian in newer versions) :
```
---
name: "Example"
description: "Instead of describing this note, i'll tell you a secret : This will never be displayed!"
---

# This is a Markdown header  
  
This is a Markdown paragraph.
```

This Frontmatter section is omitted from HTML render, however is retained under `ObsidianText.Frontmatter`, which is a `Dictionary<string, object>` property. This allows any markdown note to pass metadata about its content, and allows MoltenObsidian to control a note's behaviour in vaults. 