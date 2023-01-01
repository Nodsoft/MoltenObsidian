using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nodsoft.MoltenObsidian.Blazor.Helpers;
using Nodsoft.MoltenObsidian.Blazor.Services;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Blazor;

/// <summary>
/// Provides a central Razor component for loading files from the vault, according to page routing.
/// </summary>
[PublicAPI]
public sealed partial class ObsidianVaultDisplay : ComponentBase
{
	/// <summary>
	/// The Vault being displayed.
	/// </summary>
	[Inject, Parameter] public IVault Vault { get; set; } = null!;
	
	/// <summary>
	/// The router used to navigate the vault.
	/// </summary>
	[Inject] public VaultRouter Router { get; set; } = null!;
	
	/// <summary>
	/// Blazor Navigation manager to navigate to other pages.
	/// </summary>
	[Inject] public NavigationManager Navigation { get; set; } = null!;
	
	/// <summary>
	/// Javascript runtime to interact with the browser.
	/// </summary>
	[Inject] public IJSRuntime Js { get; set; } = null!;

	
	/// <summary>
	/// The current path being navigated.
	/// </summary>
	/// <remarks>
	/// This is usually bound from the slug parameter of the blazor page hosting this component.
	/// </remarks>
	[Parameter, EditorRequired] public string? CurrentPath { get; set; } = "/";
	
	/// <summary>
	/// The base path from which the vault is being served.
	/// </summary>
	/// <remarks>
	/// This is usually bound from the base parameter of the blazor page hosting this component.
	/// In most cases, it can be resolved using <see cref="VaultComponentHelpers.GetCallingBaseVaultPath"/>.
	/// </remarks>
	[Parameter, EditorRequired] public string? BasePath { get; set; } = "/";
	
	/// <summary>
	/// The options to use for rendering the vault.
	/// </summary>
	[Parameter] public ObsidianVaultDisplayOptions Options { get; set; } = new();

	/// <summary>
	/// The render fragment in charge of rendering the vault's index.
	/// </summary>
	/// <value>
	///	By default, this is set to <see cref="Templates.FoundVaultIndex"/>, which renders a list of all entities in the vault.
	/// </value>
	/// <seealso cref="IVault"/>
	[Parameter] public RenderFragment<IVault> Index { get; set; } = Templates.FoundVaultIndex.Render;
	
	/// <summary>
	/// The render fragment used when nothing is found at the current path.
	/// </summary>
	[Parameter] public RenderFragment<string> NotFound { get; set; } = Templates.NotFound.Render;
	
	/// <summary>
	/// The render fragment used when hitting a Note on the current path.
	/// </summary>
	/// <value>
	/// By default, this is set to <see cref="DefaultTemplates.FoundNoteDefaultTemplate"/>, which renders the note's content.
	/// </value>
	/// <seealso cref="IVaultNote"/>
	[Parameter] public RenderFragment<IVaultNote> FoundNote { get; set; } = Templates.FoundNote.Render;
	
	/// <summary>
	/// The render fragment used when hitting a Folder on the current path.
	/// </summary>
	/// <value>
	/// By default, this is set to <see cref="DefaultTemplates.FoundFolderDefaultTemplate"/>, which renders a list of all immediate descending entities in the folder.
	/// </value>
	/// <seealso cref="IVaultFolder"/>
	[Parameter] public RenderFragment<IVaultFolder> FoundFolder { get; set; } = Templates.FoundFolder.Render;
	
	/// <summary>
	/// The render fragment used when an error occurs.
	/// </summary>
	[Parameter] public RenderFragment<(string, Exception)> Error { get; set; } = Templates.Error.Render;


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
		if (firstRender)
		{
			await Js.InvokeVoidAsync("eval", /*lang=javascript*/$"document.querySelector('base').href = '{BasePath}'");
		}
	}
}