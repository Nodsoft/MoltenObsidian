namespace Nodsoft.MoltenObsidian.Blazor.Templates;

/// <summary>
/// Defines a base class for all template render context objects.
/// </summary>
/// <param name="DisplayOptions">Display options for the current vault display.</param>
public abstract record TemplateRenderContextBase(ObsidianVaultDisplayOptions DisplayOptions);