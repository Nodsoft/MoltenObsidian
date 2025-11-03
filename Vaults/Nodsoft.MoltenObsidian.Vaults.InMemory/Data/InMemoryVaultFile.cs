using System.IO.MemoryMappedFiles;
using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal class InMemoryVaultFile : InMemoryVaultEntityBase, IVaultFile, IDisposable
{
    private readonly MemoryMappedFile _file;

    protected InMemoryVaultFile(string name, InMemoryVaultFolder parent, InMemoryVault vault, Stream content) : base(name, parent, vault)
    {
        ContentType = MimeTypes.GetMimeType(fileName: name);
        ContentLength = content.Length;
        
        // Keep this MMF nameless
        // See : https://stackoverflow.com/questions/66308340/how-to-use-net-memory-mapped-file-in-linux-without-persisted-file
        _file = MemoryMappedFile.CreateNew(mapName: null, capacity: Math.Max(ContentLength, 1));

        if (content.Length > 0)
        {
            // Copy content to file
            using MemoryMappedViewStream stream = _file.CreateViewStream();
            content.CopyTo(stream);
        }
    }

    public virtual string ContentType { get; }

    private long ContentLength { get; }
    
    public ValueTask<Stream> OpenReadAsync()
    {
        MemoryMappedViewStream stream = _file.CreateViewStream(0, ContentLength);
        return new(stream);
    }
    
    public static InMemoryVaultFile Create(string name, Stream content, InMemoryVaultFolder parent, InMemoryVault vault)
        => name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            ? new InMemoryVaultNote(name, parent, vault, content)
            : new InMemoryVaultFile(name, parent, vault, content);
    
    /// <summary>
    /// Creates a new <see cref="InMemoryVaultFile"/> instance, representing a file in memory.
    /// </summary>
    /// <param name="path">The path of the file to create.</param>
    /// <param name="content">The content of the file to create.</param>
    /// <param name="parent">The parent folder of the file to create.</param>
    /// <param name="vault">The vault that the file belongs to.</param>
    /// <returns>The newly created <see cref="InMemoryVaultFile"/> instance.</returns>
    public static ValueTask<InMemoryVaultFile> WriteFileAsync(string path, Stream content, InMemoryVaultFolder parent, InMemoryVault vault)
    {
        InMemoryVaultFile file = Create(path, content, parent, vault);
        
        // Add the file to the parent folder
        if (parent.Files.All(f => f.Path != path))
        {
            parent.Files.Add(file);
            parent.AddChildReference(file);
        }
        
        return new(file);
    }
    
    public void DeleteFile()
    {
        _file.Dispose();
        Parent?.DeleteChildReference(this);
    }

    public void Dispose()
    {
        _file.Dispose();
        GC.SuppressFinalize(this);
    }
}