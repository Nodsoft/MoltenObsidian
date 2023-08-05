import {VaultFile, VaultManifest} from 'vault-manifest'

/**
 * Represents a client for resolving a MoltenObsidian vault's assets from its manifest.
 * @class
 * @public
 */
export class VaultManifestClient {
  public static fromManifest(manifest: VaultManifest): VaultManifestClient {
    return new VaultManifestClient(manifest);
  }

  public static async fromPath(path: string | URL): Promise<VaultManifestClient> {
    // Check if the path is a string
    if (typeof path === 'string') {
      // Check if the path is an absolute URL
      if (path.startsWith('http://') || path.startsWith('https://')) {
        path = new URL(path);
      } else {
        // Assume the path is a relative path, from base URL (defined in <base> tag)
        path = new URL(path, document.baseURI ?? window.location.href);
      }
    }
        
    // Fetch the manifest from the URL
    const response = await fetch(path.toString());
    if (!response.ok) {
      throw new Error(`Failed to fetch manifest from ${path.toString()}: ${response.statusText}`);
    }

    const manifest = await response.json() as VaultManifest;
    return VaultManifestClient.fromManifest(manifest);
  }

  protected constructor(
    public readonly Manifest: VaultManifest
  ) { }
}