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
    public string OutputPathString { get; private set; } = string.Empty;
    public DirectoryInfo? OutputPath { get; private set; }

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

[UsedImplicitly]
public sealed class GenerateStaticSite: AsyncCommand<GenerateStaticSiteCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateStaticSiteCommandSettings settings)
    {
        IVault vault = settings.UrlScheme switch
        {
            "ftp://" or "ftps://" => await ConstructFtpVault(settings),
            "http://" or "https://" => await ConstructHttpVault(settings),
            null when settings.RemoteManifestUri is null => FileSystemVault.FromDirectory(settings?.LocalVaultPath),
            null when settings.RemoteManifestUri is not null => throw new Exception("malformed url to remote vault manifest"),
            _ => throw new Exception("error upon creating a vault.")
        };

        foreach (KeyValuePair<string, IVaultFile> pathNotePair in vault.Files)
        {
            string path = Path.Combine(settings?.OutputPath?.ToString(), pathNotePair.Key.Replace('/', Path.DirectorySeparatorChar));
            byte[] fileData = await pathNotePair.Value.ReadBytesAsync();
            if (path.EndsWith(".md"))
            {
                fileData =  Encoding.ASCII.GetBytes(new ObsidianText(Encoding.Default.GetString(fileData)).ToHtml());
                path = $"{path[..^3]}.html";
            }
            FileInfo fileInfo =  new(path);

            
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            await using FileStream stream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write);
            await stream.WriteAsync(fileData);
            await stream.FlushAsync();
        }

        return 0;
    }

    private async Task<IVault> ConstructHttpVault(GenerateStaticSiteCommandSettings settings)
    {
        HttpClient client = new() { BaseAddress = settings.RemoteManifestUri };
        RemoteVaultManifest manifest = await client.GetFromJsonAsync<RemoteVaultManifest>("moltenobsidian.manifest.json")
			?? throw new InvalidOperationException("Failed to retrieve the vault manifest from the server.");

		return HttpRemoteVault.FromManifest(manifest, client);
    }

    private async Task<IVault> ConstructFtpVault(GenerateStaticSiteCommandSettings settings)
    {
        var uri = settings?.RemoteManifestUri;
        var (user, pass) = uri.UserInfo?.Split(':') is { } info ? info.Length is 2 ? (info[0], info[1]) : (info[0], "") : ("", "");
        AsyncFtpClient client = new AsyncFtpClient(uri.Host, user, pass, 21);
        await client.EnsureConnected();
        byte[] bytes = await client.DownloadBytes("moltenobsidian.manifest.json", CancellationToken.None);
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, client);
    }
}
