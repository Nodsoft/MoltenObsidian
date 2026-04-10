using JetBrains.Annotations;
using Nodsoft.MoltenObsidian.Vaults.FileSystem;

namespace Nodsoft.MoltenObsidian.Tool.Tests;

/// <summary>
/// Provides a test fixture that seeds a minimal valid Obsidian vault in a temporary directory.
/// </summary>
[UsedImplicitly]
public sealed class VaultFixture : IDisposable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="VaultFixture"/> class,
	/// creating a temp vault directory with a <c>.obsidian</c> marker and a few seed notes.
	/// </summary>
	public VaultFixture()
	{
		VaultDirectory = Directory.CreateTempSubdirectory("moltenobsidian-test-vault-");

		// Create the .obsidian marker folder required for Obsidian vault validation.
		Directory.CreateDirectory(Path.Combine(VaultDirectory.FullName, ".obsidian"));

		// Seed a few notes so the manifest/SSG tests have real files to work with.
		File.WriteAllText(Path.Combine(VaultDirectory.FullName, "Note.md"), "# Hello\n\nThis is a test note.");
		File.WriteAllText(Path.Combine(VaultDirectory.FullName, "Another Note.md"), "# Another\n\nSecond note.");

		string subDir = Path.Combine(VaultDirectory.FullName, "SubFolder");
		Directory.CreateDirectory(subDir);
		File.WriteAllText(Path.Combine(subDir, "Nested Note.md"), "# Nested\n\nNested note.");

		Vault = FileSystemVault.FromDirectory(VaultDirectory);
	}

	/// <summary>
	/// The temporary directory that acts as the vault root.
	/// </summary>
	public DirectoryInfo VaultDirectory { get; }

	/// <summary>
	/// A <see cref="FileSystemVault"/> loaded from <see cref="VaultDirectory"/>.
	/// </summary>
	public FileSystemVault Vault { get; }

	/// <inheritdoc />
	public void Dispose() => VaultDirectory.Delete(recursive: true);
}
