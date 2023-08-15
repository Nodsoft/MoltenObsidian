using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Nodsoft.MoltenObsidian.Utilities;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory;

/// <summary>
/// 
/// </summary>
public sealed class InMemoryVault: IVault
{

    private InMemoryVault(){}

    private MemoryCache Cache { get; set; }
    public string Name { get; private set; } = null!;
    public IVaultFolder Root { get; private set; }
    public IReadOnlyDictionary<string, IVaultFile> Files { get; private set; }
    public IReadOnlyDictionary<string, IVaultFolder> Folders { get; private set; }
    
    public IReadOnlyDictionary<string, IVaultNote> Notes { get; private set; }
    public static IEnumerable<string> DefaultIgnoredFolders { get; } = new[] { ".obsidian", ".git", ".vs", ".vscode", "node_modules" };
    public static IEnumerable<string> DefaultIgnoredFiles { get; } = new[] { ".DS_Store" };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    [PublicAPI]
    public static InMemoryVault FromDirectory(MemoryCache cache, DirectoryInfo directoryInfo) =>
        FromDirectory(directoryInfo, DefaultIgnoredFolders, DefaultIgnoredFiles, cache);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="ignoredFolders"></param>
    /// <param name="ignoredFiles"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    [PublicAPI]
    public static InMemoryVault FromDirectory(DirectoryInfo directory,
        IEnumerable<string> ignoredFolders,
        IEnumerable<string> ignoredFiles, 
        MemoryCache cache)
    {
        if (!directory.Exists)
        {
            throw new DirectoryNotFoundException("The specified directory does not exist.");
        }

        InMemoryVault vault = new()
        {
            Cache = cache
        };

        vault.Root = new InMemoryVaultFolder(directory, null, vault);
        vault.Name = directory.Name;
        vault.Folders = new Dictionary<string, IVaultFolder>(
            vault.Root.GetFolders(SearchOption.AllDirectories));
    }
    
}