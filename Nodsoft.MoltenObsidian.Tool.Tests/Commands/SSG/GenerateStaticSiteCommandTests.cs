using Nodsoft.MoltenObsidian.Tool.Commands.SSG;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

namespace Nodsoft.MoltenObsidian.Tool.Tests.Commands.SSG;

/// <summary>
/// Provides unit tests for <see cref="GenerateStaticSite"/> logic (specifically <see cref="GenerateStaticSite.WriteStaticFilesAsync"/>).
/// </summary>
public sealed class GenerateStaticSiteCommandTests : IDisposable
{
	private readonly VaultFixture _fixture;
	private readonly DirectoryInfo _outputDir;

	/// <summary>
	/// Initializes a new instance of <see cref="GenerateStaticSiteCommandTests"/>.
	/// </summary>
	public GenerateStaticSiteCommandTests()
	{
		_fixture = new VaultFixture();
		_outputDir = Directory.CreateTempSubdirectory("moltenobsidian-ssg-output-");
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_fixture.Dispose();
		_outputDir.Delete(recursive: true);
	}

	/// <summary>
	/// <see cref="GenerateStaticSite.WriteStaticFilesAsync"/> writes expected output files
	/// to the output directory for a seeded local vault.
	/// </summary>
	[Fact]
	public async Task WriteStaticFilesAsync_WritesExpectedOutputFiles()
	{
		string[] ignoredFiles = [..FileSystemVault.DefaultIgnoredFiles];
		string[] ignoredFolders = [..FileSystemVault.DefaultIgnoredFolders];

		await GenerateStaticSite.WriteStaticFilesAsync(
			_fixture.Vault,
			_outputDir,
			ignoredFiles,
			ignoredFolders,
			TestContext.Current.CancellationToken
		);

		// The seeded vault has Note.md, Another Note.md, and SubFolder/Nested Note.md.
		// WriteStaticFilesAsync converts notes to .html files.
		string[] htmlFiles = Directory.GetFiles(_outputDir.FullName, "*.html", SearchOption.AllDirectories);
		Assert.NotEmpty(htmlFiles);
	}

	/// <summary>
	/// Files listed in the <c>ignoredFiles</c> array are not written to the output directory.
	/// </summary>
	[Fact]
	public async Task WriteStaticFilesAsync_IgnoredFile_NotWrittenToOutput()
	{
		// Add a file that should be ignored.
		const string ignoredFileName = "ignored.md";
		File.WriteAllText(Path.Combine(_fixture.VaultDirectory.FullName, ignoredFileName), "# Ignored");

		// Reload the vault so the new file is picked up.
		FileSystemVault vault = FileSystemVault.FromDirectory(_fixture.VaultDirectory);

		string[] ignoredFiles = [ignoredFileName, ..FileSystemVault.DefaultIgnoredFiles];
		string[] ignoredFolders = [..FileSystemVault.DefaultIgnoredFolders];

		await GenerateStaticSite.WriteStaticFilesAsync(
			vault,
			_outputDir,
			ignoredFiles,
			ignoredFolders,
			TestContext.Current.CancellationToken
		);

		// The ignored file must not appear in the output directory (as .html or otherwise).
		string ignoredBaseName = Path.GetFileNameWithoutExtension(ignoredFileName);
		string[] matchingFiles = Directory.GetFiles(_outputDir.FullName, $"{ignoredBaseName}*", SearchOption.AllDirectories);
		Assert.Empty(matchingFiles);
	}

	/// <summary>
	/// Files inside ignored folders are not written to the output directory.
	/// The vault must be constructed with the <paramref name="ignoredFolders"/> parameter so that
	/// files within those folders are excluded at the vault level before <see cref="GenerateStaticSite.WriteStaticFilesAsync"/> runs.
	/// </summary>
	[Fact]
	public async Task WriteStaticFilesAsync_IgnoredFolder_NotWrittenToOutput()
	{
		const string ignoredFolderName = "SecretFolder";
		string ignoredFolderPath = Path.Combine(_fixture.VaultDirectory.FullName, ignoredFolderName);
		Directory.CreateDirectory(ignoredFolderPath);
		File.WriteAllText(Path.Combine(ignoredFolderPath, "Secret.md"), "# Secret");

		// Build the vault *with* the custom ignored folder so the file is excluded at the vault level.
		string[] ignoredFiles = [..FileSystemVault.DefaultIgnoredFiles];
		string[] ignoredFolders = [ignoredFolderName, ..FileSystemVault.DefaultIgnoredFolders];
		FileSystemVault vault = FileSystemVault.FromDirectory(_fixture.VaultDirectory, ignoredFolders, ignoredFiles);

		await GenerateStaticSite.WriteStaticFilesAsync(
			vault,
			_outputDir,
			ignoredFiles,
			ignoredFolders,
			TestContext.Current.CancellationToken
		);

		// No output file should be inside a "SecretFolder" subdirectory.
		string expectedOutputSubdir = Path.Combine(_outputDir.FullName, ignoredFolderName);
		Assert.False(Directory.Exists(expectedOutputSubdir));
	}
}
