using FluentFTP;
using Nodsoft.MoltenObsidian.Tool.Commands.SSG;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;
using YamlDotNet.Serialization;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vaults.Http;
using Nodsoft.MoltenObsidian.Vaults.Ftp;
using System.Diagnostics.Contracts;

namespace Nodsoft.MoltenObsidian.Tool.Services;

public static class StaticSiteGenerator
{
    private static ISerializer YamlSerializer { get; } = new SerializerBuilder()
        .WithNewLine(Environment.NewLine)
        .Build();

    /// <summary>
    /// Decide which Type of vault to create
    /// </summary>
    /// <param name="settings"><see cref="GenerateStaticSiteCommandSettings"/></param>
    /// <returns><see cref="IVault"/></returns>
    /// <exception cref="Exception"></exception>
    public static async ValueTask<IVault> CreateReadVaultAsync(GenerateStaticSiteCommandSettings settings) => settings switch
    {
        // Remote Vaults
        { RemoteManifestUri: { Scheme: "ftp" or "ftps" } manifestUri } => await ConstructFtpVaultAsync(manifestUri),
        { RemoteManifestUri: { Scheme: "http" or "https" } manifestUri } => await ConstructHttpVaultAsync(manifestUri),

        // Local Vaults
        { LocalVaultPath: { Exists: true } vaultPath } => FileSystemVault.FromDirectory(vaultPath),

        // Invalid
        _ => throw new ArgumentException($"Failed to create vault, both {nameof(settings.RemoteManifestUri)} and {nameof(settings.LocalVaultPath)} are null.", nameof(settings))
    };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIgnored(string path, IEnumerable<string> ignoredFolders, IEnumerable<string> ignoredFiles)
        => ignoredFiles.Contains(path) || ignoredFolders.Contains(path);

    /// <summary>
    /// Creates the output file, if a <see cref="IVaultNote"></see> is encountered convert to html.
    /// <paramref name="outputDir"/>
    /// <paramref name="inputFile"/>
    /// <returns>A new <see cref="FileInfo"/> to be written to and a buffer containing the file data.</returns>
    /// </summary>
    public static async ValueTask<List<InfoDataPair>> CreateOutputFiles(string outputDir, KeyValuePair<string, IVaultFile> inputFile)
    {
        // hack same directory seperator on all OS's
        inputFile = new KeyValuePair<string, IVaultFile>(inputFile.Key.Replace('/', Path.DirectorySeparatorChar), inputFile.Value);
        // save extension for later
        string extension = Path.GetExtension(inputFile.Key);
        // outputDir + new file name, without extension for easy processing
        string path = Path.Combine(outputDir, inputFile.Key[..^extension.Length]);
        List<InfoDataPair> outputList = new List<InfoDataPair>();

        return inputFile switch
        {
            // .md -> .html conversion here
            (_, IVaultNote) note when await HasFrontMatter((IVaultNote)note.Value) =>
                new() {
                    new(new(path + ".yaml"), Encoding.ASCII.GetBytes(YamlSerializer.Serialize((await ((IVaultNote) note.Value).ReadDocumentAsync()).Frontmatter))),
                    new(new(path + ".html"), Encoding.ASCII.GetBytes((await ((IVaultNote)note.Value).ReadDocumentAsync()).ToHtml()))
                },
            (_, IVaultNote) note when !await HasFrontMatter((IVaultNote)note.Value) =>
                new()
                {
                    new(new(path + ".html"), Encoding.ASCII.GetBytes((await ((IVaultNote)note.Value).ReadDocumentAsync()).ToHtml()))
                },
            // all other files
            (_, IVaultFile) vaultFile =>
                new()
                {
                    new(new(path + extension), await vaultFile.Value.ReadBytesAsync())
                }
         };
    }

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask<bool> HasFrontMatter(IVaultNote file)
    {
        ObsidianText text = await file.ReadDocumentAsync();
        return text.Frontmatter is { Count: not 0 };
    }


    /// <summary>
    /// creates a vault from http url <see cref="HttpRemoteVault" />.
    /// </summary>
    private static async ValueTask<IVault> ConstructHttpVaultAsync(Uri uri)
    {
        if (!uri.IsFile)
        {
            uri = new(uri, RemoteVaultManifest.ManifestFileName);
        }

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

        object value = await client.EnsureConnected();
        byte[] bytes = await client.DownloadBytes(RemoteVaultManifest.ManifestFileName, CancellationToken.None);
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, client);
    }
}
