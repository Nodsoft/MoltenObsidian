using System.Text.Json;
using FluentFTP;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
    private static RemoteVaultManifest? ToRemoteVaultManifest(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return JsonSerializer.Deserialize<RemoteVaultManifest>(ms);
    }

    private static async Task<IVault> BuildFtpVaultAsync(Func<IServiceProvider, AsyncFtpClient> ftpClientProvider, IServiceProvider services)
    {
        using var ftpClient = ftpClientProvider(services);
        try
        {
           await ftpClient.Connect();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
        var bytes = await ftpClient.DownloadBytes("moltenobsidian.manifest.json", default)
                    ?? throw new InvalidOperationException("could not retrieve the vault manifest from the server");
        var manifestBytes = bytes.ToRemoteVaultManifest();
        return FtpRemoteVault.FromManifest(manifestBytes, ftpClient);
    }

    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection, Func<IServiceProvider, AsyncFtpClient> ftpClientProvider) =>
        serviceCollection.AddSingleton<IVault>(services => new TaskFactory()
            .StartNew(async () => await BuildFtpVaultAsync(ftpClientProvider, services))
            .Unwrap().GetAwaiter().GetResult());
}