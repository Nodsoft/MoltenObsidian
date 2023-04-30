using System.Runtime.CompilerServices;
using FluentFTP;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<AsyncFtpClient> EnsureConnected(this AsyncFtpClient client)
    {
        if (!client.IsConnected)
        {
            await client.Connect();
        }
        return client;
    }
}