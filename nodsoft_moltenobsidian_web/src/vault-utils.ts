import type { VaultFile } from "moltenobsidian-ssg-client/vault-manifest";

export function toRelativeVaultPath(path: string) {
  return `../assets/vault/${path}`;
}

export class VaultNote {
  constructor(public readonly path: string) {
    
  }
}

export function cast<T extends {[key: string]: any}, U>(obj: any, prototype: new() => T): T | null {
  if (typeof obj !== 'object' || obj === null) {
    // obj is not an object, can't proceed
    return null;
  }

  let newObj = new prototype();
  Object.keys(obj).forEach((key) => {
    (newObj as any)[key] = obj[key];
  });

  return newObj;
}

/**
* Sorts vault files by path, alphabetically ascending, per folder depth
*/
export function sortVaultFiles(a: VaultFile, b: VaultFile) : number {
  // Compare folder names, parents first
  const aFolders = a.path.split('/');
  const bFolders = b.path.split('/');
  
  for (let i = 0; i < Math.max(aFolders.length, bFolders.length); i++) {
    const aFolder = aFolders.length > i ? aFolders[i]?.toLowerCase() : null;
    const bFolder = bFolders.length > i ? bFolders[i]?.toLowerCase() : null;
    
    if (!aFolder && !bFolder) {
      // Both are null, continue
      return 0;
    }
    
    if (!aFolder) {
      // a is null, b is not, b comes first
      return -1;
    }
    
    if (!bFolder) {
      // b is null, a is not, a comes first
      return 1;
    }
    
    if (aFolder === bFolder) {
      // Both are equal, continue
      continue;
    }
    
    // Compare folder names
    return aFolder.localeCompare(bFolder);
  }
  
  // Compare file names
  return a.path.localeCompare(b.path);
}

export type LeafNode = {
  name: string,
  path: string,
  type: 'file' | 'folder',
  order?: number,
  children?: LeafNode[],
  isIndex?: boolean // True if this node is an index file
}

export function buildVaultTree(files: Array<VaultFile> | Map<string, VaultFile>) {  
  if (files instanceof Map) {
    files = Array.from(files.values());
  }
  
  const items = files.sort(sortVaultFiles);
  const tree: LeafNode = { name: '', path: '', type: 'folder', children: [] };
  
  for (const item of items) {
    // Split path into folders and file name (last item)
    const slices = item.path.split('/');
    const filename = slices.pop()!;
    const folders = slices;
    
    // Find the parent folder
    let parentFolder = tree;
    for (const folder of folders) {      
      // Find the folder in the parent folder
      let folderNode = parentFolder.children!.find((node) => node.name === folder);
      
      // If not found, create it
      if (!folderNode) {
        const folderPath = `${parentFolder.path}/${folder}`;
        
        folderNode = {
          name: folder.replace(/\.md$/i, ''),
          path: folderPath.startsWith('/') ? folderPath.substring(1) : folderPath, // Remove leading slash (if any)
          type: 'folder',
          children: []
        };

        // If order is set, add it to the folder's current order score
        if (item.order) {
          folderNode.order = (folderNode.order ?? 0) + item.order;
        }
        
        // Add the folder to the parent folder        
        parentFolder.children!.push(folderNode);
      }

      // Set the parent folder to the current folder
      parentFolder = folderNode;
    }

    // Add the file to the parent folder
    parentFolder.children!.push({
      name: filename,
      path: item.path.replace(/\/?index\.md|readme\.md$/i, ''),
      order: item.order,
      type: 'file',
      isIndex: (ciEquals(filename, 'index.md') || ciEquals(filename, 'readme.md'))
    });
  }
  
  return tree;
}

export function ciEquals(a: string, b: string): a is typeof b {
  return a.localeCompare(b, "en", {sensitivity: 'accent'}) === 0;
}