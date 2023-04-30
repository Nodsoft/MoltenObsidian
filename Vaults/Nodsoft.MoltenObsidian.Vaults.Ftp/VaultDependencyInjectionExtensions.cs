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
    private static async Task<IVault> BuildFtpVaultAsync(Func<IServiceProvider, AsyncFtpClient> ftpClientProvider, IServiceProvider services)
    {
        AsyncFtpClient ftpClient = await ftpClientProvider(services).EnsureConnected();
        byte[] bytes = await ftpClient.DownloadBytes("moltenobsidian.manifest.json", CancellationToken.None) 
            ?? throw new InvalidOperationException("Could not download manifest.");
        RemoteVaultManifest? manifest = JsonSerializer.Deserialize<RemoteVaultManifest>(bytes);

        return FtpRemoteVault.FromManifest(manifest, ftpClient);
    }

    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection, Func<IServiceProvider, AsyncFtpClient> ftpClientProvider)
    {
        serviceCollection.AddSingleton<IVault>(services => new TaskFactory()
            .StartNew(async () => await BuildFtpVaultAsync(ftpClientProvider, services))
            .Unwrap().GetAwaiter().GetResult());
        
        return serviceCollection;
    }
}