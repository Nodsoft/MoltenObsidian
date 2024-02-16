import {VaultFile, VaultManifest} from 'vault-manifest'
import YAML from 'yaml'

/**
 * Represents a client for resolving a MoltenObsidian vault's assets from its manifest.
 * @class
 * @public
 */
export class VaultManifestClient {
  protected _routingTable: Map<string, VaultFile> | undefined;
  
  public static fromManifest(manifest: VaultManifest) {
    return new VaultManifestClient(manifest);
  }

  public static async fromPath(path: string | URL) {
    // Fetch the manifest from the URL
    const response = await fetch(path);
    if (!response.ok) {
      throw new Error(`Failed to fetch manifest from ${path.toString()}: ${response.statusText}`);
    }

    const manifest = await response.json() as VaultManifest;
    return VaultManifestClient.fromManifest(manifest);
  }

  protected constructor(
    public readonly Manifest: VaultManifest
  ) { }
  
  public get routingTable() {
    if (!this._routingTable) {
      this._routingTable = this.buildRoutingTable();
    }
    
    return this._routingTable;
  }
  
  private buildRoutingTable() {
    const routingTable = new Map<string, VaultFile>();
    
    for (const file of this.Manifest.files) {
      // If markdown, add to routing table
      if (!file.path.endsWith('.md')) {
        continue;
      }
      
      // Exclusion cases
      if ((file.moltenobsidian?.publish ?? file.publish) == false) {
        continue;
      }
      
      const indexFileRegex = /(\/?index\.md|readme\.md)$/ig;
      if (file.path.match(indexFileRegex)) {
        console.log(file.path, file.path.replace(indexFileRegex, ''));
        routingTable.set(file.path.replace(indexFileRegex, ''), file);
      }
      else if (file.path.endsWith('.md')) {
        const mdRegex = /\.md$/;
        routingTable.set(file.path.replace(mdRegex, ''), file);
        // routingTable.set(file.path.replace(mdRegex, '.html'), file);
      }

      // routingTable.set(file.path, file);
    }
    
    return routingTable;
  }
  
  getSsgPath(file: { path: string }) {
    let path = file.path;
    if (path.startsWith('/')) {
      path = path.substring(1);
    }

    return path.replace(/\.md$/, '.html');
  }

  public static getFrontMatter(file: string | BinaryData) {
    if (typeof file !== 'string') {
      file = new TextDecoder().decode(file);
    }
    
    return YAML.parse(file);
  }
  
  public getFrontMatterFilePath(path: string) {
    return path.replace(/\.md$|\.html$/, '.yaml');
  }
}