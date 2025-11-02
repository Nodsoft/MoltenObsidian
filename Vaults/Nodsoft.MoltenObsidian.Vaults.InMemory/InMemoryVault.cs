using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory;

/// <summary>
/// Represents an in-memory vault.
/// </summary>
public sealed class InMemoryVault : IWritableVault
{
    private readonly ConcurrentDictionary<string, IVaultFolder> _folders = [];
    private readonly ConcurrentDictionary<string, IVaultFile> _files = [];
    private readonly ConcurrentDictionary<string, IVaultNote> _notes = [];
    
    /// <summary>
    /// Indicates whether the vault is currently in setup mode.
    /// </summary>
    /// <remarks>
    /// When in setup mode, no events are raised for changes to the vault.
    /// </remarks>
    public bool Setup { get; set; }
    
    /// <inheritdoc />
    public string Name { get; private set; }
    
    /// <inheritdoc />
    public IVaultFolder Root { get; private set; }
    
    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultFile> Files => _files;
    
    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;
    
    /// <inheritdoc />
    public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;
    
    /// <inheritdoc />
    public event IVault.VaultUpdateEventHandler? VaultUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryVault"/> class with a specified name and cache.
    /// </summary>
    /// <param name="name">The name of the vault.</param>
    /// <param name="setup">Indicates whether the vault is in setup mode.</param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    [PublicAPI]
    public InMemoryVault(string name, bool setup = false) 
    {
        Name = name;
        Setup = setup;
        Root = new InMemoryVaultFolder("/", null, this);
    }
    
    /// <inheritdoc />
    public async ValueTask<IVaultFolder> CreateFolderAsync(string path)
    {
        // Small starters : Strip the leading slash if present
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }
		
        // First checks : Is this a valid path?
        if (path is null or "" or "/")
        {
            throw new ArgumentException("The specified path is invalid.", nameof(path));
        }

        // Second checks : Does the folder already exist?
        if (Folders.TryGetValue(path, out IVaultFolder? existing))
        {
            return existing;
        }
        
        // Find the furthest folder that exists, and create the rest.
        string[] segments = path.Split('/');
        int furthestDepth = 0;
        IVaultFolder furthestFolder = Root;
		
        for (int i = 0; i < segments.Length; i++)
        {
            string currentPath = string.Join('/', segments[..i]);
			
            if (!Folders.TryGetValue(currentPath, out IVaultFolder? folder))
            {
                break;
            }

            furthestDepth = i;
            furthestFolder = folder;
        }
		
        // Now we need to create the rest of the folders
        for (int i = furthestDepth; i < segments.Length; i++)
        {
            string currentPath = string.Join('/', segments[..^i]);
			
            // Create the folder
            furthestFolder = InMemoryVaultFolder.CreateFolder(currentPath, (InMemoryVaultFolder)furthestFolder, this);
            _folders.TryAdd(currentPath, furthestFolder);
        }

        if (!Setup && VaultUpdate is not null)
        {
            // Raise the vault update event
            await VaultUpdate.Invoke(this, new(UpdateType.Add, furthestFolder));
        }
        
        return furthestFolder;
    }

    /// <inheritdoc />
    public async ValueTask DeleteFolderAsync(string path)
    {
        // Small step: Strip the leading slash if present
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }

        // First checks : Is this a valid path?
        if (path is null or "")
        {
            throw new ArgumentException("The specified path is invalid.", nameof(path));
        }

        // Second checks : Does the folder exist?
        if (!_folders.TryRemove(path, out IVaultFolder? folder))
        {
            throw new DirectoryNotFoundException("The specified directory does not exist inside the instantiated vault.");
        }

        // Cascade delete any downstream files
        _TraverseRemoveDownstream(folder);

        // Delete the folder
        ((InMemoryVaultFolder)folder).DeleteFolder();

        if (!Setup && VaultUpdate is not null)
        {
            // Raise the vault update event
            await VaultUpdate.Invoke(this, new(UpdateType.Remove, folder));
        }
        
        return;

        void _TraverseRemoveDownstream(IVaultFolder f)
        {
            foreach (IVaultFolder subfolder in f.Subfolders)
            {
                _TraverseRemoveDownstream(subfolder);
            }

            foreach (IVaultFile file in f.Files)
            {
                _files.Remove(file.Path, out _);
            }
        }
    }


    /// <inheritdoc />
    public async ValueTask<IVaultFile> WriteFileAsync(string path, Stream content)
    {
        // Small step: Strip the leading slash if present
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }
		
        // First checks : Is this a valid path?
        if (path is null or "")
        {
            throw new ArgumentException("The specified path is invalid.", nameof(path));
        }
        
        // Does the parent folder exist? If not, create it.
        // Luckily, we can use the CreateFolderAsync method for this.
        IVaultFolder parent = path.Contains('/') ? await CreateFolderAsync(path[..path.LastIndexOf('/')]) : Root;
		
        // Write to file
        InMemoryVaultFile created = await InMemoryVaultFile.WriteFileAsync(path[(path.LastIndexOf('/') + 1)..], content, (InMemoryVaultFolder)parent, this);
        _files.TryAdd(path, created);
		
        if (created is IVaultNote note)
        {
            _notes.TryAdd(path, note);
        }
		
        if (!Setup && VaultUpdate is not null)
        {
            // Raise the vault update event
            await VaultUpdate.Invoke(this, new(UpdateType.Add, created));
        }
        
        return created;
    }

    /// <inheritdoc />
    public async ValueTask DeleteFileAsync(string path)
    {
        // Small step: Strip the leading slash if present
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }
		
        // First checks : Is this a valid path?
        if (path is null or "")
        {
            throw new ArgumentException("The specified path is invalid.", nameof(path));
        }

        // Second checks : Does the file exist?
        if (!_files.TryRemove(path, out IVaultFile? file))
        {
            throw new FileNotFoundException("The specified file does not exist inside the instantiated vault.");
        }
		
        // Also remove from notes if it's a note
        if (file is IVaultNote)
        {
            _notes.TryRemove(path, out _);
        }
		
        // Delete the file
        ((InMemoryVaultFile)file).DeleteFile();
        
        if (!Setup && VaultUpdate is not null)
        {
            // Raise the vault update event
            await VaultUpdate.Invoke(this, new(UpdateType.Remove, file));
        }
    }

    /// <inheritdoc />
    public async ValueTask<IVaultNote> WriteNoteAsync(string path, Stream content)
    {
        // This is just a wrapper around CreateFileAsync with a check for the extension.
        if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified path does not point to a Markdown file.", nameof(path));
        }
		
        return (IVaultNote)await WriteFileAsync(path, content);
    }

    /// <inheritdoc />
    public async ValueTask DeleteNoteAsync(string path)
    {
        // This is just a wrapper around DeleteFileAsync with a check for the extension.
        if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The specified path does not point to a Markdown file.", nameof(path));
        }
		
        await DeleteFileAsync(path);
    }
}