using System.Text;
using Nodsoft.MoltenObsidian.Vault;

namespace Nodsoft.MoltenObsidian.Vaults.InMemory.Data;

internal sealed class InMemoryVaultNote : InMemoryVaultFile, IVaultNote
{
    public InMemoryVaultNote(string name, InMemoryVaultFolder parent, InMemoryVault vault, Stream content) : base(name, parent, vault, content) {}

    public override string ContentType => "text/markdown";

    public async ValueTask<ObsidianText> ReadDocumentAsync() =>
        new(Encoding.UTF8.GetString(await this.ReadBytesAsync()), this);
}