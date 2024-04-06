using System.Text.Json;
using FluentFTP;
using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Ftp;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the Molten Obsidian FTP vault within the dependency injection container.
/// </summary>
[PublicAPI]
public static class VaultDependencyInjectionExtensions
{
    private static async Task<IVault> BuildFtpVaultAsync(Func<IServiceProvider, AsyncFtpClient> ftpClientProvider, IServiceProvider services)
    {
        AsyncFtpClient ftpClient = await ftpClientProvider(services).EnsureConnected();
        byte[] bytes = await ftpClient.DownloadBytes("moltenobsidian.manifest.json", CancellationToken.None) 
            ?? throw new InvalidOperationException("Could not download manifest.");
        

        return FtpRemoteVault.FromManifest(manifest, ftpClient);
    }

    /// <summary>
    /// Adds the Molten Obsidian FTP vault to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the vault to.</param>
    /// <param name="ftpClientProvider">A function that provides an FTP client.</param>
    /// <returns>The service collection with the vault added.</returns>
    public static IServiceCollection AddMoltenObsidianFtpVault(this IServiceCollection serviceCollection, Func<IServiceProvider, AsyncFtpClient> ftpClientProvider)
    {
        serviceCollection.AddSingleton<IVault>(services => new TaskFactory()
            .StartNew(async () => await BuildFtpVaultAsync(ftpClientProvider, services))
            .Unwrap().GetAwaiter().GetResult());
        
        return serviceCollection;
    }
}