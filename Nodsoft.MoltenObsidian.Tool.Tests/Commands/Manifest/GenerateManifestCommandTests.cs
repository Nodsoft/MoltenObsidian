using Nodsoft.MoltenObsidian.Manifest;
using Nodsoft.MoltenObsidian.Tool.Commands.Manifest;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;
using Nodsoft.MoltenObsidian.Vaults.InMemory;

namespace Nodsoft.MoltenObsidian.Tool.Tests.Commands.Manifest;

/// <summary>
/// Provides unit tests for <see cref="GenerateManifestCommand"/> logic.
/// </summary>
public sealed class GenerateManifestCommandTests : IDisposable
{
	private readonly VaultFixture _fixture;
	private readonly DirectoryInfo _outputDir;

	/// <summary>
	/// Initializes a new instance of <see cref="GenerateManifestCommandTests"/>.
	/// </summary>
	public GenerateManifestCommandTests()
	{
		_fixture = new VaultFixture();
		_outputDir = Directory.CreateTempSubdirectory("moltenobsidian-manifest-output-");
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_fixture.Dispose();
		_outputDir.Delete(recursive: true);
	}

	/// <summary>
	/// <see cref="GenerateManifestCommand.GenerateManifestAsync"/> writes a manifest file at the output path.
	/// </summary>
	[Fact]
	public async Task GenerateManifestAsync_WritesManifestFile()
	{
		await GenerateManifestCommand.GenerateManifestAsync(
			_fixture.Vault,
			_fixture.VaultDirectory,
			_outputDir,
			debugMode: false,
			promptOverwrite: _ => true,
			ct: TestContext.Current.CancellationToken
		);

		string manifestPath = Path.Combine(_outputDir.FullName, RemoteVaultManifest.ManifestFileName);
		Assert.True(File.Exists(manifestPath));
	}

	/// <summary>
	/// The manifest file written by <see cref="GenerateManifestCommand.GenerateManifestAsync"/> contains the seeded vault files.
	/// </summary>
	[Fact]
	public async Task GenerateManifestAsync_ManifestContainsVaultFiles()
	{
		RemoteVaultManifest manifest = await GenerateManifestCommand.GenerateManifestAsync(
			_fixture.Vault,
			_fixture.VaultDirectory,
			_outputDir,
			debugMode: false,
			promptOverwrite: _ => true,
			ct: TestContext.Current.CancellationToken
		);

		Assert.NotEmpty(manifest.Files);
		Assert.Contains(manifest.Files, f => f.Path.EndsWith("Note.md"));
	}

	/// <summary>
	/// When the manifest file already exists and the overwrite prompt returns <c>false</c>,
	/// the existing file is left untouched.
	/// </summary>
	[Fact]
	public async Task GenerateManifestAsync_ExistingFile_PromptReturnsFalse_FileNotOverwritten()
	{
		string manifestPath = Path.Combine(_outputDir.FullName, RemoteVaultManifest.ManifestFileName);
		const string originalContent = "original-content";
		await File.WriteAllTextAsync(manifestPath, originalContent, TestContext.Current.CancellationToken);

		await GenerateManifestCommand.GenerateManifestAsync(
			_fixture.Vault,
			_fixture.VaultDirectory,
			_outputDir,
			debugMode: false,
			promptOverwrite: _ => false,
			ct: TestContext.Current.CancellationToken
		);

		Assert.Equal(originalContent, await File.ReadAllTextAsync(manifestPath, TestContext.Current.CancellationToken));
	}

	/// <summary>
	/// When the manifest file already exists and the overwrite prompt returns <c>true</c>,
	/// the file is overwritten with a fresh manifest.
	/// </summary>
	[Fact]
	public async Task GenerateManifestAsync_ExistingFile_PromptReturnsTrue_FileOverwritten()
	{
		string manifestPath = Path.Combine(_outputDir.FullName, RemoteVaultManifest.ManifestFileName);
		const string originalContent = "original-content";
		await File.WriteAllTextAsync(manifestPath, originalContent, TestContext.Current.CancellationToken);

		await GenerateManifestCommand.GenerateManifestAsync(
			_fixture.Vault,
			_fixture.VaultDirectory,
			_outputDir,
			debugMode: false,
			promptOverwrite: _ => true,
			ct: TestContext.Current.CancellationToken
		);

		string newContent = await File.ReadAllTextAsync(manifestPath, TestContext.Current.CancellationToken);
		Assert.NotEqual(originalContent, newContent);
		Assert.Contains(RemoteVaultManifest.ManifestFileName, manifestPath);
	}

	/// <summary>
	/// Passing a non-<see cref="FileSystemVault"/> to <see cref="GenerateManifestCommand.GenerateManifestAsync"/>
	/// throws <see cref="InvalidOperationException"/>.
	/// </summary>
	[Fact]
	public async Task GenerateManifestAsync_NonFileSystemVault_ThrowsInvalidOperationException()
	{
		InMemoryVault inMemoryVault = new("TestVault");

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			GenerateManifestCommand.GenerateManifestAsync(
				inMemoryVault,
				_fixture.VaultDirectory,
				_outputDir,
				debugMode: false,
				promptOverwrite: _ => true,
				ct: TestContext.Current.CancellationToken
			)
		);
	}
}
