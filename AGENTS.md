# AGENTS.md

This file is the repository-wide operating guide for coding agents and automated contributors. It applies to the whole tree. A more deeply nested `AGENTS.md`, if one is added later, overrides this file only for its subtree.

## Mission

MoltenObsidian is a multi-package .NET library for parsing Obsidian-flavoured Markdown, modeling vaults independently of their storage backend, rendering vaults in Blazor, generating remote-vault manifests, and exporting static-site assets. The repository also contains a TypeScript manifest client and the Astro site at `moltenobsidian.dev`.

Preserve these design goals:

- Keep the core vault abstractions storage-agnostic.
- Keep provider-specific I/O inside the corresponding project under `Vaults/`.
- Keep Markdown parsing and conversion independent of Blazor and CLI concerns.
- Keep remote providers manifest-driven and read-only unless a change explicitly redesigns that contract.
- Preserve both target frameworks (`net8.0` and `net10.0`) unless a maintainer-approved change says otherwise.

## Start here

Read, in order:

1. `CONTRIBUTING.md` for setup, workflow, and pull-request expectations.
2. `docs/ARCHITECTURE.md` for boundaries, data flow, and invariants.
3. `docs/DEVELOPMENT.md` for focused commands and change-specific checks.
4. The nearest tests for the code you plan to modify.

The root README still says “.NET 6+” in places, but the build configuration is authoritative: the supported targets are currently `net8.0;net10.0` in `Directory.Build.props`.

## Repository map

| Path | Responsibility |
| --- | --- |
| `Nodsoft.MoltenObsidian/` | Core API: `ObsidianText`, Markdig pipeline extensions, vault interfaces, common helpers |
| `Nodsoft.MoltenObsidian.Manifest/` | Remote manifest model and SHA-256/frontmatter manifest generation |
| `Vaults/...FileSystem/` | Writable filesystem vault and file watcher |
| `Vaults/...InMemory/` | Writable memory-mapped in-memory vault |
| `Vaults/...Http/` | Read-only manifest-backed HTTP vault |
| `Vaults/...Ftp/` | Read-only manifest-backed FTP vault |
| `Nodsoft.MoltenObsidian.Blazor/` | Router, display/navigation components, templates, Blazor DI integration |
| `Nodsoft.MoltenObsidian.Tool/` | `moltenobsidian` CLI: manifest and static-site generation |
| `Nodsoft.MoltenObsidian.Tests/` | Core, manifest, filesystem, HTTP, and in-memory xUnit tests |
| `Nodsoft.MoltenObsidian.Blazor.Tests/` | bUnit and service/helper tests for Blazor integration |
| `nodsoft_moltenobsidian_ssg_client/` | TypeScript manifest client used by the web project |
| `nodsoft_moltenobsidian_web/` | Astro documentation site and its source vault |
| `Samples/` | Example Blazor hosts; some projects are intentionally outside the solution |
| `.github/workflows/` | Cross-platform build/test, CodeQL, NuGet publish, release, and site deployment |

## Canonical validation

CI uses a .NET 10 SDK to restore, build, and test the multi-targeted solution on Linux, Windows, and macOS.

```bash
dotnet restore Nodsoft.MoltenObsidian.slnx
dotnet build Nodsoft.MoltenObsidian.slnx --no-restore
dotnet test Nodsoft.MoltenObsidian.slnx --no-build --verbosity normal
```

For website or TypeScript changes, also run:

```bash
cd nodsoft_moltenobsidian_ssg_client
pnpm install --frozen-lockfile
pnpm build

cd ../nodsoft_moltenobsidian_web
pnpm install --frozen-lockfile
pnpm build
```

The Astro build requires generated files under `nodsoft_moltenobsidian_web/public/vault/`. Follow the generation sequence in `docs/DEVELOPMENT.md`; this directory is ignored and must not be committed.

If a required SDK or external service is unavailable, do not claim full validation. Run the narrow checks that are available and state the exact gap.

## Change-to-test matrix

| Change | Minimum focused validation |
| --- | --- |
| `ObsidianText`, Markdig parsers/renderers | Relevant tests under `Nodsoft.MoltenObsidian.Tests/Core`; then both test projects |
| Vault interfaces or `VaultExtensions` | Core tests plus every provider test; inspect Blazor router impact |
| Filesystem provider | `FileSystemVaultTests`; verify watcher/event behavior and path normalization |
| In-memory provider | `InMemoryVaultTests`; verify stream lifetime, replacement, folder references, and events |
| HTTP provider or manifest tree construction | `HttpRemoteVaultTests` and manifest tests |
| FTP provider | Full solution tests plus a focused manual or integration check; there is no dedicated FTP test suite |
| Manifest schema/generator | `VaultManifestTests`; update the JSON schema and TypeScript types together |
| Blazor router/helpers/components/templates | Corresponding bUnit/service tests under `Nodsoft.MoltenObsidian.Blazor.Tests` |
| CLI | Full .NET build/tests and a local invocation with `dotnet run`; CLI-specific automated coverage is currently sparse |
| TypeScript manifest client | `pnpm build` in the client, then regenerate assets and build Astro |
| Astro site or vault docs | Regenerate `public/vault`, then `pnpm build` in the web project |
| Dependencies, target frameworks, build props | Full restore/build/test; confirm all three CI operating systems remain viable |

## Non-negotiable invariants

- Vault paths are relative to the vault root, use `/`, and do not start with `/`. The root folder path is the empty string.
- `IVault.Files`, `Folders`, and `Notes` are keyed by complete vault-relative paths. Note keys include `.md`.
- Any `.md` file must be represented as `IVaultNote`, not only `IVaultFile`.
- Folder child collections and vault dictionaries must remain consistent after create, update, move, and delete operations.
- `IWritableVault` operations must create missing parent folders where documented and raise semantically correct `VaultUpdate` events.
- Remote manifests use `moltenobsidian.manifest.json`; file hashes are Base64-encoded SHA-256 values.
- Internal wikilinks only resolve with a current `IVaultNote` in the Markdig parser context. Preserve this context when changing conversion flows.
- README/index note detection is case-insensitive. The `moltenobsidian:index:enabled` frontmatter override must continue to accept Boolean `false` and string `"false"`.
- Generated files (`bin/`, `obj/`, `artifacts/`, website `dist/`, `.astro/`, `node_modules/`, and `public/vault/`) are not source and must not be committed.
- Public library projects generate XML documentation and enforce code style during build. Add or update XML docs for public API changes.

## Implementation rules

- Follow existing C# style: file-scoped namespaces, nullable annotations, tabs in established C# projects, explicit visibility, async suffixes, and collection expressions where appropriate.
- Prefer `ValueTask` only where the surrounding public contract already uses it or the operation can commonly complete synchronously; do not introduce it mechanically.
- Thread `CancellationToken` through new I/O paths. Avoid adding sync-over-async code.
- Normalize paths at the public boundary, then keep internal representations canonical.
- Keep MIME detection consistent with existing providers and the manifest generator.
- Do not silently broaden ignored-file or ignored-folder behavior; it affects manifests, navigation, and site output.
- Treat `version.json` path filters as release behavior. Update them only after checking which packages a change should version.
- Keep GitHub Actions pinned to immutable commit SHAs.
- Preserve lockfiles when dependency manifests change.

## Tests

- Tests use xUnit v3. Blazor component tests use bUnit.
- Add regression tests first or alongside bug fixes.
- Prefer the existing fixtures for filesystem, HTTP, and in-memory vaults.
- Assert both the indexed dictionaries and parent/child relationships for vault mutations.
- When testing filesystem watcher events, account for asynchronous delivery and duplicate native events; do not use arbitrary long sleeps.
- For rendering changes, test semantic output and routing behavior rather than brittle full-document snapshots unless the complete markup is the contract.

## Known edges to inspect before editing

- The `.slnx` does not include every `.csproj`. In particular, the HTTP WASM sample and older/experimental sample directories sit outside the canonical solution. Do not assume `dotnet test` covers them.
- The website and SSG client are separate pnpm projects and are not built by the .NET workflow.
- `.run/` configurations reference an older `src/assets/vault` output path; the current website and deployment workflow use `public/vault`.
- The Pages workflow currently generates the manifest in the source vault before exporting to `public/vault`; because the filesystem provider excludes the manifest by default, verify that `public/vault/moltenobsidian.manifest.json` actually exists when changing or debugging deployment. The local sequence in `docs/DEVELOPMENT.md` writes it directly to the required output.
- Remote HTTP/FTP vaults build their tree once from a manifest and do not refresh or raise update events.
- The CLI SSG path converts Markdown to `.html`, writes frontmatter to optional `.yaml`, and copies other files. Encoding and path changes can affect all published documentation.
- The current architecture has no dedicated FTP tests and limited CLI/TypeScript tests. Changes in those areas need explicit manual evidence.

## Pull requests and handoff

Keep changes scoped. A good handoff states:

- what changed and why;
- which packages and public contracts are affected;
- exact validation commands and results;
- any checks not run;
- compatibility, manifest/schema, generated-output, or release-version implications.

Do not commit, push, publish packages, deploy the site, or open a pull request unless the user or maintainer explicitly requests that action.
