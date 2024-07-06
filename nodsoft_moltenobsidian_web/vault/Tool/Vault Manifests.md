# Vault Manifest management

## Premise
Some remote vaults do not support directory listing. As such, one efficient way we've overcome this issue is by creating a conventional file called a Vault Manifest, tasked with listing all files in a vault, along with the path, size, SHA256 checksum, and any frontmatter associated within the file.

This is a process that is fully automated using the CLI Tool, and that we plan to also export to a GitHub Actions CI step in the near future.

## Usage
Creating a manifest goes as follows:
```sh  
moltenobsidian manifest generate "/path/to/vault/root/"
```  

The given path will then be checked for the existence of the `./.obsidian/` folder, indicative of the presence of an Obsidian Vault. While the creation of a manifest is not contingent on the presence of this folder (as it will be ignored anyways), it is preferred, to ensure that an invalid path was not specified.  
Nonetheless, if you're adamant on the location, you can bypass the checks by running the same command with the `-f|--force` argument, which will force the creation of a manifest, regardless of that validating folder's presence.

Similarly, there may be cases where you need to output the manifest to a separate folder. In these edge cases, specifying the `-o|--output <output-folder>` argument will allow you to output the manifest in a different folder (all while retaining the conventionally contingent `moltenobsidian.manifest.json` filename).

Finally, if the default list of excluded folders/files is not sufficient, you can overwrite the list using the `--exclude-folder` and `--exclude-file` arguments. These can be invoked multiple times in the same command, like so:
```sh  
moltenobsidian manifest generate "/path/to/vault/root" --exclude-folder ".obsidian" --exclude-folder ".git" --exclude-folder ".github"
```  
```sh  
moltenobsidian manifest generate "/path/to/vault/root" --exclude-file "my/secret/document.md" --exclude-file "secrets.json"
```  

For reference, these are the default exclusions:

| **Entity Type** | **Exclusions**                                    |
| --------------- | ------------------------------------------------- |
| **Folders**     | `.obsidian` `.git` `.vs` `.vscode` `node_modules` |
| **Files**       | `.DS_STORE`                                       |

### Developer features
Some features of the manifest command are specifically oriented for development and automation purposes. 
Here is a detailed account of some of the extra features baked into this command.

| Flag      | Description                                                              |
| --------- | ------------------------------------------------------------------------ |
| `--force` | Forces any existing manifest found at the output path to be overwritten. |
| `--watch` | Continuously watches for changes and updates the manifest accordingly.   |
| `--debug` | Prints out extra information, similar to a verbose flag.                 |
