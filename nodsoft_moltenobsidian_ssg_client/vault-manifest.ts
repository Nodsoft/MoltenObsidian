/**
 * Represents a manifest for a MoltenObsidian vault.
 * @interface
 */
export interface VaultManifest {
  /**
   * The name of the vault.
   * @type {string}
   */
  name: string;

  /**
   * List of files in the vault.
   * @type {VaultFile[]}
   */
  files: VaultFile[];
}

/**
 * Represents a file in a MoltenObsidian vault manifest
 * @interface
 */
export interface VaultFile {
  /**
   * The path of the file.
   * @type {string}
   * @example "README.md"
   * @example "super/secret/file.md"
   */
  path: string;

  /**
   * The size of the file in bytes.
   * @type {number}
   */
  size: number;

  /**
   * The hash of the file.
   * @type {string}
   */
  hash: string | null;

  /**
   * The content type of the file, as a MIME type.
   * @type {string}
   * @example "text/markdown"
   */
  contentType: string | null;
}
