using Nodsoft.MoltenObsidian.Vaults.Http;

namespace Nodsoft.MoltenObsidian.Tests.Vaults.Http;

/// <summary>
/// Provides a test fixture for the <see cref="FileSystemVault"/> class.
/// </summary>
[CollectionDefinition(nameof(HttpRemoteVault))]
public abstract class HttpVaultServerFixtureCollection : ICollectionFixture<HttpVaultServerFixture>
{
	// This class has no code, and is never created.
	// Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}