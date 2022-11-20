using System.Reflection.Metadata;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor;

/// <summary>
/// Provides a central Razor component for loading files from the vault, according to page routing.
/// </summary>
[PublicAPI]
public sealed partial class ObsidianVaultDisplay : ComponentBase
{
	[Inject] public IVault Vault { get; set; } = null!;
	[Inject] public VaultRouter Router { get; set; } = null!;
	[Inject] public NavigationManager Navigation { get; set; } = null!;

	[Parameter, EditorRequired] public string? Path { get; set; } = "/";

	[Parameter] public RenderFragment<(IVault, string)> Index { get; set; } = DefaultTemplates.IndexDefaultTemplate;
	[Parameter] public RenderFragment<string> NotFound { get; set; } = DefaultTemplates.NotFoundDefaultTemplate;
	[Parameter] public RenderFragment<(IVaultMarkdownFile, string)> FoundFile { get; set; } = DefaultTemplates.FoundFileDefaultTemplate;
	[Parameter] public RenderFragment<(IVaultFolder, string)> FoundFolder { get; set; } = DefaultTemplates.FoundFolderDefaultTemplate;
	[Parameter] public RenderFragment<(string, Exception)> Error { get; set; } = DefaultTemplates.ErrorDefaultTemplate;


	private IVaultEntity? _foundEntity;
	private string _currentPath = ".";
	
	protected override async Task OnParametersSetAsync()
	{
		await base.OnParametersSetAsync();
		
		_currentPath = Navigation.ToBaseRelativePath(Navigation.Uri);

		if (Path is not null)
		{
			_foundEntity = Router.RouteTo(Path);
		}
	}
}