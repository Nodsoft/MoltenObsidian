using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
	[Inject] public IJSRuntime Js { get; set; } = null!;

	[Parameter, EditorRequired] public string? CurrentPath { get; set; } = "/";
	[Parameter, EditorRequired] public string? BasePath { get; set; } = "/";

	[Parameter] public RenderFragment<(IVault, string)> Index { get; set; } = DefaultTemplates.IndexDefaultTemplate;
	[Parameter] public RenderFragment<string> NotFound { get; set; } = DefaultTemplates.NotFoundDefaultTemplate;
	[Parameter] public RenderFragment<(IVaultNote, string)> FoundFile { get; set; } = DefaultTemplates.FoundFileDefaultTemplate;
	[Parameter] public RenderFragment<(IVaultFolder, string)> FoundFolder { get; set; } = DefaultTemplates.FoundFolderDefaultTemplate;
	[Parameter] public RenderFragment<(string, Exception)> Error { get; set; } = DefaultTemplates.ErrorDefaultTemplate;


	private IVaultEntity? _foundEntity;
	private string _currentPath = ".";
	
	protected override async Task OnParametersSetAsync()
	{
		await base.OnParametersSetAsync();
		
		_currentPath = Uri.UnescapeDataString(Navigation.ToBaseRelativePath(Navigation.Uri));

		if (CurrentPath is not null)
		{
			_foundEntity = Router.RouteTo(CurrentPath);
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await Js.InvokeVoidAsync("eval", /*lang=javascript*/$"document.querySelector('base').href = '{BasePath}'");
	}
}