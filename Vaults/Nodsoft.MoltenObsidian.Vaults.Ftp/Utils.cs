using System.Runtime.CompilerServices;
using FluentFTP;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

/// <summary>
/// Provides utility methods for the FTP vault.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Ensures that the client is connected to the server.
    /// </summary>
    public static async ValueTask<AsyncFtpClient> EnsureConnected(this AsyncFtpClient client)
    {
        if (!client.IsConnected)
        {
            await client.Connect();
        }
        return client;
    }
}