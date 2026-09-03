<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN], nothing integrated yet -->

# The contents of `.config/dotnet-tools.json` for Rasm

Versions were read on 2026-09-03 through the `nuget` MCP against the repository's `NuGet.config`. Settled: the manifest holds `dotnet-reportgenerator-globaltool`, `dotnet-stryker`, and both MCP servers, with `rollForward` on the two net8 tools run through `dotnet tool run`. `binlogtool` stays out, because the two commands the binlog MCP lacks, `redact` and `stats`, have no use while no binlog leaves the machine and none exceeds the MCP threshold. Every Forge tool leaves, no package validation and no per-package versioning enter, MinVer supplies the repository release version as a `PackageReference` and never a tool, and coverage merges into one report per language with no threshold or gate.

## [00]-[REPOSITORY_CONTENTS]

`Workspace.slnx` lines 5-19 list eight .NET projects: six `Rasm.Interop.*` libraries, `tests/dotnet/support/Rasm.TestSupport.csproj`, and `tools/dotnet/Rasm.Policy.Analyzers`. No test project exists, and `Rasm.TestSupport` is a plain library: its project file has no `OutputType`, no `UseMicrosoftTestingPlatformRunner`, and no MTP package reference, and it references `CsCheck`, `LanguageExt.Core`, `Microsoft.Extensions.TimeProvider.Testing`, `Thinktecture.Runtime.Extensions`, `xunit.v3.assert`, `xunit.v3.extensibility.core`. No project references `coverlet.MTP`, `Microsoft.Testing.*`, or any test host (`rg -l "coverlet\|Microsoft.Testing" --glob '*.csproj'` returns nothing).

No application exists, `apps/` holds `README.md` alone. No CI exists, `.github` does not exist. Ten packaging projects sit under `eng/native`, excluded from the solution and from the root build files (`eng/native/*/*.csproj`, `nx.json` line 81 `"exclude": ["eng/native/**"]`, `eng/native/Directory.Build.props` line 2).

The manifest is committed and clean (`git ls-files .config/` lists `.config/dotnet-tools.json`, `git status --short .config/` is empty) and holds one tool, `dotnet-stryker` 4.16.0. The machine has SDKs 8.0.424, 9.0.317, 10.0.400 (`dotnet --list-sdks`) and `Microsoft.NETCore.App` runtimes 8.0.30, 9.0.19, 10.0.11 (`dotnet --list-runtimes`). `global.json` pins SDK 10.0.400 with `rollForward: disable` and `test.runner: Microsoft.Testing.Platform`. The `dnx` shell script is off `PATH` (`command -v dnx` finds nothing), and `.mcp.json` line 6 uses `dotnet dnx`, which works.

Absence decides three of the five decision areas: no test project, no application, no published package.

## [01]-[COVERAGE]

### [01.1]-[CURRENT_OUTPUT]

`coverlet.MTP` produces nothing. `Directory.Packages.props` line 768 pins `coverlet.MTP` 10.0.1 (the current release, published 2026-05-18), a version declaration no project consumes.

Once a test project exists, from the package README (nuget.org, `coverlet.MTP` 10.0.1): `--coverlet-output-format` defaults to `json`, `cobertura`, and the supported formats are `json`, `lcov`, `opencover`, `cobertura`, `teamcity`. `--coverlet-file-prefix` exists "to prevent overwrites when multiple test projects write to the same directory". Output is one Cobertura file per test project, never one per workspace.

### [01.2]-[MERGING]

Nothing in coverlet merges coverage across projects. `Documentation/Coverlet.MTP.Integration.md` in `coverlet-coverage/coverlet`, "Known Limitations": "Threshold validation is not yet supported (planned for future releases)" and "Report merging is not yet supported (use external tools like `dotnet-coverage` or `reportgenerator`)".

### [01.3]-[REPOSITORY_REQUIREMENTS]

`tests/README.md` line 98, under `[06]-[GATE_OWNERSHIP]`: "Coverage aggregates as Cobertura (.NET) and LCOV (Python, TypeScript) under `.artifacts/`, each reporter defines its format". No configured component satisfies it: Python and TypeScript each emit one LCOV file per run, and .NET emits N Cobertura files with no aggregation step. `tests/README.md` line 65 records the collection half ("`--coverlet` on run, writes to the run results directory"), and line 75 requires that after a tool runs "`git status --short` and the repository-root listing must show no new generated entries", and the aggregate belongs under `.artifacts/`.

`README.md` `[03]-[QUALITY]` sets no coverage threshold, and the fifth-round decision removes every threshold: coverage is information about what is tested. The Roslyn MCP's `find_uncovered_symbols` answers a different question, symbols no test reaches statically, not lines executed.

### [01.4]-[VERDICT]

Adopt `dotnet-reportgenerator-globaltool` 5.5.11 (published 2026-07-27). It consumes MTP output rather than duplicating it. Its `Reports` parameter takes "Path to the coverage report(s) that should be parsed. Globbing is supported", and its `Cobertura` report type "Creates a XML file in Cobertura format" (<https://reportgenerator.io/usage>). N Cobertura inputs to one Cobertura output is the sentence in `tests/README.md` line 98. It emits `MarkdownSummaryGithub`, `TextSummary`, `Badges`, and `lcov` from the same run. It targets .NET 8.0 (nuget.org package page, "This package targets .NET 8.0"). ReportGenerator has no threshold or failing exit code, the usage page's parameter list holds none, which matches the decision exactly.

Reject `dotnet-coverage` 18.11.0 (2026-09-02): its collector duplicates `coverlet.MTP`, which `Directory.Packages.props` already chose, and one merge tool is enough.

### [01.5]-[CONFIGURATION_CHANGE]

Both edits stay out of project bodies (RASM0002 at `Directory.Build.targets` lines 65, 94-95 forbids them).

1. Root `Directory.Build.targets`, which makes every future test project collect Cobertura under a distinct prefix. It belongs in `.targets` because the condition reads `RasmRole`, which `Directory.Build.targets` lines 3-6 define and which does not exist at `.props` evaluation time. The MTP integration page documents `TestingPlatformCommandLineArguments` in `Directory.Build.props` or `Directory.Build.targets` with conditions as the way to route arguments per project (<https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-integration-dotnet-test>).

```xml
<PropertyGroup Label="Test Coverage" Condition="'$(RasmRole)' == 'tests'">
    <TestingPlatformCommandLineArguments>--coverlet --coverlet-output-format cobertura --coverlet-file-prefix $(MSBuildProjectName)</TestingPlatformCommandLineArguments>
</PropertyGroup>
```

2. An Nx target that runs after `test`, writing the aggregate under `.artifacts/`:

```
dotnet tool run reportgenerator -reports:.artifacts/dotnet/**/*.cobertura.*.xml -targetdir:.artifacts/dotnet/coverage -reporttypes:Cobertura;MarkdownSummaryGithub;TextSummary
```

`tests/README.md` `[04]-[GENERATED_OUTPUTS]` gains a row for it, and `[07]-[CONFIGURATION_OWNERS]` gains the target as the owner of the aggregate.

## [02]-[RUNTIME_DIAGNOSTICS]

### [02.1]-[WORKLOADS]

No process exists to attach to. No project in `Workspace.slnx` has `OutputType` `Exe`. The plugin-host machinery exists unused: `Directory.Build.targets` line 46 conditions the policy analyzer on `'$(OutputType)' == 'Exe' or '$(OutputType)' == 'WinExe' or '$(RasmPluginHost)' == 'true'`, and `Directory.Build.props` lines 57-66 resolve a Rhino bundle path. `apps/README.md` names Rhino 9 and Grasshopper 2 as the hosts. A plugin runs inside the Rhino process, which is what `dotnet-counters`, `dotnet-trace`, `dotnet-dump`, and `dotnet-gcdump` attach to by PID. `Microsoft.Testing.Extensions.CrashDump` and `HangDump` 2.3.3 are pinned (`Directory.Packages.props` lines 771-772) and referenced by no project.

### [02.2]-[MCP_LIMITS]

Neither MCP server observes a running process. The binlog server's `binlog_expensive_projects`, `binlog_expensive_targets`, `binlog_expensive_tasks`, `binlog_expensive_analyzers`, and `binlog_project_target_times` measure build time from a `.binlog`. Every Roslyn MCP tool is static analysis, and `analyze_control_flow` and `analyze_data_flow` are compile-time.

### [02.3]-[VERDICT]

None enters. `dotnet-counters`, `dotnet-trace`, `dotnet-dump`, `dotnet-gcdump` (all 10.0.731102, published 2026-09-02) and `dotnet-sos`, `dotnet-symbol` (same version) enter the manifest when an application or plugin project exists and a test project references the `CrashDump`/`HangDump` extensions. Until then nothing installs them for Rasm. Attaching to a running Rhino is a machine act, and Forge's copies (`dev-tools.nix` lines 181-184, pinned at 9.0.661903 while the current release is 10.0.731102) are among the rows Forge removes later. `dotnet-monitor` 10.0.3 (2026-07-17) is a long-running HTTP diagnostics sidecar for hosted services, the declared hosts are Rhino and Grasshopper, and it never enters the manifest as configured.

## [03]-[FORGE_TOOLS]

Forge installs eight .NET tools at `Parametric_Forge/modules/home/programs/languages/dev-tools.nix` lines 174-190, judged against the repository's own rules.

`csharpier` 1.3.0 (2026-06-07) drops, an SDK verb replaces it. `README.md` line 85 names `dotnet format` as the .NET formatter and `.editorconfig` as the holder of rule severity. `Directory.Build.props` lines 30 and 32 set `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors`, the 553-line `.editorconfig`'s style rules are build errors, and a second formatter rewrites code the build then rejects. `dotnet format --version` on the machine prints `10.0.400-servicing.26379.115`.

`dotnet-outdated` (`dotnet-outdated-tool` 4.8.1, 2026-06-11) drops, an SDK verb with the MCP replaces it. The report is `dotnet package list --outdated --format json` (`dotnet package list --help` lists `--outdated`, `--deprecated`, `--vulnerable`, `--include-prerelease`, `--highest-patch`, `--highest-minor`, `--format`, `--output-version`). The in-place upgrade is the `nuget` MCP's `upgrade_packages_to_latest` and `update_package_version`, which `CLAUDE.md` `[TOOL_ROUTING]` mandates.

`dotnet-ef` 10.0.11 (2026-08-11) drops. `README.md` `[07]-[CHANGE]`: schemas derive from their owning types and a schema library applies the delta "with no migration file or history table". `dotnet ef migrations add` produces the artifact that sentence forbids. `Microsoft.EntityFrameworkCore.Design` 10.0.11 is pinned at `Directory.Packages.props` line 517 and referenced by no project.

`sharpfuzz` (`SharpFuzz.CommandLine` 2.3.0, 2026-06-16) drops. No fuzz target, harness, or corpus exists, and the fourth-round decision drops every fuzzing package.

`ilspycmd` 11.0.0.9375 (2026-08-12) drops, the MCP replaces it. `RoslynCodeLens.Mcp` exposes `inspect_external_assembly` and `peek_il`, built on ICSharpCode.Decompiler (README, "Third-party licenses"). Whole-assembly decompilation to `.cs` files on disk has no consumer.

`nuget-to-json` (nixpkgs, unversioned) drops as a Nix packaging script rather than a .NET tool. The nixpkgs manual's Dotnet section: "Next, use the `nuget-to-json` tool provided in Nixpkgs to generate a lockfile to `deps.json`" (`doc/languages-frameworks/dotnet.section.md` line 253). It serves Nix packaging of .NET applications, and Rasm packages nothing with Nix.

`dotnet-coverage` 18.11.0 (2026-09-02) drops for the reason the coverage verdict states.

`roslyn-ls` (nixpkgs, unversioned) stays in Forge and never becomes a manifest entry. It is an editor language server (`Microsoft.CodeAnalysis.LanguageServer`) consumed by Forge's editor configuration, and `CLAUDE.md` routes C# navigation to the Roslyn MCP.

`dotnet-stryker` 4.16.0 (the current release, published 2026-07-03) is installed twice: the Forge pin and the manifest. The manifest copy is authoritative because `stryker-config.json` is a repository file and its `solution: Workspace.slnx` is a repository path.

## [04]-[TOOLS_OUTSIDE_FORGE]

### [04.1]-[PACKAGE_VALIDATION]

`Meziantou.Framework.NuGetPackageValidation.Tool` 2.0.6 (2026-09-02) validates a local `.nupkg` before it is pushed to a server. It is rejected on three grounds, and the no-publishing decision settles it:

1. The packages never reach a server. `tools/nx/native-packaging.ts` lines 222-242 build the pack command as `dotnet pack <root> --configuration Release --output <source>`, where `<source>` is the `local` entry in `NuGet.config` (`.artifacts/nuget`, `NuGet.config` line 6, and the plugin's own comment at line 73: "The local source in NuGet.config owns the pack output path, restore reads the nupkg from there"). `packageSourceMapping` (`NuGet.config` lines 17-19) restores `Rasm.*` from that feed alone.
2. Its rules encode publish-time metadata the packages deliberately omit: `eng/native/Directory.Build.props` line 10 sets `Authors` alone, each project adds `Description` and `PackageLicenseExpression`, and nothing sets a project URL, tags, icon, readme, or repository.
3. Pack is checked three ways: `eng/native/Directory.Build.targets` lines 18-21 (`EnsureStagedNativeLibraries`), 27-33 (`EnsureManifestVersionMatch`), and 36-42 (`EnsureCentralPackageVersionMatch`) each error before `GenerateNuspec`.

The SDK's own package validation (`Microsoft.NET.ApiCompat.ValidatePackage.targets` under `<sdk>/10.0.400/Sdks/Microsoft.NET.Sdk/targets/`, enabled by `EnablePackageValidation`) is rejected as well: eight of the ten packaging projects set `IncludeBuildOutput` false and include a `lib/netstandard2.0/_._` placeholder (`eng/native/Directory.Build.targets` line 11), and no managed API exists to compare. The two that compile managed code, `Rasm.Z3` (`Version` 5.0.0) and `Rasm.Gmsh` (`Version` 4.15.2), generate their sources from a pinned upstream archive with the package version as its version. `Microsoft.DotNet.ApiCompat.Tool` 10.0.400 (2026-08-11) is rejected for the same reason.

### [04.2]-[VERSIONING]

The sixth-round decision: MinVer at the root supplies the release version, read from the git tag. MinVer is "a minimalist .NET build package" installed as a `PackageReference` that "should normally include `PrivateAssets="All"`", and `minver-cli` is the separate tool "for versioning any kind of software or content using Git tags" (<https://github.com/adamralph/minver>). Both are at 7.0.0 (published 2026-01-05). MinVer sets `AssemblyVersion`, `FileVersion`, `InformationalVersion`, `PackageVersion`, and `Version` from the latest tag in the commit history, with height added on untagged commits.

The reference goes in the root `Directory.Build.props`, which satisfies RASM0002: `Directory.Build.targets` line 65 matches `Version|VersionPrefix|VersionSuffix|PackageVersion|InformationalVersion` in a project's own text, and MinVer sets the properties from its targets, never from a project file. `IsPackable` stays false repo-wide (`Directory.Build.props` line 20), MinVer stamps the assemblies and the release, and nothing is published.

`eng/native` is untouched. `eng/native/Directory.Build.props` line 2 states "Isolates eng/native packaging projects from the root build files" and the file imports nothing, and the root `PackageReference` never reaches the packaging projects. `eng/native/Directory.Build.targets` lines 27-33 run `EnsureManifestVersionMatch` before `GenerateNuspec` and error unless `$(Version)` equals the `version-string` in the library's manifest: `eng/native/z3/vcpkg.json` declares `"version-string": "5.0.0"` and `eng/native/Rasm.Native.Z3/Rasm.Native.Z3.csproj` line 3 declares `<Version>5.0.0</Version>`. The package version is the upstream native library pin. `tools/nx/native-packaging.ts` lines 138-139 read the last `<Version>` property from the parsed project file, lines 155-166 fail the project when it is absent, and line 237 computes the cached output path as `{workspaceRoot}/<source>/<name>.<version>.nupkg`. A version injected at build time is invisible to that parser.

`nbgv` 3.10.94 (2026-08-28), `GitVersion.Tool` 6.8.2 (2026-07-10), and `minver-cli` 7.0.0 are rejected as manifest entries, the version comes from a build package.

### [04.3]-[BINLOGTOOL]

Adopt `binlogtool` 1.0.33, published 2026-08-04, targeting `net10.0` (`src/BinlogTool/BinlogTool.csproj` in `KirillOsenkov/MSBuildStructuredLog`: `TargetFramework` `net10.0`, `PackAsTool` true, `ToolCommandName` `binlogtool`, `PackageId` `binlogtool`). Its command sources in `src/BinlogTool/`: `CompilerInvocations`, `DoubleWrites`, `DumpRecords`, `ListNuGet`, `ListProperties`, `ListTools`, `Redact`, `SaveFiles`, `SaveStrings`, `Search`, `Stats` (with `StatsHtml`).

Six duplicate the binlog MCP: `search` (`binlog_search`), `savefiles` (`binlog_files`), `listnuget` (`binlog_nuget`), `listproperties` (`binlog_properties`), `compilerinvocations` (`binlog_compiler`), `doublewrites` (`binlog_double_writes`). `listtools` overlaps `binlog_compiler` partially, and `savestrings` and `dumprecords` have no equivalent and no use here.

`stats` and `redact` have no equivalent anywhere. `stats` breaks down what takes up space in a binlog and answers a limitation the binlog MCP documents: "Above a 200 MB threshold the server therefore stops trying to open the whole log and answers from a subtree instead", nine tools are not substituted (`binlog_double_writes`, `binlog_diagnose`, `binlog_compare`, `binlog_compare_property`, `binlog_files`, `binlog_search_files`, `binlog_preprocess`, `binlog_assets`, per its own table), and it refuses "with an explanation rather than answering from data that cannot support the question" (nuget.org README, "Very large binlogs"). `CLAUDE.md` mandates the `dotnet-msbuild-diagnostics` skill for all `.binlog` work and forbids reading one directly, and when a full-solution binlog crosses that line, `stats` is the only way to find out why. `redact` strips secrets and credential patterns before a binlog is shared, and its consumer arrives with CI, where a failed build's binlog is uploaded as an artifact.

The third-round verdict names `binlogtool` in the manifest.

### [04.4]-[REMAINING_CANDIDATES]

`dotnet-suggest` 2.0.11 (2026-08-11) is shell tab-completion for `System.CommandLine` apps, registered per user in a shell profile, a machine concern, and no Rasm project is such an app.

`docfx` 2.78.5 (2026-02-23): `GenerateDocumentationFile` is true repo-wide (`Directory.Build.props` line 29) and `README.md` describes `docs/` as "durable documentation", but no site, publish target, or host is configured, and the last release is over six months old. Rejected.

`Roslynator.DotNet.Cli` 1.0.0 (2026-08-21): `Roslynator.Analyzers` 5.0.0 is a workspace analyzer (`Directory.Build.props` line 53, `Directory.Packages.props` line 13) under `TreatWarningsAsErrors` and `AnalysisLevel` `latest-all`, violations are build errors, and no backlog for a bulk `fix` accumulates. Single fixes go through the Roslyn MCP (`get_code_fixes`, `apply_code_action`).

`dotnet-project-licenses` 2.7.1 (2023-03-15) has three and a half years without a release, and its successor `nuget-license` 4.0.16 (2026-08-04) is rejected as well: the licensing policy accepts copyleft, gated, and noncommercial licenses with cost as the only blocker, and no compliance tooling enters Rasm.

`dotnet-t4` 3.0.0 (2024-09-03): code generation here is Roslyn source generators (`Riok.Mapperly` 5.0.0-next.10, `Directory.Packages.props` line 32, `Thinktecture.Runtime.Extensions` 10.5.0, line 34) with Python generators under `eng/scripts/` (`gen_gmsh_bindings.py`).

### [04.5]-[MCP_SERVERS]

Adopt both. Package ids and versions were confirmed through the `nuget` MCP on 2026-09-03, and target frameworks read from the local package cache. `RoslynCodeLens.Mcp` is at 2.18.0 (published 2026-08-21), includes tools for `net10.0` alone (`~/.nuget/packages/roslyncodelens.mcp/2.18.0/tools/`), and is declared in `.mcp.json` line 6 as `dotnet dnx RoslynCodeLens.Mcp Workspace.slnx`. `Microsoft.AITools.BinlogMcp` is at 3.0.2 (published 2026-08-26), includes tools for `net8.0` and `net10.0` (`~/.nuget/packages/microsoft.aitools.binlogmcp/3.0.2/tools/`), and is declared in `~/.claude.json` at user scope as `dotnet dnx Microsoft.AITools.BinlogMcp`. Both resolve the latest version at every launch.

`dotnet dnx` honors the manifest. From `docs/core/tools/dotnet-tool-exec.md` (dotnet/docs, `ms.date` 09/06/2025): "`dotnet tool exec` works seamlessly with both global and local tools. If you have a local tool manifest available, it uses the manifest to determine which version of the tool to run", and "`dotnet dnx` - A hidden alias for `dotnet tool exec` that is used as a way to easily implement the `dnx` script itself". Without a manifest the command checks "the version (or version range) you specify (or the latest version if none is specified)". Neither declaration specifies one.

Reasons to adopt both:

- Reproducibility is the workspace's configuration everywhere else
- The two components an agent depends on most are the only ones floating
- Both move fast: 2.18.0 on 2026-08-21 (its release notes: "run the SDK's source generators by matching its Roslyn version") and 3.0.2 on 2026-08-26
- The binlog package's README disclaims direct use, and a pin is the only defense against a breaking change from an author who disclaims compatibility
- The `roslyn-codelens` entry in `.mcp.json` needs no edit, the existing command resolves the pinned version once the manifest entry exists

Everywhere else means `RestoreLockedMode` under CI (`Directory.Build.props` line 35), `RestorePackagesWithLockFile` (line 34), `global.json` `rollForward: disable`, a pinned vcpkg commit, and pinned native release archives. The README disclaimer reads: "The tool is meant to be used solely by the plugins distributed via https://github.com/dotnet/skills, Microsoft does not guarantees and compatibility nor support for direct usages." `dotnet tool restore` in `eng:provision` then downloads both up front.

Settled follow-ups:

- The binlog server moves from user scope into `.mcp.json`, and a fresh clone gets the server the `dotnet-msbuild-diagnostics` skill requires
- Both `.mcp.json` entries set `DOTNET_CLI_TELEMETRY_OPTOUT=1` in their `env`

`.claude/settings.json` sets `"enableAllProjectMcpServers": true`, and the server is approved without a prompt. The intent that every Rasm-related tool belongs in Rasm's own manifests supports the move. The binlog server's README, "Telemetry configuration": "Telemetry collection is on by default", it sends "a structured log record per emit point to Microsoft via Azure Application Insights", and the "single canonical opt-out" is `DOTNET_CLI_TELEMETRY_OPTOUT`. The plan lists `DOTNET_CLI_TELEMETRY_OPTOUT` among the Forge couplings to cut, and the `.mcp.json` `env` is where the two declarations own it.

## [05]-[MANIFEST_MECHANICS]

### [05.1]-[TOOL_RESTORE]

`eng/scripts/provision.py` line 387 runs `["dotnet", "tool", "restore"]` at `REPO_ROOT`, `eng/project.json` defines `nx run eng:provision`, and `README.md` `[02]-[TASKS]` records it. Options on SDK 10.0.400 (`dotnet tool restore --help`): `--configfile`, `--add-source`, `--tool-manifest`, `-v|--verbosity`, `--disable-parallel`, `--ignore-failed-sources`, `--no-http-cache`, `--interactive`.

In CI, `dotnet tool restore` runs before `dotnet restore Workspace.slnx`, which `README.md` requires before `nx affected -t build test`, and the setup composite action in the CI report runs it on every job.

Sources need no change. Tool restore reads the same `NuGet.config`, with `packageSourceMapping` routing `*` to nuget.org and `Rasm.*` to `local`. Every manifest id matches `*`.

Scope: `"isRoot": true` stops the upward manifest search at the repository root: "The search ends when it finds the referenced tool or it finds a manifest file with `isRoot` set to `true`" (`docs/core/tools/local-tools-how-to-use.md`).

The docs state the configuration for a committed manifest (`docs/core/tools/global-tools.md`): "Make sure the tool manifest file is stored in a controlled location. The .NET CLI launches local tools with `dotnet tool run` based on the contents of the tool manifest. If the manifest is modified by an untrusted party, it could cause the CLI to run malicious code."

### [05.2]-[TOOL_RUN]

`dotnet tool run <name>` and `dotnet <name>` both resolve a local tool. `dotnet tool run --help` on 10.0.400: "Run a local tool. Note that this command cannot be used to run a global tool.", and `docs/core/tools/dotnet-tool-run.md`: "The `dotnet tool run` command searches tool manifest files that are in scope for the current directory."

The decisive difference is `rollForward`. From dotnet/sdk PR #37231 (the implementing PR, base branch `release/8.0.2xx`): "Note that to run the tool with `dotnet [toolname]`, the rollForward does not work." Tools with `rollForward` in their manifest entry are invoked as `dotnet tool run <command>`, uniformly in every Nx target and `eng/` script.

### [05.3]-[ROLL_FORWARD]

`rollForward` is a per-tool boolean beside `version` and `commands`, and `dotnet tool update` writes `"rollForward": false` into a tool's entry (dotnet/sdk issue #51922, opened 2025-11-26). `dotnet tool install|update --allow-roll-forward` writes it, "Available starting with .NET 9.0 SDK. Allow tool to use a newer version of the .NET runtime if the runtime it targets isn't installed" (`docs/core/tools/dotnet-tool-install.md`), and "This option configures the tool with roll-forward mode `Major`" (`docs/core/whats-new/dotnet-9/sdk.md`, ".NET tool roll-forward"). The schemastore schema for `dotnet-tools.json` lacks it (issue #51922), an editor can flag it, and the SDK writes and reads it regardless. `global.json`'s `rollForward: disable` pins the SDK, and the runtime a tool resolves is separate.

The measured target frameworks decide who needs it:

| [TOOL] | [TARGETS] | [ROLL_FORWARD] | [PROOF] |
| :----- | :-------- | :------------- | :------ |
| `dotnet-stryker` 4.16.0 | `net8.0` alone | Yes | `~/.nuget/packages/dotnet-stryker/4.16.0/tools/` contains `net8.0` alone |
| `dotnet-reportgenerator-globaltool` 5.5.11 | .NET 8.0 | Yes | nuget.org package page |
| `binlogtool` 1.0.33 | `net10.0` | No | `BinlogTool.csproj` |
| `RoslynCodeLens.Mcp` 2.18.0 | `net10.0` | No | Local package cache `tools/` listing |
| `Microsoft.AITools.BinlogMcp` 3.0.2 | `net8.0` and `net10.0` | No | Local package cache `tools/` listing |

With the three runtimes on the machine, nothing fails. A machine provisioned by mise from `global.json` has the .NET 10 SDK and its runtime, and a framework-dependent `net8.0` app does not roll to a newer major by default. `"rollForward": true` on the two `net8.0` tools is the portability fix, and `README.md` requires portability to Linux and Windows.

### [05.4]-[TOOL_UPDATE]

`dotnet tool update <packageId> --local` updates a tool. Options on 10.0.400 (`dotnet tool update --help`): `--local`, `--version <VERSION>`, `--configfile`, `--tool-manifest <PATH>`, `--add-source`, `--source`, `--prerelease`, `--allow-downgrade`, `--all`.

The manifest's version string is the pin. `RestorePackagesWithLockFile` governs project restore and writes `packages.lock.json`, it does not cover the tool manifest, and the committed JSON is the only lock the tools have.

`docs/core/tools/local-tools-how-to-use.md`: "The update command finds the first manifest file that contains the package ID and updates it... The search scope is up through parent directories until a manifest file with `isRoot = true` is found." Running the command anywhere inside Rasm edits the Rasm manifest and no other. Newest-version checks route through the `nuget` MCP per `CLAUDE.md`.

## [06]-[MANIFEST]

```json
{
    "version": 1,
    "isRoot": true,
    "tools": {
        "binlogtool": {
            "version": "1.0.33",
            "commands": ["binlogtool"]
        },
        "dotnet-reportgenerator-globaltool": {
            "version": "5.5.11",
            "commands": ["reportgenerator"],
            "rollForward": true
        },
        "dotnet-stryker": {
            "version": "4.16.0",
            "commands": ["dotnet-stryker"],
            "rollForward": true
        },
        "microsoft.aitools.binlogmcp": {
            "version": "3.0.2",
            "commands": ["binlog-mcp"]
        },
        "roslyncodelens.mcp": {
            "version": "2.18.0",
            "commands": ["roslyn-codelens-mcp"]
        }
    }
}
```

Five entries: two MCP servers the agent workflow depends on, two that serve the repository's own documents (`stryker-config.json`, `tests/README.md` line 98), and one that closes a limitation the binlog MCP documents about itself. MinVer is absent by design as a `PackageReference` in the root `Directory.Build.props`.

`dotnet tool install` writes the `commands` array from each package's `DotnetToolSettings.xml`. The three new entries are added with `dotnet tool install --local <id> --version <v>` and the resulting `commands` value accepted as written. `binlogtool` is confirmed from its project file (`ToolCommandName` `binlogtool`), `reportgenerator` from the nuget.org usage line `dotnet reportgenerator [options]`, and `binlog-mcp` from the README's global-tool setup (`"command": "binlog-mcp"`).

## [07]-[REJECTED_TOOLS]

Tools rejected without a section of their own:

| [TOOL] | [RELEASED] | [REASON] |
| :----- | :--------- | :------- |
| `CycloneDX` 6.2.0, `Microsoft.Sbom.DotNetTool` 4.1.5 | 2026-04-26, 2025-12-15 | No compliance tooling |
| `dotnet-trx` 1.0.1, `trx2junit` 2.1.0 | 2025-09-29, 2024-02-20 | MTP reporters produce the results, no other JUnit consumer |
| `dotnet-format` 5.1.250801 | 2021-10-11 | Superseded by the `dotnet format` SDK verb |
| `dotnet-affected` 6.2.0 | 2026-03-03 | `nx affected` drives change detection |
| `Nuke.GlobalTool` 10.1.0 | 2025-12-02 | Nx is the task graph, control flow belongs in `eng/scripts/*.py` |
| `dotnet-script` 2.0.1 | 2026-05-28 | Steps with control flow are Python under `eng/scripts/` |

MTP's console reporter and `TrxReport` produce the test results, and Vitest's `junit` reporter is the only JUnit consumer in the repository.

## [08]-[FORGE_REMOVAL_NOTE]

Forge stays untouched, and the following leaves later. Ten of Forge's `# --- [NET]` rows become dead for Rasm: `csharpier`, `dotnet-outdated`, `dotnet-ef`, `sharpfuzz`, `ilspycmd`, `nuget-to-json`, `dotnet-coverage`, `reportgenerator`, `dotnet-stryker`, and the four diagnostics tools. Two statements want correcting at the same time: `dev-tools.nix` lines 175-176 assert that "no repo carries a `.config/dotnet-tools.json` of its own", and `tests/README.md` line 110 names `Parametric_Forge dev-tools.nix` as the owner of ".NET CLI tools available on `PATH`". After the change the manifest owns them, and the `tests/README.md` row is a Rasm edit in the implementation phase.
