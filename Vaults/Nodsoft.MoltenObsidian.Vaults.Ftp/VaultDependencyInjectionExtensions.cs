using System.Text.Json;
using FluentFTP;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp;

namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
    private static RemoteVaultManifest? ToRemoteVaultManifest(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return JsonSerializer.Deserialize<RemoteVaultManifest>(ms);
    }

    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection,
        Func<IServiceProvider, AsyncFtpClient> ftpClientProvider) =>
        serviceCollection.AddSingleton<IVault>(services =>
        {
            using var ftpClient = ftpClientProvider(services);
            try
            {
                ftpClient.Connect().GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            var bytes = ftpClient.DownloadBytes("moltenobsidian.manifest.json", default).GetAwaiter().GetResult()
                        ?? throw new InvalidOperationException("could not retrieve the vault manifest from the server");
            var manifestBytes = bytes.ToRemoteVaultManifest();
            ftpClient.Disconnect().GetAwaiter().GetResult();
            return FtpRemoteVault.FromManifest(manifestBytes, ftpClient);
        });
}