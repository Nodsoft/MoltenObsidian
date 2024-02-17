# Vault Providers
MoltenObsidian is designed with data source modularity in mind. Vaults should be able to come in all shapes and sizes.

## Official providers
The library comes with several vault providers out of the box, to cover most mainstream use cases. Modular, they come in the form of NuGet packages :
- [[Filesystem]] ([`Nodsoft.MoltenObsidian.Vaults.FileSystem`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.FileSystem))
- [[HTTP]] ([`Nodsoft.MoltenObsidian.Vaults.Http`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.Http))
- [[FTP]] ([`Nodsoft.MoltenObsidian.Vaults.Ftp`](https://www.nuget.org/packages/Nodsoft.MoltenObsidian.Vaults.Ftp))

## Implementing your own
Implementing your own vault provider is easy, provided you're familiar with the basics of file tree resolution. Indeed, depending on your data source, you'll have a different experiences implementing a provider, with the vault tree building aspect being the most daunting to all but seasoned devs.

We recommend you follow in the footsteps of the reference provider implementations, so to get a grasp of the concepts involved.

**For most cases, here are a few guidelines:**
 - Start by implementing `IVault`, as this is the root of any entity connected to a vault. You'll have an easier time working your way top-to-bottom. 
 - We recommend you derive from a common `IVaultEntity` base/abstract implementation in cases where folders and files are physically represented. In remote sources, this will rarely be the case.
 - We also recommend you create a Factory-style constructor (private/protected ctor & public static method) for the `IVaultFile` implementation. This will allow you to conditionally return a `IVaultNote` during construction, in case the resolved file turns out to be a Markdown file.