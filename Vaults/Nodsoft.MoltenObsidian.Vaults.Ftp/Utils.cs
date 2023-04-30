using System.Runtime.CompilerServices;
using FluentFTP;

namespace Nodsoft.MoltenObsidian.Vaults.Ftp;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncFtpClient EnsureConnected(this AsyncFtpClient client)
    {
        if (!client.IsConnected)
        {
            client.Connect();
        }
        return client;
    }
}