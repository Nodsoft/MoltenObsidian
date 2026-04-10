using Nodsoft.MoltenObsidian.Tool.Commands.Manifest;

namespace Nodsoft.MoltenObsidian.Tool.Tests.Commands.Manifest;

/// <summary>
/// Provides tests for <see cref="GenerateManifestSettings.Validate"/>.
/// </summary>
public sealed class GenerateManifestSettingsTests : IDisposable
{
	private readonly DirectoryInfo _vaultDir;

	/// <summary>
	/// Initializes a new instance of <see cref="GenerateManifestSettingsTests"/>,
	/// creating a temp vault directory with the required <c>.obsidian</c> marker.
	/// </summary>
	public GenerateManifestSettingsTests()
	{
		_vaultDir = Directory.CreateTempSubdirectory("moltenobsidian-settings-test-");
		Directory.CreateDirectory(Path.Combine(_vaultDir.FullName, ".obsidian"));
	}

	/// <inheritdoc />
	public void Dispose() => _vaultDir.Delete(recursive: true);

	/// <summary>
	/// A valid vault path that contains a <c>.obsidian</c> folder passes validation.
	/// </summary>
	[Fact]
	public void Validate_ValidVaultPath_ReturnsSuccess()
	{
		GenerateManifestSettings settings = new() { VaultPathStr = _vaultDir.FullName };
		Assert.True(settings.Validate().Successful);
	}

	/// <summary>
	/// An empty vault path string is rejected.
	/// </summary>
	[Fact]
	public void Validate_EmptyVaultPath_ReturnsError()
	{
		GenerateManifestSettings settings = new() { VaultPathStr = "" };
		Assert.False(settings.Validate().Successful);
	}

	/// <summary>
	/// A vault path that does not exist on disk is rejected.
	/// </summary>
	[Fact]
	public void Validate_NonExistentVaultPath_ReturnsError()
	{
		GenerateManifestSettings settings = new() { VaultPathStr = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) };
		Assert.False(settings.Validate().Successful);
	}

	/// <summary>
	/// A vault directory without <c>.obsidian</c> and <c>Force = false</c> is rejected.
	/// </summary>
	[Fact]
	public void Validate_NoObsidianFolder_ForceOff_ReturnsError()
	{
		DirectoryInfo plainDir = Directory.CreateTempSubdirectory("moltenobsidian-plain-");

		try
		{
			GenerateManifestSettings settings = new() { VaultPathStr = plainDir.FullName, Force = false };
			Assert.False(settings.Validate().Successful);
		}
		finally
		{
			plainDir.Delete(recursive: true);
		}
	}

	/// <summary>
	/// A vault directory without <c>.obsidian</c> but <c>Force = true</c> passes validation.
	/// </summary>
	[Fact]
	public void Validate_NoObsidianFolder_ForceOn_ReturnsSuccess()
	{
		DirectoryInfo plainDir = Directory.CreateTempSubdirectory("moltenobsidian-plain-");

		try
		{
			GenerateManifestSettings settings = new() { VaultPathStr = plainDir.FullName, Force = true };
			Assert.True(settings.Validate().Successful);
		}
		finally
		{
			plainDir.Delete(recursive: true);
		}
	}

	/// <summary>
	/// A valid vault path combined with a valid existing output path passes validation.
	/// </summary>
	[Fact]
	public void Validate_ValidVaultAndExistingOutputPath_ReturnsSuccess()
	{
		DirectoryInfo outputDir = Directory.CreateTempSubdirectory("moltenobsidian-output-");

		try
		{
			GenerateManifestSettings settings = new()
			{
				VaultPathStr = _vaultDir.FullName,
				OutputPathStr = outputDir.FullName,
			};

			Assert.True(settings.Validate().Successful);
		}
		finally
		{
			outputDir.Delete(recursive: true);
		}
	}

	/// <summary>
	/// A valid vault path combined with a non-existent output path is rejected.
	/// </summary>
	[Fact]
	public void Validate_ValidVaultAndNonExistentOutputPath_ReturnsError()
	{
		GenerateManifestSettings settings = new()
		{
			VaultPathStr = _vaultDir.FullName,
			OutputPathStr = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
		};

		Assert.False(settings.Validate().Successful);
	}
}
