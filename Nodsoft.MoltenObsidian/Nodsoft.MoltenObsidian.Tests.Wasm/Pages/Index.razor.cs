using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Converter;

namespace Nodsoft.MoltenObsidian.Tests.Wasm.Pages;

public partial class Index : ComponentBase
{
	private static readonly MarkupString _convertedMarkdown = new ObsidianText(MarkdownText).ToHtml(new(new ObsidianPipelineBuilder(true).Build()));

	private const string MarkdownText = 
		/*lang=markdown*/"""
	    # Hello, world!

	    This is a sample Markdown document.  
	    And a paragraph with **bold** and *italic* text.
	    
	    ---
	    
	    ## Lists
	    
	    ### Unordered
	    
	    - Item 1
	    - Item 2
	    - Item 3
	    
	    ### Ordered
	    
	    1. Item 1
	    2. Item 2
	    3. Item 3
	    
	    ---  
	    
	    ## Code
	    
	    ```csharp
	    public class Test
	    {
	        public string Name { get; set; }
	    }
	    ```
	    
	    ---
	    
	    ## Tables
	    
	    | Header 1 | Header 2 | Header 3 |
	    | -------- | -------- | -------- |
	    | Item 1   | Item 2   | Item 3   |
	    | Item 4   | Item 5   | Item 6   |
	    | Item 7   | Item 8   | Item 9   |
	    
	    ---
	    
	    ## Links
	    
	    [MoltenObsidian](https://github.com/Nodsoft/MoltenObsidian)
	    
	    ---
	    
	    ## Images
	    
	    ![Nodsoft Systems](https://avatars.githubusercontent.com/u/5573728)
	    
	    ---
	    
	    ## Blockquotes
	    
	    > This is a blockquote.
	    >
	    > It can span multiple lines.
	    >
	    > > And it can be nested.
	    > >
	    > > > And so on...
	    
	    --- 
	    
	    ## Emojis
	    
	    :smile: :+1: :sparkles: :tada: :rocket: :metal: :octocat:
	    
	    ---
	    """;
}