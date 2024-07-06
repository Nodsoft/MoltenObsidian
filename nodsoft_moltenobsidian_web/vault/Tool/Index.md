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
```sh  
moltenobsidian <command> [subcommand] [-a|--arguments [value]]  
```

