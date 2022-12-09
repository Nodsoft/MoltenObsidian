using System.ComponentModel;
using System.Text.Json;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Tool.Services;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.Http.Common;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Nodsoft.MoltenObsidian.Tool.Commands.Manifest;

/// <summary>
/// Specifies the command line arguments for the <see cref="GenerateManifestCommand"/>.
/// </summary>
[PublicAPI]
public sealed class GenerateManifestSettings : CommandSettings
{
	[CommandArgument(0, "<VAULT_PATH>"), Description("The path to the Molten Obsidian vault to generate a manifest for.")]
	public string VaultPathStr { get; set; } = string.Empty;

	public DirectoryInfo VaultPath { get; private set; }

	[CommandOption("-o|--output <OUTPUT_PATH>"), Description("The path to the output folder to write the manifest to.")]
	public string OutputPathStr { get; set; } = string.Empty;
	
	public DirectoryInfo? OutputPath { get; private set; }

	[CommandOption("--force"), Description("Forces the manifest to be generated even if vault validation fails.")]
	public bool Force { get; set; }

	public override ValidationResult Validate()
	{
		if (VaultPathStr is "" || (VaultPath = new(VaultPathStr)) is { Exists: false })
		{
			return ValidationResult.Error($"The vault path '{VaultPath}' does not exist.");
		}

		if (OutputPathStr is not (null or "") && (OutputPath = new(OutputPathStr)) is { Exists: false })
		{
			return ValidationResult.Error($"The output path '{OutputPath}' does not exist.");
		}

		if (!Force)
		{
			if (!VaultPath.GetDirectories().Any(static d => d.Name == ".obsidian"))
			{
				return ValidationResult.Error($"The vault path '{VaultPath}' does not appear to be a valid Obsidian vault.");
			}
		}

		return ValidationResult.Success();
	}
}

/// <summary>
/// Provides a command that generates a manifest for a Molten Obsidian vault.
/// </summary>
[UsedImplicitly]
public sealed class GenerateManifestCommand : AsyncCommand<GenerateManifestSettings>
{
	public override async Task<int> ExecuteAsync(CommandContext context, GenerateManifestSettings settings)
	{
		// This is where the magic happens.
		// We'll be using the Spectre.Console library to provide a nice CLI experience.
		// Statuses at each step, and a nice summary at the end.

		FileSystemVault vault = null!;
		
		// First, load the Obsidian vault. This will validate the vault and load all the files.
		await AnsiConsole.Console.Status().StartAsync("Loading vault...", async _ =>
		{
			// Load the vault.
			vault = FileSystemVault.FromDirectory(settings.VaultPath);
		});
		
		AnsiConsole.MarkupLine(/*lang=markdown*/$"Loaded vault with [green]{vault.Files.Count}[/] files.");

		// Next, generate the manifest.
		RemoteVaultManifest manifest = null!;
		await AnsiConsole.Console.Status().StartAsync("Generating manifest...", async ctx =>
		{
			// Generate the manifest.
			manifest = await VaultManifestGenerator.GenerateManifestAsync(vault);
		});
		
		AnsiConsole.MarkupLine(/*lang=markdown*/$"Generated manifest with [green]{manifest.Files.Count}[/] files.");
		
		// Finally, write the manifest to disk, at the specified location (or the vault root if not specified).
		FileInfo manifestFile = null!;
		
		await AnsiConsole.Console.Status().StartAsync("Writing manifest...", async ctx =>
		{
			manifestFile = settings.OutputPath is null
				? new(Path.Combine(settings.VaultPath.FullName, RemoteVaultManifest.ManifestFileName))
				: new(Path.Combine(settings.OutputPath.FullName, RemoteVaultManifest.ManifestFileName));

			await using FileStream stream = manifestFile.Open(FileMode.OpenOrCreate, FileAccess.Write);
			await JsonSerializer.SerializeAsync(stream, manifest);
		});
		
		AnsiConsole.MarkupLine(/*lang=markdown*/$"Wrote manifest to [green link]{manifestFile.FullName}[/].");
		return 0;
	}
}