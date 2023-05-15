using System.ComponentModel;
using FluentFTP;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp;
using Spectre.Console;
using Spectre.Console.Cli;
using Nodsoft.MoltenObsidian.Manifest;
using System.Text.Json;
using System.Net.Http.Json;
using Nodsoft.MoltenObsidian.Vaults.Http;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using System.Text;

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
    public string? UrlScheme { get; private set; }

    [CommandOption("-o|--output-path <OUTPUT_PATH>"), Description("Directory to write the output files to")]
    public string OutputPathString { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
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

        if (!string.IsNullOrEmpty(LocalVaultPathString))
        {
            if((LocalVaultPath = new(LocalVaultPathString)) is { Exists: false })
            {
                return ValidationResult.Error($"The vault path {LocalVaultPathString} does not exist");
            }
            LocalVaultPath = new(LocalVaultPathString);
        }

        if (!string.IsNullOrEmpty(OutputPathString))
        {
            if((OutputPath = new(OutputPathString)) is { Exists: false })
            {
                return ValidationResult.Error($"The output path '{OutputPath}' does not exist.");
            }
        }

        if (!string.IsNullOrEmpty(RemoteManifestUrlString))
        {
            if (!Uri.IsWellFormedUriString(RemoteManifestUrlString, UriKind.Absolute))
            {
                return ValidationResult.Error($"The url {RemoteManifestUrlString} is not valid or is not an absolute path");
            }

            if (!RemoteManifestUrlString.EndsWith("moltenobsidian.manifest.json"))
            {
                return ValidationResult.Error($"The Url must end in moltenobsidian.manifest.json");
            }
            RemoteManifestUri = new Uri(RemoteManifestUrlString);

            if (RemoteManifestUri.GetLeftPart(UriPartial.Scheme) is not ("ftp://" or "ftps://" or "http://" or "https://"))
            {
                return ValidationResult.Error($"The url protocol must be http or ftp");
            }
            UrlScheme = RemoteManifestUri.GetLeftPart(UriPartial.Scheme);
        }

        return ValidationResult.Success();
    }

}

/// <summary>
/// Provides a command that allows you to generate static vault assets that can be used anywhere <see cref="GenerateStaticSite"/>.
/// </summary>
[UsedImplicitly]
public sealed class GenerateStaticSite: AsyncCommand<GenerateStaticSiteCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateStaticSiteCommandSettings settings)
	{
		IVault vault = await CreateReadVault(settings);

		await AnsiConsole.Status().StartAsync("Generating static assets.", async ctx =>
		{
			ctx.Status("Generating static assets.");
			ctx.Spinner(Spinner.Known.Noise);
			ctx.SpinnerStyle(Style.Parse("purple bold"));

			if (settings.DebugMode)
			{
				AnsiConsole.Console.MarkupLine(/*lang=markdown*/$"[grey]Ignoring folders:[/] {string.Join("[grey], [/]", settings.IgnoredFolders ?? new[] { "*None*" })}");
				AnsiConsole.Console.MarkupLine(/*lang=markdown*/$"[grey]Ignoring files:[/] {string.Join("[grey], [/]", settings.IgnoredFiles ?? new[] { "*None*" })}");
			}

			foreach (KeyValuePair<string, IVaultFile> pathFilePair in vault.Files)
			{
				if (IsIgnored(pathFilePair.Key, settings.IgnoredFolders, settings.IgnoredFiles))
				{
					continue;
				}

				(FileInfo fileInfo, byte[] fileData) = await CreateOutputFile(settings.OutputPath.ToString(), pathFilePair);

				if (!fileInfo.Directory.Exists)
				{
					fileInfo.Directory.Create();
				}

				await using FileStream stream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
				await stream.WriteAsync(fileData);
				await stream.FlushAsync();
			}
		});

		return 0;
	}

    /// <summary>
    /// Decide which Type of vault to create
    /// </summary>
    /// <param name="settings"><see cref="GenerateStaticSiteCommandSettings"/></param>
    /// <returns><see cref="IVault"/></returns>
    /// <exception cref="Exception"></exception>
	private async ValueTask<IVault> CreateReadVault(GenerateStaticSiteCommandSettings settings) => settings.UrlScheme switch
	{
		// todo: perhaps move this and Construct ftp/http functions into vault interface? IVault.FromUri(uri)
		"ftp://" or "ftps://" => await ConstructFtpVault(settings.RemoteManifestUri),
		"http://" or "https://" => await ConstructHttpVault(settings.RemoteManifestUri),
		null when settings.RemoteManifestUri is null => FileSystemVault.FromDirectory(settings?.LocalVaultPath),
		null when settings.RemoteManifestUri is not null => throw new Exception("malformed url to remote vault manifest"),
		_ => throw new Exception("error upon creating a vault.")
	};

	private bool IsIgnored(string path, string[]? ignoredFolders, string[]? ignoredFiles)
	{
		if (ignoredFiles.Contains(path))
        {
            return false;
        }

        if (ignoredFolders.Contains(path))
        {
            return false;
        }

        return true;
	}

	/// <summary>
	/// Creates the output file, if a <see cref="IVaultNote"></see> is encountered convert to html.
	/// <paramref name="outputPath"/>
	/// <paramref name="pathFilePair"/>
    /// <returns>A new <see cref="FileInfo"/> to be written too and a <see cref="byte[]"/> containing the file data</returns>
	/// </summary>
	private async ValueTask<(FileInfo, byte[])> CreateOutputFile(string outputPath, KeyValuePair<string, IVaultFile> pathFilePair)
	{
        string path = Path.Combine(outputPath, pathFilePair.Key.Replace('/', Path.DirectorySeparatorChar));
        byte[] fileData = await pathFilePair.Value.ReadBytesAsync();
        
        // .md -> .html happens here
        if (path.EndsWith(".md"))
        {
            path = $"{path[..^3]}.html";
            
            string html = new ObsidianText(Encoding.Default.GetString(fileData)).ToHtml();
            fileData = Encoding.ASCII.GetBytes(html);
        }

        FileInfo fileInfo =  new(path);
        return (fileInfo, fileData);
    }

    /// <summary>
    /// creates a vault from http url <see cref="HttpRemoteVault"></see>.
    /// <paramref name="uri"/>
    /// </summary>
    private async ValueTask<IVault> ConstructHttpVault(Uri uri)
    {
        HttpClient client = new() { BaseAddress = uri };
        RemoteVaultManifest manifest = await client.GetFromJsonAsync<RemoteVaultManifest>("moltenobsidian.manifest.json")
			?? throw new InvalidOperationException("Failed to retrieve the vault manifest from the server.");

		return HttpRemoteVault.FromManifest(manifest, client);
    }

    /// <summary>
    /// creates a vault from ftp url <see cref="FtpRemoteVault"></see>.
    /// <paramref name="uri"/>
    /// </summary>
    private async ValueTask<IVault> ConstructFtpVault(Uri uri)
    {
        // extracts user and pass from ftp url
        var (user, pass) = uri.UserInfo?.Split(':') is { } info ? info.Length is 2 ? (info[0], info[1]) : (info[0], "") : ("", "");

        // download manifest and use it to construct the ftpremoteovault
        AsyncFtpClient client = new AsyncFtpClient(uri.Host, user, pass, 21);
        await client.EnsureConnected();
        byte[] bytes = await client.DownloadBytes("moltenobsidian.manifest.json", CancellationToken.None);
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, client);
    }
}
