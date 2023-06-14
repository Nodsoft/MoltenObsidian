using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Tool.Services;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Manifest;
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

	[CommandOption("-f|--force"), Description("Forces the manifest to be generated even if vault validation fails.")]
	public bool Force { get; set; }
	
	[CommandOption("--ignore-folder <IGNORED_FOLDER>"), Description("Sets a folder to be ignored when generating the manifest. Can be called multiple times.")]
	public string[]? IgnoredFolders { get; set; }
	
	[CommandOption("--ignore-file <IGNORED_FILE>"), Description("Sets a file to be ignored when generating the manifest. Can be called multiple times.")]
	public string[]? IgnoreFiles { get; set; }
	
	[CommandOption("--debug", IsHidden = true)]
	public bool DebugMode { get; set; }

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
		if (settings.DebugMode)
		{
			// Inform the user that we're in debug mode.
			AnsiConsole.MarkupLine("[bold blue]Debug mode is enabled.[/]");
		}
		
		// This is where the magic happens.
		// We'll be using the Spectre.Console library to provide a nice CLI experience.
		// Statuses at each step, and a nice summary at the end.

		FileSystemVault vault = null!;
		
		// First, load the Obsidian vault. This will validate the vault and load all the files.
		await AnsiConsole.Console.Status().StartAsync("Loading vault...", async _ =>
		{
			// Print the ignores if they're set.
			if (settings.DebugMode)
			{
				AnsiConsole.Console.MarkupLine(/*lang=markdown*/$"[grey]Ignoring folders:[/] {string.Join("[grey], [/]", settings.IgnoredFolders ?? new[] { "*None*" })}");
				AnsiConsole.Console.MarkupLine(/*lang=markdown*/$"[grey]Ignoring files:[/] {string.Join("[grey], [/]", settings.IgnoreFiles ?? new[] { "*None*" })}");
			}
			
			

			settings.IgnoredFolders ??= FileSystemVault.DefaultIgnoredFolders.ToArray();
			settings.IgnoreFiles ??= FileSystemVault.DefaultIgnoredFiles.ToArray();

			// Load the vault.
			vault = FileSystemVault.FromDirectory(settings.VaultPath, settings.IgnoredFolders, settings.IgnoreFiles);
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
		
		
		manifestFile = settings.OutputPath is null
			? new(Path.Combine(settings.VaultPath.FullName, RemoteVaultManifest.ManifestFileName))
			: new(Path.Combine(settings.OutputPath.FullName, RemoteVaultManifest.ManifestFileName));

		// Check if the file already exists.
		if (manifestFile.Exists)
		{
			if (settings.Force)
			{
				// Warn the user that the file will be overwritten.
				AnsiConsole.MarkupLine(/*lang=markdown*/"[yellow]A manifest file already exists at the specified location, but [green]--force[/] was specified. Overwriting.[/]");
			}
			else
			{
				// If it does, ask the user if they want to overwrite it.
				bool overwrite = AnsiConsole.Prompt(new ConfirmationPrompt(/*lang=markdown*/"[yellow]The manifest file already exists. Overwrite?[/]"));
			
				if (!overwrite)
				{
					// If they don't, abort.
					AnsiConsole.MarkupLine("[red]Aborted.[/]");
					return 1;
				}
			
				// If they do, delete the file.
				manifestFile.Delete();
			}
		}

		await AnsiConsole.Console.Status().StartAsync("Writing manifest...", async ctx =>
		{
			// Write the new manifest to disk.
			await using FileStream stream = manifestFile.Open(FileMode.OpenOrCreate, FileAccess.Write);
			stream.SetLength(0);

			await JsonSerializer.SerializeAsync(stream, manifest, new JsonSerializerOptions
			{
				WriteIndented = settings.DebugMode, // If debug mode is enabled, write the manifest with indentation.
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for property names.
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Don't write null values.
			});
		});
		
		AnsiConsole.MarkupLine(/*lang=markdown*/$"Wrote manifest to [green link]{manifestFile.FullName}[/].");
		return 0;
	}
}