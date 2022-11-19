using System.Text;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.FileSystem.Data;

internal sealed class FileSystemVaultMarkdownFile : FileSystemVaultFile, IVaultMarkdownFile
{
	public FileSystemVaultMarkdownFile(FileInfo file, IVaultFolder parent) : base(file, parent) { }
	
	public override string ContentType => "text/markdown";
	
	public ObsidianText ReadDocument() => new(Encoding.UTF8.GetString(ReadAllBytes()));
}