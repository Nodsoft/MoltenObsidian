using System.Text.Json;
using FluentFTP;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp.Data;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
    private static RemoteVaultManifest? ToRemoteVaultManifest(this byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return JsonSerializer.Deserialize<RemoteVaultManifest>(ms);
    }

    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection,
        Func<IServiceProvider, AsyncFtpClient> ftpClientProvider)
        => serviceCollection.AddSingleton<IVault>(services => {
            AsyncFtpClient ftpClient = ftpClientProvider(services);
            var bytes = ftpClient.DownloadBytes("moltenobsidian.manifest.json", default).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException("could not retrieve the vault manifest from the server");
            var manifestBytes = bytes.ToRemoteVaultManifest();
            if (manifestBytes is null)
                throw new Exception("There was an issue reading the manifest file, ensure your ftp client is configured correctly.");
            return FtpRemoteVault.FromManifest(manifestBytes, ftpClient);
        });
}