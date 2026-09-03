<!-- Source for monorepo-build-infrastructure [02]-[TOOLCHAIN], nothing integrated yet -->
# [MCP_COVERAGE_AND_TOOL_MANIFEST]

Versions were read on 2026-09-03 through the `nuget` MCP against the repository's `NuGet.config`.

## [01]-[REPOSITORY_DECLARATIONS]

### [01.1]-[SERVER_DECLARATIONS]

`.mcp.json` declares one server:

```json
{
    "mcpServers": {
        "roslyn-codelens": {
            "type": "stdio",
            "command": "dotnet",
            "args": ["dnx", "RoslynCodeLens.Mcp", "Workspace.slnx"],
            "env": {}
        }
    }
}
```

`~/.claude.json` declares the binlog server at user scope under `mcpServers`, beside `ast-grep`, `claudeCodeDocs`, `context7`, `exa`, `github`, `greptile`, `hostinger`, `nuget`, `openaiDeveloperDocs`, `playwright`, and `rhino-mcp-platform`:

```json
"binlog": { "type": "stdio", "command": "dotnet", "args": ["dnx", "Microsoft.AITools.BinlogMcp"], "env": {} }
```

Fresh clones of Rasm get `roslyn-codelens` from `.mcp.json` and no binlog server, although `CLAUDE.md` mandates the `dotnet-msbuild-diagnostics` skill, which names `Microsoft.AITools.BinlogMcp` as its server (`.claude/skills/dotnet-msbuild-diagnostics/SKILL.md` line 8).

`.claude/settings.json` sets `"enableAllProjectMcpServers": true`, a `.mcp.json` server is approved without a prompt, and the file declares no servers of its own.

### [01.2]-[TOOL_MANIFEST_AND_SDK]

`.config/dotnet-tools.json` is committed, `git ls-files .config/` lists it, and the status is clean:

```json
{ "version": 1, "isRoot": true, "tools": { "dotnet-stryker": { "version": "4.16.0", "commands": ["dotnet-stryker"] } } }
```

`global.json` pins `sdk.version` `10.0.400` with `rollForward: disable`, and `test.runner` `Microsoft.Testing.Platform`.

`README.md` `[02]-[TASKS]` states that `nx run eng:provision` restores the tool manifest, and the layout lists `.config/dotnet-tools.json` as ".NET local tool manifest, restored by the eng provision target".

## [02]-[MCP_SERVERS]

### [02.1]-[ROSLYNCODELENS_MCP]

`.mcp.json` names the package id `RoslynCodeLens.Mcp`. The nuget MCP `get_latest_package_version` reports 2.18.0, published 2026-08-21. The source repository is <https://github.com/MarcelRoozekrans/roslyn-codelens-mcp> (MIT, C#, created 2026-03-06). The github MCP `get_latest_release` reports tag `v2.18.0`, published 2026-08-21T17:12:55Z, with `RoslynCodeLens.Mcp.2.18.0.nupkg` (16,531,110 bytes) as an asset and the release note "run the SDK's source generators by matching its Roslyn version".

The README "Requirements" section names ".NET 10 SDK" and "A .NET solution with compilable projects". Its "Quick Start" section gives the install forms `dnx RoslynCodeLens.Mcp --yes`, `dotnet tool install -g RoslynCodeLens.Mcp`, `npx -y roslyn-codelens-mcp` (a launcher that "installs the `RoslynCodeLens.Mcp` .NET global tool at a matching version and execs it, so the .NET 10 SDK must be on `PATH`"), `claude install gh:MarcelRoozekrans/roslyn-codelens-mcp`, and Docker. The transport is stdio by default, and `--http` serves streamable HTTP on `127.0.0.1:3001`, "intended for single-user, local use", with no authentication (README, "HTTP transport").

The README "Features" list names the capabilities, and `.claude/skills/dotnet-roslyn-codelens/SKILL.md` `[02]-[TOOL_INDEX]` holds 67 rows drawn from it:
- Symbol navigation: `go_to_definition`, `search_symbols`, `find_references`
- `find_references` reports per occurrence, tagged by reference kind, with a server-side `kinds` filter
- Call navigation: `find_callers`, `find_implementations`, `find_event_subscribers`, `get_call_graph`, `resolve_stack_trace`
- Shape and source: `get_file_overview`, `get_type_overview`, `get_symbol_context`, `get_type_hierarchy`, `analyze_method`
- Method source: `get_method_source`
- Members: `get_overloads`, `get_extension_methods`, `get_operators`, `get_instantiation_options`
- Diagnostics and fixes: `get_diagnostics` (compiler and Roslyn analyzer diagnostics), `get_code_fixes`, `get_code_actions`
- Actions: `apply_code_action`
- Refactoring: `rename_symbol`, `change_signature`
- Quality audit: `get_complexity_metrics` (cyclomatic, cognitive, nesting), `find_naming_violations`, `find_async_violations`
- Disposal audit: `find_disposable_misuse`
- Structure audit: `find_large_classes`, `find_god_objects`, `find_unused_symbols`, `find_circular_dependencies`, `check_architecture`
- Project audit: `get_project_health`
- API surface: `get_public_api_surface`, `find_breaking_changes` ("Diff the current API against a baseline JSON or DLL")
- `find_breaking_changes` reports Breaking/NonBreaking severity
- Tests: `find_tests_for_symbol`, `get_test_summary`, `find_uncovered_symbols`, `generate_test_skeleton`
- Dependencies: `get_project_dependencies`, `get_nuget_dependencies`, `get_di_registrations`
- Generators: `get_source_generators`, `get_generated_code`
- Metadata and IL: `inspect_external_assembly`, `peek_il` (ICSharpCode.Decompiler, MIT, per "Third-party licenses")
- Flow: `analyze_data_flow`, `analyze_control_flow`, `analyze_change_impact`
- Exceptions: `get_exception_flow`, `find_throw_sites`, `find_catch_blocks`
- Usage: `find_attribute_usages`, `find_obsolete_usage`, `find_reflection_usage`
- Solutions: `list_solutions`, `load_solution`, `unload_solution`, `set_active_solution`, `rebuild_solution`
- Tasks and trust: `start_background_task`, `get_task_status`, `list_running_tasks`, `trust_solution`, `list_trusted_paths`, `revoke_trust`

`get_diagnostics` and `get_code_fixes` load analyzer DLLs in process. "Solutions passed on the CLI at startup are auto-trusted for the current session", Rasm passes `Workspace.slnx`, and analyzers run without a `trust_solution` call. "Analyzer DLLs must come from the user's NuGet global packages folder, the dotnet SDK install dir, or the solution's own `bin`/`obj`" (README, "Security: Trust Model").

The server is a workspace, not a build: "The server watches `.cs`, `.csproj`, `.props`, and `.targets` files for changes" and lazily re-compiles stale projects (README, "Hot Reload").

### [02.2]-[MICROSOFT_AITOOLS_BINLOGMCP]

`~/.claude.json` names the package id `Microsoft.AITools.BinlogMcp` under user-scope `mcpServers.binlog`. The nuget MCP `get_latest_package_version` reports 3.0.2, published 2026-08-26. The README "Bug reports" section points to <https://github.com/dotnet/skills>: "For bug reports and support please use the https://github.com/dotnet/skills repository". Its "Notice" section states: "The tool is meant to be used solely by the plugins distributed via https://github.com/dotnet/skills, Microsoft does not guarantees and compatibility nor support for direct usages. The MCP and its implementation are considered internal technical details of the msbuild skills."

The README "Setup" section gives the install forms `dotnet run --project .../Microsoft.AITools.BinlogMcp.csproj`, or "if installed as a global tool" invoked as `binlog-mcp`. The server "parses MSBuild binary logs using the StructuredLogger library" (README, "Overview").

The README "Tools (44)" section names the capabilities, and `.claude/skills/dotnet-msbuild-diagnostics/SKILL.md` `[02]-[BINLOG_TOOLS]` covers them:
- Investigation: `binlog_overview`, `binlog_errors`, `binlog_warnings`, `binlog_search` (StructuredLog Viewer DSL, tree output with stable node ids)
- Project state: `binlog_projects`, `binlog_properties`, `binlog_compare_property`, `binlog_items`, `binlog_imports`, `binlog_explain_property`
- Specialized diagnostics: `binlog_assembly_conflicts` (MSB3277 and RAR parameters), `binlog_diagnose`, `binlog_double_writes`, `binlog_nuget`
- Compiler and assets: `binlog_compiler` (Csc/Vbc/Fsc command lines and response files), `binlog_assets` (`project.assets.json`)
- `binlog_assets` covers frameworks, libraries, CPM transitive pins, and reverse dependency chains
- Files: `binlog_files`, `binlog_search_files`, `binlog_preprocess` (equivalent to `msbuild /pp`)
- Tree navigation: `binlog_explore_node`
- Targets and tasks: `binlog_project_targets`, `binlog_search_targets`, `binlog_target_reasons`, `binlog_tasks_in_target`, `binlog_task_details`
- Evaluations: `binlog_evaluations`, `binlog_evaluation_properties`, `binlog_evaluation_global_properties`
- Performance: `binlog_expensive_projects`, `binlog_expensive_targets`, `binlog_expensive_tasks`, `binlog_expensive_analyzers`
- Timing: `binlog_project_target_times`
- Comparison: `binlog_compare`
- Structured output and graphs: `binlog_capabilities`, `binlog_build_graph`, `binlog_target_graph`, `binlog_analyzer_summary`
- Incrementality: `binlog_incremental_analysis` (skipped versus rebuilt targets with inferred reason and triggering files)
- `binlog_incremental_analysis` reports `IncrementalClean` deletions
- Extraction: `binlog_extract_preview`, `binlog_extract` (writes a standalone `.binlog` of selected projects, "Works on binlogs too large to open")
- Server control: `list_mcp_instances`, `stop_instance`, `stop`
- Prompts: `analyze_errors`, `analyze_performance`

A CLI exists beside the server: `binlog-mcp index <log>`, `binlog-mcp index --force`, `binlog-mcp index-stats <log>`. "`index-stats` exits non-zero when any binlog is missing an index or its index is stale, which makes it usable as a CI check". The index is written to `$XDG_CACHE_HOME/binlog-mcp/index` or `~/.cache/binlog-mcp/index` on macOS and Linux (README, "Ahead-of-time indexing").

"Above a 200 MB threshold the server therefore stops trying to open the whole log and answers from a subtree instead." Nine tools are not substituted (`binlog_double_writes`, `binlog_diagnose`, `binlog_compare`, `binlog_compare_property`, `binlog_files`, `binlog_search_files`, `binlog_preprocess`, `binlog_assets` in the README's own table), and they refuse "with an explanation rather than answering from data that cannot support the question". `BINLOG_MCP_AUTO_EXTRACT_MB` changes the threshold (README, "Very large binlogs").

"Telemetry collection is on by default", and the server sends "a structured log record per emit point to Microsoft via Azure Application Insights". The "single canonical opt-out" is `DOTNET_CLI_TELEMETRY_OPTOUT` (README, "Telemetry configuration"). `.mcp.json` and `~/.claude.json` both declare `"env": {}` and neither sets it.

## [03]-[SDK_CAPABILITIES]

On this machine `dotnet --version` reports `10.0.400` and `dotnet --list-sdks` shows 8.0.424, 9.0.317, and 10.0.400. The pinned SDK provides:
- `dotnet format` formats and fixes code style, and `dotnet format --version` prints `10.0.400-servicing.26379.115+14fbf8d5`
- `dotnet package list --outdated` reports outdated packages, with `--include-prerelease`, `--highest-patch`, and `--highest-minor`
- `dotnet package list --vulnerable` reports vulnerable packages, per `dotnet package list --help`
- `dotnet package list --deprecated` reports deprecated packages, per the same help output
- `dotnet package list --format json --output-version <n>` writes a machine-readable package report
- `dotnet package search <term>` searches the configured sources, per `dotnet package search --help`
- `dotnet nuget why <project or solution> <package>` "Shows the dependency graph for a particular package for a given project or solution"
- `dotnet build -check` runs the BuildCheck analysis rules (`dotnet-msbuild-diagnostics/SKILL.md` `[01]-[CAPTURE]`)
- `-getProperty`, `-getItem`, and `-getTargetResult` extract evaluated properties and items without a build (MSBuild command-line reference)
- `dotnet msbuild -pp:<file>.xml` writes the preprocessed project with imports inlined
- `-bl:<dir>/<purpose>-{}.binlog` captures a binary log
- `-profileEvaluation:<file>.md` profiles evaluation
- `Microsoft.NET.ApiCompat.targets` checks API compatibility between assemblies during a build, in `<sdk>/10.0.400/Sdks/Microsoft.NET.Sdk/targets/`
- `Microsoft.NET.ApiCompat.ValidatePackage.targets` (`EnablePackageValidation`) validates a package (baseline, framework, strict mode)
- `Microsoft.NET.Sdk.SourceLink.props` and `.targets` in the same directory provide Source Link
- `ManagePackageVersionsCentrally`, `RestorePackagesWithLockFile`, and `RestoreLockedMode` provide central package management and locked restore
- `Directory.Packages.props` line 4 and `Directory.Build.props` lines 34-35 set them in Rasm
- `dotnet test` with `test.runner` `Microsoft.Testing.Platform` in `global.json` runs tests under the Microsoft Testing Platform
- `dotnet solution` edits solution files, per `dotnet --help`

## [04]-[MANIFEST_ENTRIES]

The nuget MCP supplied the versions and publish dates on 2026-09-03, the third-round manifest decision fixed the set, and the verified tool-manifest report records every rejected tool.

`dotnet-stryker` 4.16.0, published 2026-07-03, runs .NET mutation testing configured by the root `stryker-config.json` (`tests/README.md` row [08]). The pinned 4.16.0 is the current release.

`dotnet-reportgenerator-globaltool` 5.5.11, published 2026-07-27, merges Cobertura, OpenCover, and other coverage files into HTML, Markdown, badges, lcov, and a text summary, and `Reports` supports globbing. `coverlet.MTP` writes one Cobertura file per test project, its own documentation says "Report merging is not yet supported", `tests/README.md` line 98 requires one Cobertura aggregate under `.artifacts/`, and no threshold and no gate apply.

`binlogtool` 1.0.33, published 2026-08-04, is the binlog CLI from the Structured Log Viewer project, with command sources in `src/BinlogTool/`: `CompilerInvocations`, `DoubleWrites`, `DumpRecords`, `ListNuGet`, `ListProperties`, `ListTools`, `Redact`, `SaveFiles`, `SaveStrings`, `Search`, `Stats`. `search`, `savefiles`, `listnuget`, `listproperties`, `compilerinvocations`, and `doublewrites` have MCP equivalents, and `redact` (strip secrets before a binlog is shared) and `stats` (what consumes space, against the MCP's 200 MB threshold) do not.

`Microsoft.AITools.BinlogMcp` 3.0.2, published 2026-08-26, is the binlog MCP server with the `binlog-mcp index` and `index-stats` CLI, pinned so that `dotnet dnx` stops resolving "latest" at every launch. `RoslynCodeLens.Mcp` 2.18.0, published 2026-08-21, is the Roslyn MCP server, pinned for the same reason.

`coverlet.MTP` 10.0.1 (2026-05-18), `Microsoft.Testing.Extensions.TrxReport` 2.3.3, `Microsoft.Testing.Extensions.CrashDump` and `HangDump` 2.3.3, and `Microsoft.Testing.Platform` 2.3.3 are `PackageVersion` rows in `Directory.Packages.props`, and `tests/README.md` row [01] records coverage as "`coverlet.MTP` — `--coverlet` on run, writes to the run results directory". MinVer 7.0.0 (2026-01-05) supplies the repository release version as a `PackageReference` with `PrivateAssets="All"` in the root `Directory.Build.props`, and `minver-cli` is not installed.

## [05]-[MANIFEST_MECHANICS]

### [05.1]-[MANIFEST_RESOLUTION]

"The tools listed in a manifest file are available to the current directory and subdirectories... the SDK searches for a manifest file in the current directory and parent directories... The search ends when it finds the referenced tool or it finds a manifest file with `isRoot` set to `true`" (`docs/core/tools/local-tools-how-to-use.md`). Rasm's manifest sets `"isRoot": true`.

`dotnet tool restore` accepts `--tool-manifest <PATH>`, `--configfile`, `--add-source`, `--ignore-failed-sources`, `--disable-parallel`, `--no-http-cache`, `--interactive`, and `-v` (`dotnet tool restore --help` on 10.0.400). Rasm calls it at `eng/scripts/provision.py` line 387 through `nx run eng:provision`.

Restored tools run in the forms the same tutorial gives: `dnx <command>` ("When using dnx with a local tool manifest, it automatically uses the version specified in the manifest"), `dotnet <command>`, and `dotnet tool run <command>` ("searches tool manifest files that are in scope for the current directory", `docs/core/tools/dotnet-tool-run.md`). Tools with `rollForward` set in their entry run through `dotnet tool run`, and "to run the tool with `dotnet [toolname]`, the rollForward does not work" (dotnet/sdk PR #37231).

`--allow-roll-forward` on `dotnet tool install`, `update`, `run`, and `exec` is "Available starting with .NET 9.0 SDK" and configures roll-forward mode `Major`. `global.json`'s `rollForward: disable` pins the SDK band, and the runtime a tool resolves is a separate setting.

Tool restore reads the same `NuGet.config`, with `packageSourceMapping` routing `*` to nuget.org and `Rasm.*` to `.artifacts/nuget`, and every manifest id matches `*`. The manifest's exact version string is the pin, and `RestorePackagesWithLockFile` (`Directory.Build.props` line 34) governs project restore and does not cover the tool manifest.

### [05.2]-[DNX_AND_LOCAL_TOOLS]

`dotnet dnx` is "A hidden alias for `dotnet tool exec` that is used as a way to easily implement the `dnx` script itself" (`docs/core/tools/dotnet-tool-exec.md`), and `dnx` is "A shell script provided by the installer and available on `PATH`". On this machine the script is absent and `dotnet dnx` works. The command "Checks the version (or version range) you specify (or the latest version if none is specified) against your configured NuGet feeds", downloads to the NuGet cache if absent, invokes the tool, and returns its exit code.

| [ASPECT] | [DNX] | [MANIFEST_WITH_RESTORE] |
| :-- | :-- | :-- |
| Version | Latest resolved at each launch, unless `@<version>` given | Exact, committed in `.config/dotnet-tools.json` |
| Reproducibility | None, clones can run different versions | Identical, reviewable in a diff |
| Restore step | Implicit per launch, needs the network on a cache miss | One `dotnet tool restore`, wired into `eng:provision` |
| System state | None | None, local tools stay off `PATH` |

The documentation states: "`dotnet tool exec` works seamlessly with both global and local tools. If you have a local tool manifest available, it uses the manifest to determine which version of the tool to run."

### [05.3]-[MCP_SERVER_PINS]

Adding `RoslynCodeLens.Mcp` 2.18.0 and `Microsoft.AITools.BinlogMcp` 3.0.2 to `.config/dotnet-tools.json` makes the existing `dotnet dnx` commands resolve the manifest version in place of "latest", by the sentence quoted from the documentation, without editing the `roslyn-codelens` entry. `dotnet tool restore` downloads both during `eng:provision` rather than at the first agent session (the Roslyn nupkg is 16,531,110 bytes).

Facts behind the decision:
- Both servers float until the manifest pins them
- Reproducibility is the workspace configuration: `RestoreLockedMode` under CI, the pinned vcpkg commit (`provision.py` `_VCPKG_COMMIT`)
- The pinned native release archives and `global.json` `rollForward: disable` are the same configuration
- The binlog package's author disclaims compatibility for direct use, and the pin is the only defense against a breaking change
- Both servers include a CLI beside the transport (`roslyn-codelens-mcp ... --http`, `binlog-mcp index` and `index-stats`)
- A manifest entry makes those subcommands runnable through `dotnet tool run` from a script or an Nx target

Settled follow-ups: the binlog server moves into `.mcp.json` so that fresh clones get it, and both `.mcp.json` entries set `DOTNET_CLI_TELEMETRY_OPTOUT=1` in `env`.

### [05.4]-[COVERAGE_SUMMARY]

- RoslynCodeLens MCP, pinned in the manifest, covers C# symbols, diagnostics, refactoring, IL, generators, API surface, architecture, and complexity
- Binlog MCP, pinned in the manifest, covers build evaluation, targets, tasks, timing, incrementality, restore, and assets
- The same server covers double writes and assembly conflicts
- SDK 10.0.400 covers formatting, package reports, API compat during build, Source Link, CPM, lock files, BuildCheck, and `-getProperty`
- `nuget` MCP (user scope) covers package version resolution and upgrade
- `dotnet-stryker` in the manifest covers mutation testing
- `dotnet-reportgenerator-globaltool` in the manifest covers the merged coverage report across projects, with no threshold
- `binlogtool` in the manifest covers binlog secret redaction before sharing and binlog size analysis
- MinVer `PackageReference` in the root `Directory.Build.props` covers the repository release version from the git tag
- Nothing covers runtime profiling, dumps, GC heaps, and counters, by decision, until an application or plugin project exists
- Nothing covers validation of a produced `.nupkg`, license reports, SBOM, and whole-assembly decompilation to disk, by decision
- No publishing and no compliance tooling exist, and `inspect_external_assembly` and `peek_il` answer the reading need
