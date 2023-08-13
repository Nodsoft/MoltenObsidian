
export function toRelativeVaultPath(path: string) {
  return `../assets/vault/${path}`;
}

export class VaultNote {
  constructor(public readonly path: string) {
    
  }
}