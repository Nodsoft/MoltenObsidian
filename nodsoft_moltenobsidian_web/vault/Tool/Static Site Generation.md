# Vault export

## Premise
Sometimes you would like to use your vault in non-ASP.NET applications.  
This feature allows you to export a vault to static html files to be used where you like.

## Usage
Exporting a local MoltenObsidian vault to a specified directory goes as follows :
```sh  
moltenobsidian ssg generate --from-folder "/path/to/local/vault" -o "/destination/directory"
```  

The `ssg` command also supports exporting remote vaults from HTTP or FTP :
```sh  
moltenobsidian ssg generate --from-url "https://url/to/remote/vault/moltenobsidian.manifest.json" -o "/destination/directory"
moltenobsidian ssg generate --from-url "ftp://url/to/remote/vault/moltenobsidian.manifest.json" -o "/destination/directory"
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