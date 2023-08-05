using System.ComponentModel;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vault;
using Spectre.Console;
using Spectre.Console.Cli;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Tool.Services;

namespace Nodsoft.MoltenObsidian.Tool.Commands.SSG;

/// <summary>
/// Specifies the command line arguments for the <see cref="GenerateStaticSite"/>.
/// </summary>
[PublicAPI]

public sealed class GenerateStaticSiteCommandSettings : CommandSettings
{
    [CommandOption("--from-folder <PATH_TO_VAULT>"), Description("Path to the local moltenobsidian vault")]
    public string LocalVaultPathString { get; private set; } = string.Empty;
    public DirectoryInfo? LocalVaultPath { get; private set; }

    [CommandOption("--from-url <PATH_TO_MANIFEST>"), Description("Full url to the manifest file for remote vault")]
    public string RemoteManifestUrlString { get; private set; } = string.Empty;
    public Uri? RemoteManifestUri { get; private set; }

    [CommandOption("-o|--output-path <OUTPUT_PATH>"), Description("Directory to write the output files to")]
    public string OutputPathString { get; private set; } = string.Empty;
    public DirectoryInfo? OutputPath { get; private set; }

    /// <summary>
    /// Prints Ignored folders, input and output directory
    /// </summary>
    [CommandOption("--debug", IsHidden = true)]
    public bool DebugMode { get; set;  } = false;

    [CommandOption("--ignored-files <IGNONRED_FOLDER>"), Description("Ignore these files when creating the static site.")]
    public string[]? IgnoredFiles { get; private set; } = FileSystemVault.DefaultIgnoredFiles.ToArray();
    
    [CommandOption("--ignored-folders <IGNORED_FILES>"), Description("Ignore an entire directoroy when creating the static site.")]
    public string[]? IgnoredFolders { get; private set; } = FileSystemVault.DefaultIgnoredFolders.ToArray();

    public override ValidationResult Validate()
    {
        if (!string.IsNullOrEmpty(LocalVaultPathString) && !string.IsNullOrEmpty(RemoteManifestUrlString))
        {
            return ValidationResult.Error("--from-url and --from-folder options cannot be used together");
        }

        if (LocalVaultPathString is not (null or "") && (LocalVaultPath = new(LocalVaultPathString)) is { Exists: false })
        {
	        return ValidationResult.Error($"The vault path {LocalVaultPathString} does not exist");
        }

        if (OutputPathString is not (null or "") && (OutputPath = new(OutputPathString)) is { Exists: false })
        {
	        return ValidationResult.Error($"The output path '{OutputPath}' does not exist.");
        }
        
        OutputPath ??= new(Environment.CurrentDirectory);

        if (!string.IsNullOrEmpty(RemoteManifestUrlString))
        {
	        if (!Uri.IsWellFormedUriString(RemoteManifestUrlString, UriKind.Absolute))
            {
                return ValidationResult.Error($"The url {RemoteManifestUrlString} is not valid or is not an absolute path");
            }
            
            RemoteManifestUri = new(RemoteManifestUrlString);
            
            if (RemoteManifestUri.Scheme is not ("ftp" or "ftps" or "http" or "https"))
            {
				return ValidationResult.Error("The url protocol must be http or ftp");
            }
        }

        return ValidationResult.Success();
    }

}

/// <summary>
/// Provides a command that allows you to generate static vault assets that can be used anywhere <see cref="GenerateStaticSite"/>.
/// </summary>
[UsedImplicitly]
public sealed class GenerateStaticSite : AsyncCommand<GenerateStaticSiteCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateStaticSiteCommandSettings settings)
	{
		IVault vault = await StaticSiteGenerator.CreateReadVaultAsync(settings);

		await AnsiConsole.Status().StartAsync("Generating static assets.", async ctx =>
		{
			ctx.Status("Generating static assets.");
			ctx.Spinner(Spinner.Known.Noise);
			ctx.SpinnerStyle(Style.Parse("purple bold"));

			if (settings.DebugMode)
			{
				AnsiConsole.Console.MarkupLine(/*lang=md*/$"[grey]Ignoring folders:[/] {string.Join("[grey], [/]", settings.IgnoredFolders ?? new[] { "*None*" })}");
				AnsiConsole.Console.MarkupLine(/*lang=md*/$"[grey]Ignoring files:[/] {string.Join("[grey], [/]", settings.IgnoredFiles ?? new[] { "*None*" })}");

				AnsiConsole.Console.MarkupLine(settings.OutputPath is null
					? /*lang=md*/$"[grey]Output path defaulted to current directory: [/]{Environment.CurrentDirectory}"
					: /*lang=md*/$"[grey]Output path set: [/]{settings.OutputPath}"
				);
			}

			string[] ignoredFiles = settings.IgnoredFiles ?? Array.Empty<string>();
			string[] ignoredFolders = settings.IgnoredFolders ?? Array.Empty<string>();
			
			foreach (KeyValuePair<string, IVaultFile> pathFilePair in vault.Files)
			{
				if (StaticSiteGenerator.IsIgnored(pathFilePair.Key, ignoredFolders, ignoredFiles))
				{
					continue;
				}

				List<InfoDataPair> fileData = await StaticSiteGenerator.CreateOutputFilesAsync(settings.OutputPath!.ToString(), pathFilePair);
                await Task.WhenAll(fileData.Select(WriteDataAsync));
			}
			
            AnsiConsole.Console.MarkupLine(/*lang=md*/$"Wrote static files to [green link]{settings.OutputPath}[/].");
        });

		return 0;
	}

    private static async Task WriteDataAsync(InfoDataPair pair)
    {
	    if (!pair.FileInfo.Directory!.Exists)
	    {
		    pair.FileInfo.Directory.Create();
	    }
	    await using FileStream stream = pair.FileInfo.Open(FileMode.Create, FileAccess.Write);
	    await stream.WriteAsync(pair.FileData);
	    await stream.FlushAsync();

    }
}
