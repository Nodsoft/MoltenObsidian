﻿using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Vault;
using Nodsoft.MoltenObsidian.Vaults.Http.Data;

namespace Nodsoft.MoltenObsidian.Vaults.Http;

/// <summary>
/// Defines a remotely-accessible MoltenObisidan vault, via HTTP.
/// </summary>
public sealed class HttpRemoteVault : IVault
{
	internal RemoteVaultManifest Manifest { get; private init; }
	
	internal HttpClient HttpClient { get; private init; }

	private HttpRemoteVault() { }

	/// <inheritdoc />
	public string Name { get; set; }

	/// <inheritdoc />
	public IVaultFolder Root => _root; 
	private HttpRemoteFolder _root;

	/// <inheritdoc />
	public IReadOnlyDictionary<string, IVaultFolder> Folders => _folders;
	private readonly Dictionary<string, IVaultFolder> _folders = [];

	/// <inheritdoc />
	public IReadOnlyDictionary<string, IVaultFile> Files => _files;
	private readonly Dictionary<string, IVaultFile> _files = [];

	/// <inheritdoc />
	public IReadOnlyDictionary<string, IVaultNote> Notes => _notes;
	private readonly Dictionary<string, IVaultNote> _notes = [];
	
	
	/// <inheritdoc />
	/// <remarks>
	/// There is no change detection implemented within the HTTP Vault (yet).
	/// </remarks>
	public event IVault.VaultUpdateEventHandler? VaultUpdate;
	
	
	/// <summary>
	/// Builds a new Vault from a remote manifest.
	/// </summary>
	/// <param name="manifest">The manifest to build the vault from.</param>
	/// <param name="client">The HTTP client to use for remote requests.</param>
	/// <returns></returns>
	public static HttpRemoteVault FromManifest(RemoteVaultManifest manifest, HttpClient client)
	{
		// Instantiate the vault.
		HttpRemoteVault vault = new()
		{
			Manifest = manifest,
			HttpClient = client,
		};

		vault._root = HttpRemoteFolder.FromRoot(manifest.Name, vault);
		
		// Iterate through the manifest and add the files and folders listed.
		foreach (ManifestFile manifestFile in manifest.Files)
		{
			if (manifestFile.Path.Split('/') is not [.. var folderParts, var fileName]) continue;

			IVaultFolder? currentFolder = vault._root;
			IVaultFolder parentFolder = vault._root;

			for (int i = 0; i < folderParts.Length; i++)
			{
				string pathPart = string.Join('/', folderParts.Take(i + 1));

				if (!vault._folders.TryGetValue(pathPart, out currentFolder))
				{
					currentFolder = new HttpRemoteFolder(pathPart, parentFolder);
					((HttpRemoteFolder)parentFolder).AddSubfolder((HttpRemoteFolder)currentFolder);
					vault._folders.Add(pathPart, currentFolder);
				}
				
				parentFolder = currentFolder;
			}

			HttpRemoteFile file = HttpRemoteFile.FromManifest(manifestFile, fileName, currentFolder);
			((HttpRemoteFolder)currentFolder).AddFile(file);
			vault._files.Add(manifestFile.Path, file);

			if (file.Path.EndsWith(".md"))
			{
				vault._notes.Add(file.Path, (IVaultNote) file);
			}
		}

		return vault;
	}
}