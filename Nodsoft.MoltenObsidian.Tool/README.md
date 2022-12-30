# Molten Obsidian - CLI Tool
#### Package: [`Nodsoft.MoltenObsidian.Tool`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Tool)

## Premise
Molten Obsidian vaults require some extra steps before using them in your projects, depending on the use case and data source. To remedy the menial tasks, we provided a CLI Tool to assist developers with rapidly building their solutions around Molten Obsidian.

## Installation
Installing the tool is extremely straightforward, given the [`dotnet` CLI tool](https://learn.microsoft.com/en-us/dotnet/core/tools/) framework. First be sure to have the [.NET SDK ](https://dotnet.microsoft.com/en-us/download/dotnet) installed.

Then, it's a one-line matter, to install globally :
```sh
dotnet tool install --global Nodsoft.MoltenObsidian.Tool
```

Or, if you prefer installing it locally within the scope of your project/solution :
```sh
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local Nodsoft.MoltenObsidian.Tool
```

### Update
Updating the tool is just as easy, using one command (depending on your install path) :
```sh
dotnet tool update -g Nodsoft.MoltenObsidian.Tool # Tool was setup on Global
dotnet tool update -l Nodsoft.MoltenObsidian.Tool # Tool was setup on Local 
```


## Usage
Using this tool is very easy and intuitive. 
The integrated help provides a syntax check, as such :

```sh
moltenobsidian -h
```

All subsequent commands follow the conventional POSIX structure :
```
moltenobsidian <command> [-a|--arguments value]
```


# Features


## Vault Manifest management

### Premise
Some remote vaults do not support directory listing. As such, one efficient way we've overcome this issue is by creating a conventional file called a Vault Manifest, tasked with listing all files in a vault, along with the path, size, SHA256 checksum, and any frontmatter associated within the file.

This is a process that is fully automated using the CLI Tool, and that we plan to also export to a GitHub Actions CI step in the near future.

### Usage
Creating a manifest goes as follows:
```sh
moltenobsidian manifest generate "/path/to/vault/root/"
```

The given path will then be checked for the existence of the `./.obsidian/` folder, indicative of the presence of an Obsidian Vault. While the creation of a manifest is not contingent on the presence of this folder (as it will be ignored anyways), it is preferred, to ensure that an invalid path was not specified. 
Nonetheless, if you're adamant on the location, you can bypass the checks by running the same command with the `-f|--force` argument, which will force the creation of a manifest, regardless of that validating folder's presence.

Similarly, there may be cases where you need to output the manifest to a separate folder. In these edge cases, specifying the `-o|--output <output-folder>` argument will allow you to output the manifest in a different folder (all while retaining the conventionally contingent `moltenobsidian.manifest.json` filename).
