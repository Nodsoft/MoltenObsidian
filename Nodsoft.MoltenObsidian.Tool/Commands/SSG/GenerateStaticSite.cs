using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FluentFTP;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp;
using Spectre.Console;
using Spectre.Console.Cli;
using Nodsoft.MoltenObsidian.Manifest;
using System.Text.Json;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
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

    [CommandOption("-o|--output-path <OUTPUT_PATH>"), Description("Directory to write the output files to")]
    public string OutputPathString { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public DirectoryInfo OutputPath { get; private set; } = null!;

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
		IVault vault = await CreateReadVaultAsync(settings);

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

			string[] ignoredFiles = settings.IgnoredFiles ?? Array.Empty<string>();
			string[] ignoredFolders = settings.IgnoredFolders ?? Array.Empty<string>();
			
			foreach (KeyValuePair<string, IVaultFile> pathFilePair in vault.Files)
			{
				if (IsIgnored(pathFilePair.Key, ignoredFolders, ignoredFiles))
				{
					continue;
				}

				(FileInfo fileInfo, byte[] fileData) = await CreateOutputFile(settings.OutputPath.ToString(), pathFilePair);

				if (!fileInfo.Directory!.Exists)
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
	private static async ValueTask<IVault> CreateReadVaultAsync(GenerateStaticSiteCommandSettings settings) => settings switch
	{
		// Remote Vaults
		{ RemoteManifestUri: { Scheme: "ftp" or "ftps" } manifestUri } => await ConstructFtpVaultAsync(manifestUri),
		{ RemoteManifestUri: { Scheme: "http" or "https" } manifestUri } => await ConstructHttpVaultAsync(manifestUri),
		
		// Local Vaults
		{ LocalVaultPath: { Exists: true } vaultPath } => FileSystemVault.FromDirectory(vaultPath),
		
		// Invalid
		_=> throw new ArgumentException($"Failed to create vault, both {nameof(settings.RemoteManifestUri)} and {nameof(settings.LocalVaultPath)} are null.", nameof(settings))
	};

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsIgnored(string path, IEnumerable<string> ignoredFolders, IEnumerable<string> ignoredFiles) 
		=> !(ignoredFiles.Contains(path) || ignoredFolders.Contains(path));

	/// <summary>
	/// Creates the output file, if a <see cref="IVaultNote"></see> is encountered convert to html.
	/// <paramref name="outputPath"/>
	/// <paramref name="pathFilePair"/>
    /// <returns>A new <see cref="FileInfo"/> to be written to and a buffer containing the file data.</returns>
	/// </summary>
	private static async ValueTask<(FileInfo, byte[])> CreateOutputFile(string outputPath, KeyValuePair<string, IVaultFile> pathFilePair)
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
    /// creates a vault from http url <see cref="HttpRemoteVault" />.
    /// </summary>
    private static async ValueTask<IVault> ConstructHttpVaultAsync(Uri uri)
    {
        HttpClient client = new() { BaseAddress = uri };
        
        RemoteVaultManifest manifest = await client.GetFromJsonAsync<RemoteVaultManifest>(RemoteVaultManifest.ManifestFileName, CancellationToken.None)
			?? throw new InvalidOperationException("Failed to retrieve the vault manifest from the server.");

		return HttpRemoteVault.FromManifest(manifest, client);
    }

    /// <summary>
    /// creates a vault from ftp url <see cref="FtpRemoteVault" />.
    /// </summary>
    private static async ValueTask<IVault> ConstructFtpVaultAsync(Uri uri)
    {
        // extracts user and pass from ftp url
        (string user, string pass) = uri.UserInfo.Split(':') is { } info ? info.Length is 2 ? (info[0], info[1]) : (info[0], "") : ("", "");

        // download manifest and use it to construct the ftpremotevault
        AsyncFtpClient client = new(uri.Host, user, pass, 21);
        
        await client.EnsureConnected();
        byte[] bytes = await client.DownloadBytes(RemoteVaultManifest.ManifestFileName, CancellationToken.None);
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, client);
    }
}
