using Nodsoft.MoltenObsidian.Tool.Commands.SSG;

namespace Nodsoft.MoltenObsidian.Tool.Tests.Commands.SSG;

/// <summary>
/// Provides tests for <see cref="GenerateStaticSiteCommandSettings.Validate"/>.
/// </summary>
public sealed class GenerateStaticSiteCommandSettingsTests : IDisposable
{
	private readonly DirectoryInfo _vaultDir;
	private readonly DirectoryInfo _outputDir;

	/// <summary>
	/// Initializes a new instance of <see cref="GenerateStaticSiteCommandSettingsTests"/>.
	/// </summary>
	public GenerateStaticSiteCommandSettingsTests()
	{
		_vaultDir = Directory.CreateTempSubdirectory("moltenobsidian-ssg-vault-");
		_outputDir = Directory.CreateTempSubdirectory("moltenobsidian-ssg-output-");
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_vaultDir.Delete(recursive: true);
		_outputDir.Delete(recursive: true);
	}

	// ---------------------------------------------------------------------------
	// Source mutual-exclusion
	// ---------------------------------------------------------------------------

	/// <summary>
	/// Providing both <c>--from-folder</c> and <c>--from-url</c> is rejected.
	/// </summary>
	[Fact]
	public void Validate_BothFromFolderAndFromUrl_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
		{
			s.SetLocalVaultPath(_vaultDir.FullName);
			s.SetRemoteManifestUrl("http://example.com/manifest.json");
		});

		Assert.False(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// --from-folder
	// ---------------------------------------------------------------------------

	/// <summary>
	/// <c>--from-folder</c> pointing at a non-existent directory is rejected.
	/// </summary>
	[Fact]
	public void Validate_FromFolder_NonExistentPath_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetLocalVaultPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))
		);

		Assert.False(settings.Validate().Successful);
	}

	/// <summary>
	/// <c>--from-folder</c> pointing at an existing directory passes validation.
	/// </summary>
	[Fact]
	public void Validate_FromFolder_ValidPath_ReturnsSuccess()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetLocalVaultPath(_vaultDir.FullName)
		);

		Assert.True(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// --from-url
	// ---------------------------------------------------------------------------

	/// <summary>
	/// <c>--from-url</c> with a malformed URL is rejected.
	/// </summary>
	[Fact]
	public void Validate_FromUrl_InvalidUrl_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetRemoteManifestUrl("not-a-valid-url")
		);

		Assert.False(settings.Validate().Successful);
	}

	/// <summary>
	/// <c>--from-url</c> with an unsupported scheme (<c>file://</c>) is rejected.
	/// </summary>
	[Fact]
	public void Validate_FromUrl_UnsupportedScheme_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetRemoteManifestUrl("file:///some/local/path")
		);

		Assert.False(settings.Validate().Successful);
	}

	/// <summary>
	/// <c>--from-url</c> with a valid <c>http://</c> URL passes validation.
	/// </summary>
	[Fact]
	public void Validate_FromUrl_HttpUrl_ReturnsSuccess()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetRemoteManifestUrl("http://example.com/moltenobsidian.manifest.json")
		);

		Assert.True(settings.Validate().Successful);
	}

	/// <summary>
	/// <c>--from-url</c> with a valid <c>ftp://</c> URL passes validation.
	/// </summary>
	[Fact]
	public void Validate_FromUrl_FtpUrl_ReturnsSuccess()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
			s.SetRemoteManifestUrl("ftp://example.com/moltenobsidian.manifest.json")
		);

		Assert.True(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// --generate-manifest with --from-url
	// ---------------------------------------------------------------------------

	/// <summary>
	/// Combining <c>--generate-manifest</c> with <c>--from-url</c> is rejected.
	/// </summary>
	[Fact]
	public void Validate_GenerateManifest_WithFromUrl_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
		{
			s.SetRemoteManifestUrl("http://example.com/moltenobsidian.manifest.json");
			s.SetGenerateManifest(true);
		});

		Assert.False(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// --watch
	// ---------------------------------------------------------------------------

	/// <summary>
	/// <c>--watch</c> without a local vault (<c>--from-url</c> only) is rejected.
	/// </summary>
	[Fact]
	public void Validate_Watch_WithoutLocalVault_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
		{
			s.SetRemoteManifestUrl("http://example.com/moltenobsidian.manifest.json");
			s.SetWatch(true);
		});

		Assert.False(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// --output-path
	// ---------------------------------------------------------------------------

	/// <summary>
	/// A valid existing output path passes validation.
	/// </summary>
	[Fact]
	public void Validate_OutputPath_Exists_ReturnsSuccess()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
		{
			s.SetLocalVaultPath(_vaultDir.FullName);
			s.SetOutputPath(_outputDir.FullName);
		});

		Assert.True(settings.Validate().Successful);
	}

	/// <summary>
	/// A non-existent output path is rejected.
	/// </summary>
	[Fact]
	public void Validate_OutputPath_NotExists_ReturnsError()
	{
		GenerateStaticSiteCommandSettings settings = BuildSettings(s =>
		{
			s.SetLocalVaultPath(_vaultDir.FullName);
			s.SetOutputPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
		});

		Assert.False(settings.Validate().Successful);
	}

	// ---------------------------------------------------------------------------
	// Helper — builds a settings object via reflection so private setters can be
	// populated without going through the Spectre.Console.Cli parsing pipeline.
	// ---------------------------------------------------------------------------

	private static GenerateStaticSiteCommandSettings BuildSettings(Action<SettingsBuilder> configure)
	{
		SettingsBuilder builder = new();
		configure(builder);
		return builder.Build();
	}

	private sealed class SettingsBuilder
	{
		private string _localVaultPath = "";
		private string _remoteManifestUrl = "";
		private string _outputPath = "";
		private bool _generateManifest;
		private bool _watch;

		public void SetLocalVaultPath(string path) => _localVaultPath = path;
		public void SetRemoteManifestUrl(string url) => _remoteManifestUrl = url;
		public void SetOutputPath(string path) => _outputPath = path;
		public void SetGenerateManifest(bool value) => _generateManifest = value;
		public void SetWatch(bool value) => _watch = value;

		public GenerateStaticSiteCommandSettings Build()
		{
			GenerateStaticSiteCommandSettings settings = new();

			SetPrivate(settings, nameof(GenerateStaticSiteCommandSettings.LocalVaultPathString), _localVaultPath);
			SetPrivate(settings, nameof(GenerateStaticSiteCommandSettings.RemoteManifestUrlString), _remoteManifestUrl);
			SetPrivate(settings, nameof(GenerateStaticSiteCommandSettings.OutputPathString), _outputPath);
			SetPrivate(settings, nameof(GenerateStaticSiteCommandSettings.GenerateManifest), _generateManifest);
			SetPrivate(settings, nameof(GenerateStaticSiteCommandSettings.Watch), _watch);

			return settings;
		}

		private static void SetPrivate(object obj, string propertyName, object value)
		{
			System.Reflection.PropertyInfo? prop = obj.GetType().GetProperty(propertyName,
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			prop?.SetValue(obj, value);
		}
	}
}
