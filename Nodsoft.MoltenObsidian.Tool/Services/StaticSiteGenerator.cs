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
		_=> throw new ArgumentException($"Failed to create vault, both {nameof(settings.RemoteManifestUri)} and {nameof(settings.LocalVaultPath)} are null.", nameof(settings))
	};

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsIgnored(string path, IEnumerable<string> ignoredFolders, IEnumerable<string> ignoredFiles) 
		=> ignoredFiles.Contains(path) || ignoredFolders.Contains(path);

	/// <summary>
	/// Creates the output file, if a <see cref="IVaultNote"></see> is encountered convert to html.
	/// <paramref name="outputPath"/>
	/// <paramref name="pathFilePair"/>
    /// <returns>A new <see cref="FileInfo"/> to be written to and a buffer containing the file data.</returns>
	/// </summary>
	public static async ValueTask<List<InfoDataPair>> CreateOutputFiles(string outputPath, KeyValuePair<string, IVaultFile> pathFilePair)
	{
        string path = Path.Combine(outputPath, pathFilePair.Key.Replace('/', Path.DirectorySeparatorChar));
        byte[] fileData = await pathFilePair.Value.ReadBytesAsync();
        List<InfoDataPair> outputList = new List<InfoDataPair>();
        
        // convert markdown files to html here
        if (path.EndsWith(".md"))
        {
            (path, fileData, byte[] frontMatterData) = MarkDownToHtml(path, fileData);
            if (frontMatterData is not { Length: 0 })
            {
                FileInfo frontMatterInfo = new(Path.Combine(outputPath, Path.GetFileNameWithoutExtension(path) + ".frontmatter.yaml"));
                outputList.Add(new(frontMatterInfo, frontMatterData));
            }
        }

        FileInfo fileInfo = new(path);
        outputList.Add(new(fileInfo, fileData));

        return outputList;
    }

    /// <summary>
    /// Converts markdown files to static html files
    /// </summary>
    /// <param name="path">The file path of the new file</param>
    /// <param name="fileData">The file data to modify</param>
    private static (string, byte[], byte[]) MarkDownToHtml(string path, byte[] fileData)
    {
        path = $"{path[..^3]}.html"; // change file name to html
        ObsidianText text = new ObsidianText(Encoding.Default.GetString(fileData));
        byte[] frontMatter = Encoding.ASCII.GetBytes(new SerializerBuilder()
            .WithNewLine(Environment.NewLine).Build()
            .Serialize(text.Frontmatter));

        fileData = Encoding.ASCII.GetBytes(text.ToHtml());
        return (path, fileData, frontMatter);
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

        object value = await client.EnsureConnected();
        byte[] bytes = await client.DownloadBytes(RemoteVaultManifest.ManifestFileName, CancellationToken.None);
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, client);
    }
}
