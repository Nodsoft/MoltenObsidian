using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Nodsoft.MoltenObsidian.Converter;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor.Helpers.Components;


/// <summary>
/// Provides a helper component for rendering the content of a note.
/// </summary>
[PublicAPI]
public class RenderNoteContent : ComponentBase
{
    /// <summary>
    /// The note to render content for.
    /// </summary>
    [Parameter, EditorRequired]
    public IVaultNote Note { get; set; } = null!;

    /// <summary>
    /// The content to render.
    /// </summary>
    private MarkupString ContentMarkup { get; set; }

    /// <summary>
    /// The obsidian document being rendered.
    /// </summary>
    public ObsidianText Document { get; protected set; }

    /// <summary>
    /// Fires when the component finished loading the Note as an Obsidian Markdown document.
    /// </summary>
    /// <value>An <see cref="EventCallback{TValue}"/> loaded with the markup string.</value>
    [Parameter]
    public EventCallback<ObsidianText> DocumentLoaded { get; set; }

    /// <summary>
    /// Obsidian HTML converter to use for rendering.
    /// </summary>
    [Parameter]
    public ObsidianHtmlConverter Converter { get; set; } = ObsidianHtmlConverter.Default;

    /// <summary>
    /// Determines whether the component should render the content.
    /// </summary>
    [Parameter]
    public virtual Func<RenderNoteContent, bool> ShouldRenderContent { get; set; } = static _ => true;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, ContentMarkup);

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        ObsidianText document = await Note.ReadDocumentAsync();
        await OnDocumentLoadedAsync(Document = document);

        ContentMarkup = new(document.ToHtml(Converter));
    }

    /// <inheritdoc cref="ComponentBase.StateHasChanged" />
    public new void StateHasChanged()
    {
        base.StateHasChanged();
    }

    /// <inheritdoc />
    protected override bool ShouldRender() => ShouldRenderContent(this);

    /// <summary>
    /// Fires the <see cref="DocumentLoaded"/> event.
    /// </summary>
    /// <param name="document">The document that was loaded.</param>
    protected async ValueTask OnDocumentLoadedAsync(ObsidianText document) => await DocumentLoaded.InvokeAsync(document);
}