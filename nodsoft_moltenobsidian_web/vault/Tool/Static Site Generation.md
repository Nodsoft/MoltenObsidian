# Static Site Generation

## Premise
Most developers require a solution to export their Obsidian vault to HTML, when integrating in apps based outside of ASP.NET Core, like JavaScript frameworks.
This feature allows for a turnkey solution in exporting a local or remote Obsidian vault to a set of HTML and YAML files, along with all elements proper to the Obsidian syntax as well-defined as necessary.

> [!TIP]
> The content of this very website you are browsing now, [is rendered using our SSG feature](https://github.com/Nodsoft/MoltenObsidian/blob/main/.github/workflows/deploy-astro-website.yml), and integrated into [our Astro website](https://github.com/Nodsoft/MoltenObsidian/tree/main/nodsoft_moltenobsidian_web).  
> A perfect example of MoltenObsidian working with Astro, a JavaScript framework.

## Usage 
Exporting a local MoltenObsidian vault to a specified directory goes as follows :
```sh  
moltenobsidian ssg generate --from-folder "/path/to/local/vault" -o "/destination/directory"
```  

The `ssg` command also supports exporting remote vaults from HTTP or FTP :
```sh  
moltenobsidian ssg generate --from-url "https://url.to/remote/vault/moltenobsidian.manifest.json" -o "/destination/directory"
moltenobsidian ssg generate --from-url "ftp://url.to/remote/vault/moltenobsidian.manifest.json" -o "/destination/directory"
```

> [!NOTE]
> If the `-o` flag is not specified, the command will output to the current working directory.

Default ignored file and folders are the same as in `moltenobsidian manifest generate`

### Supported Protocols
Below are the protocols currently supported for vault exports :

| Protocol | URI segment           |
| -------- | --------------------- |
| HTTP     | `http://`, `https://` |
| FTP      | `ftp://`, `ftps://`   |

### Developer features
Some features of the manifest command are specifically oriented for development and automation purposes. 
Here is a detailed account of some of the extra features baked into this command.

| Flag                  | Description                                                                                                | Notes                             |
| --------------------- | ---------------------------------------------------------------------------------------------------------- | --------------------------------- |
| `--watch`             | Continuously watches for changes and updates the manifest accordingly.                                     | Can only be used on local vaults. |
| `--generate-manifest` | Generates a site manifest with the SSG assets. This is similar to running the [[Vault Manifests]] feature. |                                   |
| `--debug`             | Prints out extra information, similar to a verbose flag.                                                   |                                   |
