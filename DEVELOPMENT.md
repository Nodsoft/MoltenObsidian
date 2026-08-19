# Development and validation guide

This guide is the command-oriented companion to `CONTRIBUTING.md` and `docs/ARCHITECTURE.md`.

## Toolchain

| Area | Required toolchain | Repository evidence |
| --- | --- | --- |
| .NET libraries, tests, CLI | .NET 10 SDK | CI installs 10.0; projects multi-target `net8.0;net10.0` |
| Version inspection | local `nbgv` tool | `.config/dotnet-tools.json` |
| SSG client | pnpm + TypeScript 5.9 | client `package.json` and lockfile |
| Documentation site | Node 22+, pnpm, Astro 6 | deployment workflow and web `package.json` |

Check your environment:

```bash
dotnet --info
node --version
pnpm --version
```

## Validation tiers

### Tier 1: focused loop

Use one framework and the smallest affected test scope while iterating:

```bash
dotnet test Nodsoft.MoltenObsidian.Tests/Nodsoft.MoltenObsidian.Tests.csproj \
  --framework net10.0 \
  --filter "FullyQualifiedName~FileSystemVaultTests"
```

For Blazor:

```bash
dotnet test Nodsoft.MoltenObsidian.Blazor.Tests/Nodsoft.MoltenObsidian.Blazor.Tests.csproj \
  --framework net10.0 \
  --filter "FullyQualifiedName~VaultRouterTests"
```

### Tier 2: project validation

```bash
dotnet build path/to/Project.csproj --framework net10.0
dotnet test path/to/TestProject.csproj --framework net10.0
```

Run both target frameworks before declaring a public library change complete:

```bash
dotnet test path/to/TestProject.csproj --framework net8.0
dotnet test path/to/TestProject.csproj --framework net10.0
```

### Tier 3: repository validation

```bash
dotnet restore Nodsoft.MoltenObsidian.slnx
dotnet build Nodsoft.MoltenObsidian.slnx --no-restore
dotnet test Nodsoft.MoltenObsidian.slnx --no-build --verbosity normal
```

This matches the stages in the cross-platform build workflow. It does not build projects omitted from the solution or either JavaScript project.

### Tier 4: website pipeline

Run this when changing the CLI's output, manifest contract, SSG client, Astro code, or documentation vault.

From the repository root:

```bash
dotnet build Nodsoft.MoltenObsidian.slnx

mkdir -p nodsoft_moltenobsidian_web/public/vault

dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- \
  manifest generate nodsoft_moltenobsidian_web/vault \
  --output nodsoft_moltenobsidian_web/public/vault \
  --force

dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- \
  ssg generate \
  --from-folder nodsoft_moltenobsidian_web/vault \
  --output-path nodsoft_moltenobsidian_web/public/vault \
  --ignored-files .gitignore
```

Build the linked TypeScript client before the web project:

```bash
cd nodsoft_moltenobsidian_ssg_client
pnpm install --frozen-lockfile
pnpm build

cd ../nodsoft_moltenobsidian_web
pnpm install --frozen-lockfile
pnpm build
```

For interactive work, run `pnpm dev` in the web directory after generation.

## Test inventory

### Core test project

`Nodsoft.MoltenObsidian.Tests` covers:

- YAML frontmatter, BOM handling, empty text, line endings, conversions, and implicit conversions;
- Obsidian tag parsing and rendering;
- manifest metadata, hashing, size, MIME type, and frontmatter;
- filesystem vault enumeration, lookup, reading, writing, deletion, invalid paths, and events;
- in-memory enumeration, writes/deletes, setup mode, updates, and invalid paths;
- HTTP manifest-backed enumeration, lookup, content, and relationships.

The HTTP fixture serves the test vault through an in-process ASP.NET host and generates its manifest from the filesystem fixture.

### Blazor test project

`Nodsoft.MoltenObsidian.Blazor.Tests` covers:

- DI registration;
- route construction, casing, updates, and factory caching;
- route-attribute base path helpers;
- index note discovery and frontmatter opt-out;
- display states and custom templates;
- navigation hierarchy and attribute callbacks.

### Coverage gaps

No dedicated automated suite currently exercises:

- FTP provider behavior against a server;
- CLI command validation/prompts/watch mode end to end;
- SSG encodings and all remote source modes;
- TypeScript client routing in a unit-test runner;
- full GitHub Pages output in a browser.

For changes in these areas, add tests where feasible and include a reproducible manual check.

## Manual CLI scenarios

Show help and version:

```bash
dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- --help

dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- --version
```

Generate a manifest for a real Obsidian vault:

```bash
mkdir -p /tmp/molten-output

dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- \
  manifest generate /path/to/vault \
  --output /tmp/molten-output \
  --force --debug
```

Without `--force`, validation expects an `.obsidian` directory. Output directories must already exist.

Generate static assets from a local vault:

```bash
dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- \
  ssg generate \
  --from-folder /path/to/vault \
  --output-path /tmp/molten-output \
  --generate-manifest --debug
```

Inspect the output for:

- one `.html` per Markdown file;
- a `.yaml` neighbor only when frontmatter is non-empty;
- unchanged binary/non-Markdown assets;
- vault-relative subdirectories;
- a manifest when requested;
- correct non-ASCII text if the change touches encoding.

## Debugging by subsystem

### Frontmatter or Markdown conversion

1. Start with a string-only `ObsidianText` test.
2. Add mixed newline and BOM cases if parsing boundaries are involved.
3. For wikilinks, use a real `IVaultNote`; string-only conversion intentionally lacks vault context.
4. Inspect both the parsed inline and emitted HTML when changing a Markdig extension.

### Vault paths or tree corruption

Check all views of the same entity:

```text
vault.Files / vault.Folders / vault.Notes
entity.Path / entity.Parent
folder.Files / folder.Subfolders
```

For writable providers, verify before and after create, overwrite, move, and delete. Use nested paths and mixed-case `.md` extensions. On filesystem changes, repeat on Windows semantics or rely on the Windows CI job before merge.

### File watcher behavior

`FileSystemWatcher` is platform-dependent and may produce duplicate notifications. The implementation debounces changed-file events for 100 ms but structural events are not equivalently debounced.

Tests or repros should:

- subscribe before changing the filesystem;
- await a bounded signal such as `TaskCompletionSource`, not a long fixed sleep;
- assert the entity path and semantic update type;
- tolerate irrelevant native notifications when appropriate;
- unsubscribe and clean temporary files.

### HTTP vault behavior

Validate the manifest base address separately from content paths. A remote file opens a stream lazily through the vaultâ€™s retained `HttpClient`. Browser/WASM changes require checking response-stream lifetime and supported APIs.

The tree is immutable after `FromManifest`; there is no refresh. If a test expects an update event from HTTP, the test expectation is inconsistent with the current contract.

### Blazor routing

Inspect the route table inputs first:

- folders keep their path;
- files have `.md` removed by the router;
- exact case wins, then a case-insensitive fallback is used;
- non-`Update` vault events rebuild routes.

For display failures, determine whether the routed entity is a folder, note, root, index note, or null before inspecting templates.

### Manifest/site mismatches

Trace one entry through:

```text
IVaultFile
  -> ManifestFile JSON
  -> TypeScript VaultFile
  -> VaultManifestClient route
  -> public/vault/*.html or *.yaml
  -> Astro getStaticPaths props
  -> rendered page/navigation
```

The manifest uses flattened extension metadata. A field nested in C# but expected at top level in TypeScript will silently change publish/order/CSS behavior.

## Adding a public API

Before committing:

1. Confirm the API belongs in the chosen layer.
2. Add nullable annotations and XML documentation.
3. Consider both `net8.0` and `net10.0` API availability.
4. Avoid exposing a provider dependency from the core library.
5. Add behavioral tests, not only construction tests.
6. Update README/site docs and samples.
7. Consider package version path filters and breaking-change implications.

## Adding a vault provider

Recommended sequence:

1. Create `Vaults/Nodsoft.MoltenObsidian.Vaults.<Name>/` and its project.
2. Rely on `Vaults/Directory.Build.props` for the core reference and DI abstraction dependency.
3. Implement root/entity identity and complete dictionary indexes.
4. Create a note subtype for Markdown.
5. Define stream and client/resource lifetime.
6. Add `IWritableVault` only if atomic, coherent mutations are genuinely supported.
7. Add DI extensions in `Microsoft.Extensions.DependencyInjection`.
8. Add tests and a fixture independent of developer infrastructure where possible.
9. Add the project to `.slnx` and decide whether the CLI/samples consume it.
10. Add package metadata, `version.json`, provider docs, and a user-facing example.

## Dependency updates

For NuGet updates:

- keep framework-specific package references aligned where projects use conditional versions;
- restore and build both frameworks;
- run all tests;
- inspect transitive changes to browser/WASM compatibility.

For pnpm updates:

- update the package manifest and the lockfile in the same project;
- build the SSG client first if its API or emitted JS changes;
- regenerate vault assets and build Astro;
- keep Node 22 compatibility because deployment uses it.

GitHub Actions must remain pinned to commit SHAs.

## Release-aware changes

Normal contributors do not publish releases. Maintainers should note:

- pushes to `main` and version branches can trigger package publishing when relevant paths change;
- NuGet and GitHub Packages are both destinations;
- package provenance is attested;
- `v*` tags create GitHub releases;
- `version.json` path filters determine which packages advance for a commit;
- the toolâ€™s base version differs from the root library version.

Run `dotnet tool run nbgv -- get-version` and inspect package outputs before a release-affecting merge.

## Clean-diff check

Before handoff:

```bash
git status --short
git diff --check
git diff --stat
```

Review any changes under generated directories, lockfiles, `version.json`, workflows, and manifests deliberately. Do not delete or overwrite unrelated work in a dirty checkout.