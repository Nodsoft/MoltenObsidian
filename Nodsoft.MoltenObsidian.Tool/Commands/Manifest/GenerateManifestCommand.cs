using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
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

	public DirectoryInfo VaultPath { get; private set; } = null!;

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
	
	[CommandOption("--watch"), Description("Watches the vault for changes and regenerates the manifest.")]
	public bool Watch { get; set; }

	
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

		if (!Force && !VaultPath.GetDirectories().Any(static d => d.Name == ".obsidian"))
		{
			return ValidationResult.Error($"The vault path '{VaultPath}' does not appear to be a valid Obsidian vault.");
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
			AnsiConsole.MarkupLine( /*lang=md*/"[bold blue]Debug mode is enabled.[/]");
		}

		// This is where the magic happens.
		// We'll be using the Spectre.Console library to provide a nice CLI experience.
		// Statuses at each step, and a nice summary at the end.

		FileSystemVault vault = null!;

		// First, load the Obsidian vault. This will validate the vault and load all the files.
		AnsiConsole.Console.Status().Start("Loading vault...", _ =>
		{
			// Print the ignores if they're set.
			if (settings.DebugMode)
			{
				AnsiConsole.Console.MarkupLine(/*lang=md*/$"[grey]Ignoring folders:[/] {string.Join("[grey], [/]", settings.IgnoredFolders ?? ["*None*"])}");
				AnsiConsole.Console.MarkupLine(/*lang=md*/$"[grey]Ignoring files:[/] {string.Join("[grey], [/]", settings.IgnoreFiles ?? ["*None*"])}");
			}

			settings.IgnoredFolders ??= FileSystemVault.DefaultIgnoredFolders.ToArray();
			settings.IgnoreFiles ??= FileSystemVault.DefaultIgnoredFiles.ToArray();

			// Load the vault.
			vault = FileSystemVault.FromDirectory(settings.VaultPath, settings.IgnoredFolders, settings.IgnoreFiles);
		});
		
		AnsiConsole.MarkupLine(/*lang=md*/$"Loaded vault with [green]{vault.Files.Count}[/] files.");
		
		await GenerateManifestAsync(vault, settings.VaultPath, settings.OutputPath, settings.DebugMode, file =>
		{
			if (settings.Force || settings.Watch)
			{
				// Warn the user that the file will be overwritten.
				string forceFlagStr = settings switch
				{
					{ Force: true } => "--force",
					{ Watch: true } => "--watch",
					_ => throw new UnreachableException()
				};
				
				AnsiConsole.MarkupLine(/*lang=md*/$"[yellow]A manifest file already exists at the specified location, but [green]{forceFlagStr}[/] was specified. Overwriting.[/]");
			}
			else
			{
				// If it does, ask the user if they want to overwrite it.
				bool overwrite = AnsiConsole.Prompt(new ConfirmationPrompt(/*lang=md*/"[yellow]The manifest file already exists. Overwrite?[/]"));

				if (!overwrite)
				{
					// If they don't, abort.
					AnsiConsole.MarkupLine(/*lang=md*/"[red]Aborted.[/]");
					return false;
				}

				// If they do, delete the file.
				file.Delete();
			}

			return true;
		});
		
		if (settings.Watch)
		{
			await AnsiConsole.Console.Status().StartAsync("Watching vault for changes...", async ctx =>
			{
				// Print a status message.
				ctx.Spinner(Spinner.Known.Dots);
				ctx.SpinnerStyle(Style.Parse("purple bold"));
				
				vault.VaultUpdate += async (_, args) =>
				{
					try
					{
						if (args.Entity.Path is RemoteVaultManifest.ManifestFileName)
						{
							AnsiConsole.MarkupLine(/*lang=md*/"[grey]Manifest update detected. Ignoring...[/]");
							return;
						}
					
						// Print a status message.
						AnsiConsole.MarkupLine(/*lang=md*/$"[grey]Vault update detected (Entity name: [/]{args.Entity.Path}[grey], Change type: [/]{args.Type})");
						AnsiConsole.MarkupLine(/*lang=md*/"[blue]Regenerating manifest...[/]");
					
						// Regenerate the manifest.
						await GenerateManifestAsync(vault, settings.VaultPath, settings.OutputPath, settings.DebugMode, _ => true);
					}
					catch (Exception e)
					{
						// Print an error message.
						AnsiConsole.MarkupLine(/*lang=md*/"[red]An error occurred while regenerating the manifest:[/]");
						AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
					}
				};
				
				// Watch the vault for changes.
				await Task.Delay(-1);
			});
		}
		
		return 0;
	}

	internal static async Task<RemoteVaultManifest> GenerateManifestAsync(
		IVault vault, 
		DirectoryInfo vaultPath, 
		DirectoryInfo? outputPath, 
		bool debugMode,
		Func<FileInfo, bool> promptOverwrite
	) {
		// Assert the vault as FileSystem-Based
		if (vault is not FileSystemVault)
		{
			throw new InvalidOperationException("The vault must be a FileSystemVault to generate a manifest.");
		}
		
		// Next, generate the manifest.
		AnsiConsole.Console.MarkupLine( /*lang=md*/"Generating manifest...");
		RemoteVaultManifest manifest = await VaultManifestGenerator.GenerateManifestAsync(vault);

		AnsiConsole.MarkupLine(/*lang=md*/$"Generated manifest with [green]{manifest.Files.Count}[/] files.");
		
		// Write the manifest to disk, at the specified location (or the vault root if not specified).
		FileInfo manifestFile = new(Path.Combine((outputPath ?? vaultPath).FullName, RemoteVaultManifest.ManifestFileName));

		if (manifestFile.Exists && !promptOverwrite(manifestFile))
		{
			return manifest;
		}

		// Write the manifest to disk.
		await WriteManifestFileAsync(manifestFile, manifest, debugMode);
		return manifest;
	}

	private static async Task WriteManifestFileAsync(FileInfo file, RemoteVaultManifest manifest, bool debugMode)
	{
		AnsiConsole.MarkupLine(/*lang=md*/$"Writing manifest...");
		
		// Write the new manifest to disk.
		await using FileStream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Write);
		fs.SetLength(0);

#pragma warning disable CA1869
		await JsonSerializer.SerializeAsync(fs, manifest, new JsonSerializerOptions
		{
			WriteIndented = debugMode, // If debug mode is enabled, write the manifest with indentation.
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for property names.
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Don't write null values.
		});

		AnsiConsole.MarkupLine(/*lang=md*/$"Wrote manifest to [green link]{file.FullName}[/].");
	}
}
