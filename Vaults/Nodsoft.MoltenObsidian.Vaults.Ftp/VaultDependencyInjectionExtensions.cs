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

    private static async Task<IVault> BuildFtpVaultAsync(Func<IServiceProvider, AsyncFtpClient> ftpClientProvider, IServiceProvider services)
    {
        var ftpClient = await ftpClientProvider(services).EnsureConnected();
        var bytes = await ftpClient.DownloadBytes("moltenobsidian.manifest.json", default);
        var manifestBytes = bytes.ToRemoteVaultManifest();
        return FtpRemoteVault.FromManifest(manifestBytes, ftpClient);
    }

    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection, Func<IServiceProvider, AsyncFtpClient> ftpClientProvider) =>
        serviceCollection.AddSingleton<IVault>(services => new TaskFactory()
            .StartNew(async () => await BuildFtpVaultAsync(ftpClientProvider, services))
            .Unwrap().GetAwaiter().GetResult());
}