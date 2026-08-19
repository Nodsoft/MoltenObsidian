# Contributing to MoltenObsidian

Thank you for improving MoltenObsidian. This guide gets a new contributor from clone to a reviewable change and explains how the .NET libraries, providers, CLI, TypeScript client, and Astro documentation site fit into the contribution workflow.

## Before you start

MoltenObsidian currently targets .NET 8 and .NET 10. The root README contains older â€œ.NET 6+â€ wording; `Directory.Build.props` and CI are the source of truth.

Install:

- Git.
- The .NET 10 SDK. It can build both `net8.0` and `net10.0` targets used by the solution.
- Node.js 22 or newer for the Astro 6 documentation site.
- pnpm (the repository currently uses pnpm 11 lockfiles).
- An editor with C#, Razor, TypeScript, and Astro support.

On Windows, enable long paths if your checkout location makes NuGet or intermediate paths exceed the legacy limit.

## Clone and verify

```bash
git clone https://github.com/Nodsoft/MoltenObsidian.git
cd MoltenObsidian

dotnet restore Nodsoft.MoltenObsidian.slnx
dotnet build Nodsoft.MoltenObsidian.slnx --no-restore
dotnet test Nodsoft.MoltenObsidian.slnx --no-build --verbosity normal
```

CI runs the same restore/build/test stages on Ubuntu, Windows, and macOS. The build is deterministic, emits portable symbols and XML API documentation, and treats configured code-style diagnostics as build diagnostics.

Optional local versioning tool:

```bash
dotnet tool restore
dotnet tool run nbgv -- get-version
```

Nerdbank.GitVersioning reads the root and per-project `version.json` files. Do not hand-edit assembly/package versions in project files.

## Choose the right project

| Goal | Primary project | Tests or follow-up |
| --- | --- | --- |
| Parse frontmatter or Markdown | `Nodsoft.MoltenObsidian` | Core tests |
| Add/change a Markdig feature | `Nodsoft.MoltenObsidian/Infrastructure/Markdown` | Parser and HTML-rendering tests |
| Change the shared vault contract | `Nodsoft.MoltenObsidian/Vault` | All provider tests and Blazor routing tests |
| Add local persistent storage behavior | `Vaults/...FileSystem` | Filesystem tests and watcher behavior |
| Add ephemeral writable behavior | `Vaults/...InMemory` | In-memory tests and disposal behavior |
| Change manifest-backed HTTP/FTP access | Provider plus `Nodsoft.MoltenObsidian.Manifest` | HTTP tests; manual FTP verification |
| Change Blazor display/routing | `Nodsoft.MoltenObsidian.Blazor` | bUnit and service/helper tests |
| Change CLI commands or export | `Nodsoft.MoltenObsidian.Tool` | Full build/tests and manual CLI run |
| Change manifest consumption in JS | `nodsoft_moltenobsidian_ssg_client` | TypeScript build and Astro build |
| Change the website or docs | `nodsoft_moltenobsidian_web` | Regenerate vault output and Astro build |

Read `docs/ARCHITECTURE.md` before changing a cross-project contract.

## Development workflow

1. Start from an up-to-date branch.
2. Reproduce the issue or identify the contract to change.
3. Add or update a focused test where automated coverage exists.
4. Make the smallest coherent change.
5. Run the focused tests, then the full checks appropriate to the changed area.
6. Update XML docs, Markdown docs, the manifest schema, TypeScript types, samples, or generated instructions when a public contract changes.
7. Review the diff for build output, credentials, local paths, and accidental generated files.

Use conventional, descriptive commit subjects when practical. Existing history commonly uses forms such as:

```text
feat(vaults): add ...
fix(blazor): correct ...
test(services): cover ...
refactor(manifest): ...
chore(deps): ...
docs: ...
```

One logical concern per commit makes reviews and release notes easier.

## Focused .NET commands

Build one project for one framework:

```bash
dotnet build Nodsoft.MoltenObsidian/Nodsoft.MoltenObsidian.csproj \
  --framework net10.0
```

Run a test project for one framework:

```bash
dotnet test Nodsoft.MoltenObsidian.Tests/Nodsoft.MoltenObsidian.Tests.csproj \
  --framework net10.0
```

Run a test class or method:

```bash
dotnet test Nodsoft.MoltenObsidian.Tests/Nodsoft.MoltenObsidian.Tests.csproj \
  --framework net10.0 \
  --filter "FullyQualifiedName~ObsidianTextTests"
```

Run the CLI from source:

```bash
dotnet run --project Nodsoft.MoltenObsidian.Tool \
  --framework net10.0 -- --help
```

Package locally without publishing:

```bash
dotnet build Nodsoft.MoltenObsidian.slnx -c Release
dotnet pack Nodsoft.MoltenObsidian.slnx -c Release --no-build --include-symbols
```

Packages and symbol packages are ignored. Publishing is handled by CI after successful build/test and is not part of a normal contributor workflow.

## TypeScript and website development

The TypeScript manifest client and Astro site are separate from the .NET solution.

Build the client:

```bash
cd nodsoft_moltenobsidian_ssg_client
pnpm install --frozen-lockfile
pnpm build
```

Generate the websiteâ€™s vault assets from the current source, from the repository root:

```bash
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

Then install and build the site:

```bash
cd nodsoft_moltenobsidian_web
pnpm install --frozen-lockfile
pnpm build
```

Use `pnpm dev` for a local server after the generated vault assets exist. Do not commit `public/vault/`, `.astro/`, `dist/`, or `node_modules/`.

The `.run/` IDE tasks currently reference `src/assets/vault`, an older output layout. Prefer the commands above and the GitHub Pages workflow, which use `public/vault`.

## Coding conventions

### C# and Razor

- Use nullable reference types and preserve the repositoryâ€™s file-scoped namespaces.
- Use the existing formatting in the file you edit; established library code primarily uses tabs.
- Document public types and members with XML comments.
- Keep async APIs genuinely asynchronous and pass cancellation tokens through I/O layers.
- Preserve the storage-neutral interfaces in the core package. Provider dependencies do not belong in `Nodsoft.MoltenObsidian`.
- Use case-insensitive checks where the vault contract already treats extensions or index filenames case-insensitively.
- Treat streams as owned by the caller or callee exactly as documented. Tests should prove streams remain valid for the expected lifetime.
- Avoid changing path semantics incidentally. Vault paths are forward-slash, vault-relative identifiers.

### TypeScript and Astro

- Keep the TypeScript client strict and update its types when the C# manifest contract changes.
- Preserve the clientâ€™s separation from the Astro-specific UI.
- Never render untrusted HTML without considering the current raw HTML path (`<Fragment set:html={content} />`).
- Update and commit the relevant pnpm lockfile with dependency changes.

## Vault-provider checklist

A provider should maintain a coherent tree and indexes:

- `Root` represents the vault root and has an empty `Path`.
- `Folders`, `Files`, and `Notes` are keyed by vault-relative path.
- `Parent`, `Subfolders`, and folder-local `Files` agree with those dictionaries.
- Markdown files implement `IVaultNote` and report `text/markdown`.
- `OpenReadAsync` returns a readable stream whose lifetime is clear.
- Writable providers normalize input paths, create missing parents as documented, update every index, and raise correct events.
- Remote providers do not assume filesystem operations; manifest paths are the canonical structure.
- DI registration matches provider lifetime and I/O initialization behavior.

If you add a provider, add it under `Vaults/`, reference the core package through `Vaults/Directory.Build.props`, add DI extensions in the `Microsoft.Extensions.DependencyInjection` namespace, wire it into the solution, add tests and documentation, and decide whether the CLI should consume it.

## Markdown and frontmatter checklist

- Frontmatter is recognized only when `---` is the first logical line and has a closing `---` line.
- A UTF-8 BOM at the beginning is trimmed.
- Frontmatter values are deserialized as `Dictionary<string, object>` using YamlDotNet.
- Wikilinks use the current noteâ€™s vault context for resolution.
- Relative note-name lookup searches local, upstream, and downstream folders; folder-qualified relative resolution remains an explicit TODO.
- Tags must start after whitespace, cannot be headings (`##`), allow letters/digits/underscore/slash/hyphen, and cannot be entirely numeric.

Add parser tests for valid, invalid, boundary, Unicode, and mixed-newline inputs when changing any of these rules.

## Tests and fixtures

The test suites use xUnit v3. `Nodsoft.MoltenObsidian.Blazor.Tests` additionally uses bUnit.

- `Assets/TestVault` is copied into test output and backs filesystem/manifest/HTTP fixtures.
- Collection fixtures isolate shared filesystem and HTTP server setup.
- In-memory tests exercise writes, deletes, setup mode, events, and invalid paths.
- Blazor tests cover DI, routing, helpers, display templates, and navigation markup.

There is currently no dedicated FTP test project and limited automated coverage for CLI and TypeScript behavior. For changes in those areas, include the exact manual scenario and observed result in the pull request.

## Documentation synchronization

Update all affected representations of a contract:

- Public C# API: XML comments and user-facing Markdown examples.
- Manifest fields: C# model, `moltenobsidian.manifest.schema.json`, TypeScript types, generator, and consumer behavior.
- CLI syntax: command settings, tool README, website vault docs, and workflows or IDE tasks that invoke the command.
- Target frameworks or SDK requirements: `Directory.Build.props`, project-specific overrides, CI, samples, and README wording.
- Website source docs live under `nodsoft_moltenobsidian_web/vault/`; generated HTML/YAML/manifest files do not.

## Pull-request checklist

- [ ] The change has a narrow, clear purpose.
- [ ] Public behavior is covered by a test or a documented manual check.
- [ ] Both .NET target frameworks still build, or the PR explains an intentional compatibility change.
- [ ] Relevant provider, manifest, Blazor, CLI, TypeScript, and site checks were run.
- [ ] Public APIs have XML documentation.
- [ ] Schema/types/docs/samples were synchronized where needed.
- [ ] No generated build output, website output, credentials, or machine-specific files are included.
- [ ] The PR description lists exact commands and any checks not run.

## Reporting bugs

Include:

- MoltenObsidian package/CLI version or commit SHA;
- OS, .NET SDK/runtime, and host type (server, Blazor WASM, CLI, etc.);
- vault provider;
- a minimal vault tree and note/manifest sample with secrets removed;
- expected and actual behavior;
- exception and stack trace where relevant;
- a minimal reproduction if possible.

Use responsible disclosure rather than a public issue for vulnerabilities that would put users at risk.