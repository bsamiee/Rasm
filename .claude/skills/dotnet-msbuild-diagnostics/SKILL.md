---
name: dotnet-msbuild-diagnostics
description: "Use when diagnosing a .NET build from its .binlog: capture switches, binlog MCP tools, failure triage, BuildCheck codes, shared output paths, performance captures."
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

Covers binary log capture, queries through the `binlog` MCP server (`Microsoft.AITools.BinlogMcp`), failed build triage, BuildCheck, shared output paths and second project instances, and build performance. `.binlog` files are binary, query them through the MCP tools and never through `cat` or `strings`.

- `dotnet-msbuild-evaluation` owns the fix for a property, item, condition, or import that evaluates to a wrong value
- `dotnet-msbuild-execution` owns the fix for a target, `DependsOn` chain, `Inputs` and `Outputs`, or `FileWrites`
- `dotnet-msbuild-antipatterns` owns the corrected file beside each detected defect
- `dotnet-msbuild-packaging` owns restore, lock files, central package management diagnostics, and CI logger flags
- `monorepo-build-infrastructure` owns the `eng/` directory, task runner targets, native packaging projects, and provisioning
- `dotnet-roslyn-codelens` owns compiler and analyzer diagnostics in C# source

[AGENT]: `msbuild-debugger` takes one build symptom and returns cause, change, and proof. Use it when binlog output fills the context window.

[REFERENCES]:
- [01]-[EXECUTION_PERFORMANCE](references/execution-performance.md): Scheduling and task cost measured as a delta between two comparable captures
- [02]-[EVALUATION_AND_INCREMENTALITY](references/evaluation-and-incrementality.md): Evaluation cost, repeat evaluations, and no-change build work

## [01]-[CAPTURE]

Pass `-bl:<dir>/<purpose>-{}.binlog` on every MSBuild invocation, where `<dir>` is an artifacts directory the build owns, and MSBuild replaces `{}` with a UTC date, time, process id, and random string, which keeps one file per invocation. Every `dotnet` command that runs MSBuild accepts the switch.

| [INDEX] | [SWITCH]                                             | [EFFECT]                                                                 |
| :-----: | :--------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `-bl:<dir>/<purpose>-{}.binlog`                      | One file per invocation, imports embedded                                |
|  [02]   | `-bl`                                                | Writes and overwrites `msbuild.binlog` in the current directory          |
|  [03]   | `-bl:LogFile=<path>.binlog;ProjectImports=ZipFile`   | Imports go to `<name>.ProjectImports.zip` beside the log                 |
|  [04]   | `-bl:<path>.binlog;ProjectImports=None`              | No imports, `binlog_files` and `binlog_search_files` then return nothing |
|  [05]   | `-tl:off`                                            | Console logger with the `BinaryLogger wrote to:` line                    |
|  [06]   | `-check`                                             | BuildCheck reports as build diagnostics                                  |
|  [07]   | `--no-restore -graph -isolate`                       | Static graph build, `MSB4252` on an undeclared project instance          |
|  [08]   | `-p:Name=Value`                                      | Global property for the restore pass and the build pass                  |
|  [09]   | `-restoreProperty:Name=Value`                        | Global property for the restore pass only, the build pass reads empty    |
|  [10]   | `-pp:<file>.xml`                                     | Every import expanded in place with file boundaries, no build            |
|  [11]   | `-getProperty:A,B -getItem:C -getResultOutputFile:f` | Evaluated values as JSON in `f`, no build, one project file per call     |
|  [12]   | `-t:X -getTargetResult:X`                            | Runs `X` and prints its returned items as JSON                           |
|  [13]   | `-profileEvaluation:<file>.md`                       | Evaluation time per element, `.md` gives a markdown table                |
|  [14]   | `-v:diag`                                            | One `Property reassignment:` message per overwritten property            |
|  [15]   | `MSBuildDebugEngine=1` with `MSBUILDDEBUGPATH=<dir>` | Every MSBuild process writes a binlog under `<dir>/.MSBuild_Logs/`       |

- `-bl` values without the `.binlog` extension fail before the build with `MSB1029`
- The terminal logger prints no log path, pass `-tl:off` when the console must show it
- `-f` and `-p:TargetFramework=` run restore as a separate invocation with its own log, and a fixed name keeps only the last one
- `--no-restore` needs an assets file, and without one the build fails with `NETSDK1004`
- Failed builds keep their log and need no re-run for analysis
- Analyze `<name>.binlog`, the build log, because `dotnet test` under Microsoft.Testing.Platform writes `<name>-dotnet-test.binlog` for its own project evaluation
- `binlog_overview` reports that second file as `FAILED` with MSBuild `unknown` and one `Build failed.` error, and it holds no test run, because the platform runs test modules outside MSBuild
- `MSBuildDebugEngine` is case-sensitive on macOS, covers a build that a tool starts without `-bl`, writes `CentralNode_dotnet_PID=<pid>_<arch>_BuildManager_Default.binlog`, and without `MSBUILDDEBUGPATH` writes under `.MSBuild_Logs/` in the current directory
- `-check`, `-profileEvaluation`, and `-v:diag` write no binlog of their own, and `-bl` beside them captures the run
- `dotnet msbuild` prints the `BinaryLogger wrote to:` line only at `-v:n` or higher
- `git clean -fdx` deletes the logs with every other ignored file, and `-e "*.binlog"` keeps them

```bash
dotnet build Solution.slnx -tl:off -bl:artifacts/logs/build-{}.binlog         # One log per invocation, path printed at the end
dotnet build Solution.slnx -tl:off -check -bl:artifacts/logs/check-{}.binlog  # BuildCheck reports on the console and in the log
dotnet test --project Item.Tests/Item.Tests.csproj -bl:artifacts/logs/test-{}.binlog
dotnet msbuild Item/Item.csproj -getProperty:OutputPath -getItem:Compile -getResultOutputFile:artifacts/logs/item.json
```

## [02]-[BINLOG_TOOLS]

Run the tools in order: `binlog_overview`, then `binlog_diagnose` on a failed build, then `binlog_errors`, then `binlog_warnings`, then the drill-down tool the finding names.

| [INDEX] | [TOOL]                                | [PURPOSE]                                                                      |
| :-----: | :------------------------------------ | :----------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Server contract version and the tools that emit the JSON envelope              |
|  [02]   | `binlog_overview`                     | Status, duration, MSBuild version, project count, error and warning counts     |
|  [03]   | `binlog_diagnose`                     | Failed targets and errors grouped by root cause                                |
|  [04]   | `binlog_errors`                       | Deduplicated errors, `category` filter, task output on `include_task_output`   |
|  [05]   | `binlog_warnings`                     | Deduplicated warnings, filtered by `code` or `category`                        |
|  [06]   | `binlog_projects`                     | Every project with status and duration                                         |
|  [07]   | `binlog_evaluations`                  | One entry per evaluation with id and duration, filtered by `project`           |
|  [08]   | `binlog_evaluation_properties`        | Properties of one `evaluation_id`, filtered by `property_names`                |
|  [09]   | `binlog_evaluation_global_properties` | Global properties of one `evaluation_id`                                       |
|  [10]   | `binlog_properties`                   | Key properties of a project, or the ones that match `filter`                   |
|  [11]   | `binlog_explain_property`             | Final value of one property and every source that assigned it                  |
|  [12]   | `binlog_compare_property`             | One property across every project: differs, set, inconsistent, not set         |
|  [13]   | `binlog_items`                        | Items of one `itemType` for a project, or the item types when omitted          |
|  [14]   | `binlog_imports`                      | Import chain of a project with each missing import marked                      |
|  [15]   | `binlog_preprocess`                   | Source of the project file itself, not the `-pp` expansion                     |
|  [16]   | `binlog_files`                        | Embedded source files, listed or read by `filePath` and line range             |
|  [17]   | `binlog_search_files`                 | Text or regex search across the embedded source files                          |
|  [18]   | `binlog_search`                       | Build event search in the StructuredLog query syntax                           |
|  [19]   | `binlog_explore_node`                 | Ancestors, details, and children of one `node_id`                              |
|  [20]   | `binlog_project_targets`              | Targets of one project with timing and skip status                             |
|  [21]   | `binlog_search_targets`               | Targets by name substring across every project, with `skipped` per instance    |
|  [22]   | `binlog_target_reasons`               | Trigger, dependency chain, and durations of one target                         |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline of one evaluation, addressed as `eval-<id>`           |
|  [24]   | `binlog_tasks_in_target`              | Tasks inside one target of a project                                           |
|  [25]   | `binlog_task_details`                 | Parameters and messages of one task by `project`, `target_name`, `task_name`   |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration                                  |
|  [27]   | `binlog_expensive_targets`            | Slowest targets, aggregated by name                                            |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks, aggregated by name                                              |
|  [29]   | `binlog_project_target_times`         | Target timing of one project                                                   |
|  [30]   | `binlog_expensive_analyzers`          | Returns `[]`, use `binlog_analyzer_summary`                                    |
|  [31]   | `binlog_analyzer_summary`             | Time and invocation count per analyzer from a `ReportAnalyzer=true` build      |
|  [32]   | `binlog_build_graph`                  | Project dependency graph with durations and the critical path                  |
|  [33]   | `binlog_incremental_analysis`         | Skip or rebuild decision per target, and `IncrementalClean` deletions          |
|  [34]   | `binlog_double_writes`                | Files two `Copy` tasks wrote and directories two projects copied into          |
|  [35]   | `binlog_assembly_conflicts`           | `MSB3277` warnings with the `ResolveAssemblyReference` inputs behind them      |
|  [36]   | `binlog_compiler`                     | `Csc`, `Vbc`, and `Fsc` command lines with response files                      |
|  [37]   | `binlog_nuget`                        | Restore diagnostics, packages, versions, sources, and restore duration         |
|  [38]   | `binlog_assets`                       | `project.assets.json` frameworks, libraries, reverse dependencies of `package` |
|  [39]   | `binlog_compare`                      | Property and package diff between two binlogs                                  |
|  [40]   | `binlog_extract_preview`              | Size and project count of a subtree extraction, without a write                |
|  [41]   | `binlog_extract`                      | Standalone `.binlog` of the selected projects, without embedded source files   |
|  [42]   | `list_mcp_instances`                  | Running server instances with memory and `isOrphaned`                          |
|  [43]   | `stop_instance`                       | Stop one instance by PID                                                       |
|  [44]   | `stop`                                | Stop this instance                                                             |

- The reader drops records that a newer MSBuild wrote, and `binlog_warnings` reports the loss as one warning, `Skipped some data unknown to this version of Viewer`, which `binlog_overview` includes in its warning count
- `binlog_extract` refuses a log with those records unless `allow_unsupported_records=true`, and the extract then omits them
- `binlog_analyzer_summary`, `binlog_incremental_analysis`, and `binlog_task_details` accept no size limit, and on a solution the result goes to a file
- `binlog_explore_node` addresses a different node than the `[id]` that `binlog_search` printed, and `binlog_search_targets`, `binlog_tasks_in_target`, and `binlog_projects` give ids it accepts
- `binlog_target_reasons` prints `Chain: skipped. Previously built successfully.` for a target that ran, and the `skipped` field of `binlog_search_targets` decides
- `binlog_double_writes` reads performed copies only, a `Copy` that skipped an unchanged file is not a write, and it misses a second instance of one project and a copy-local file that two projects copy, while `BC0102` reports both
- The solution node adds one synthetic `Build failed.` error to every failed count, and `binlog_search_targets` matches the requested target name on that node
- `binlog_files` reads the embedded copy of a file, the file as the build saw it, not the file on disk
- `binlog_properties` can answer from the restore evaluation, which `MSBuildIsRestoring=True` in its output shows, and `binlog_evaluation_properties` on the build-pass evaluation id reads a value that differs between passes
- Stale server instances hold their binlogs in memory until `stop_instance` on each `isOrphaned` entry of `list_mcp_instances` stops them

### [02.1]-[SEARCH_SYNTAX]

`binlog_search` matches nodes, and a match on a target or task includes its child messages up to `context` levels, where task output, copy details, and up-to-date reasons are.

| [INDEX] | [QUERY]                       | [MATCHES]                                             |
| :-----: | :---------------------------- | :---------------------------------------------------- |
|  [01]   | `$error`, `$warning`          | Every error or warning node                           |
|  [02]   | `$task Csc`                   | Every invocation of that task                         |
|  [03]   | `$target Build`               | Every target with that name                           |
|  [04]   | `$project Item`               | Every project with the text in its name               |
|  [05]   | `under($project Item) CS1234` | Nodes under that project that contain the text        |
|  [06]   | `$task $time`                 | Tasks with timing, slowest first                      |
|  [07]   | `"exact phrase"`              | The literal text, including the messages MSBuild logs |
|  [08]   | `name=value`                  | Field match, for example a property assignment        |

### [02.2]-[LARGE_LOGS]

For a log above about 200 MB, which exceeds what one server instance holds, extract the subtree first:
1. Run `binlog_extract_preview` with `selector` set to `errors`, `warnings` with a `warning_code`, `project` with a `project_filter`, or `project_context_id`
2. Run `binlog_extract` with the same selection, an `output_file`, and the `plan_token` from the preview, which is valid for ten minutes in the same server process
3. Query the extract with the same tools, and read its `skippedUnsupportedRecords` count as the records the extract omits
4. Add `include_descendants=true` to keep every project the selection built, and `include_ancestors=true` to keep the full contents of its callers

## [03]-[FAILED_BUILD_TRIAGE]

Start at `binlog_diagnose`, then route the error class by the table and fix the first error before the next capture, because a failed restore stops every project and a failed reference blocks its dependents.

| [INDEX] | [SYMPTOM]                               | [FIRST_TOOL]                                 | [NEXT_STEP]                                   |
| :-----: | :-------------------------------------- | :------------------------------------------- | :-------------------------------------------- |
|  [01]   | `CS*` or `FS*` compiler error           | `binlog_errors`, then `binlog_compiler`      | `dotnet-roslyn-codelens` `get_diagnostics`    |
|  [02]   | `CA*`, `IDE*`, `RS*` analyzer error     | `binlog_errors`                              | `get_diagnostics`, `includeAnalyzers=true`    |
|  [03]   | `MSB3073`, the reason is in task output | `binlog_errors`, `include_task_output=true`  | Fix the tool input the task output names      |
|  [04]   | `MSB4019` import not found              | `binlog_imports`                             | `dotnet-msbuild-evaluation`, import path      |
|  [05]   | `MSB4057` target does not exist         | `binlog_project_targets` on that project     | `dotnet-msbuild-execution`, target name       |
|  [06]   | `MSB4092` or `MSB4113` condition        | `binlog_errors`, the file and line           | `dotnet-msbuild-evaluation`, condition form   |
|  [07]   | `MSB4252` under `-isolate`              | The error names both global-property sets    | Declare the edge or remove the extra property |
|  [08]   | `MSB3026` copy retry or a file lock     | `binlog_double_writes`, shared output paths  | `dotnet-msbuild-antipatterns` for the fix     |
|  [09]   | `NU1*` restore                          | `binlog_nuget`, then `binlog_assets`         | `dotnet-msbuild-packaging`, version graph     |
|  [10]   | `NETSDK1004` assets file missing        | The command line                             | Remove `--no-restore` or restore first        |
|  [11]   | `NETSDK1005` no target for framework    | `binlog_evaluations`, then global properties | `dotnet-msbuild-antipatterns`, build graph    |
|  [12]   | Other `NETSDK*`                         | `binlog_explain_property`, named property    | `dotnet-msbuild-evaluation`, the assignment   |
|  [13]   | `MSB3277` assembly version conflict     | `binlog_assembly_conflicts`                  | `binlog_assets` with `package`, both chains   |
|  [14]   | One target ran twice                    | `binlog_search_targets` on the target        | Shared output paths, two evaluations          |
|  [15]   | One file written by two projects        | `dotnet build -check`, `BC0102`              | Shared output paths, then the antipatterns    |
|  [16]   | One property has the wrong value        | `binlog_explain_property`                    | `dotnet-msbuild-evaluation`, evaluation order |
|  [17]   | One target never ran, build succeeded   | `binlog_search` for the `BeforeTargets` text | Fix the target name                           |
|  [18]   | Failed status with no error record      | `binlog_overview` failing project            | `binlog_project_targets`, the failed target   |
|  [19]   | Native asset missing at run time        | `binlog_assets` with `package`               | `binlog_items` on `NativeCopyLocalItems`      |
|  [20]   | The build is slow                       | `binlog_expensive_projects`                  | `references/execution-performance.md`         |
|  [21]   | One analyzer dominates the build        | `binlog_analyzer_summary`                    | Capture with `-p:ReportAnalyzer=true` first   |

- `MSB3073` says only `exited with code 1`, and the line that explains it is a plain message under the task that no error list contains unless the tool printed it in canonical `error:` form
- The `BeforeTargets` message reads `The target "X" listed in a BeforeTargets attribute at "<file> (line,col)" does not exist in the project, and will be ignored`, and it is neither a warning nor an error
- Empty `binlog_errors` results do not prove a clean build, because a target can fail without an error record, and the `binlog_overview` status decides
- `binlog_diagnose` counts distinct root causes by code, file, and line, and one error that repeats per target framework is one cause
- The `category` filter of `binlog_errors` and `binlog_warnings` takes `Compiler`, `NuGet`, `NETSDK`, `SDKResolvers`, `MSBuildEvaluation`, `MSBuildExecution`, `MSBuildGeneral`, `NativeToolchain`, `BuildCheck`, `Tasks`, `CodeAnalysis`, `WPF`, `Razor`, `AspNet`, or `Other`
- `binlog_compare_property` compares a wrong value in one project across every project and names the projects the solution never passed `Configuration` to
- Native assets that a package holds only under `build/` reach direct consumers through the package's `.targets` and never a transitive consumer, and `binlog_items` on `NativeCopyLocalItems` in the consumer that runs the code decides

## [04]-[BUILDCHECK]

`dotnet build -check` runs every inbox check and reports each finding as a build diagnostic with a `BC` code, and nothing runs without the switch. The checks belong to MSBuild, and `dotnet build`, `dotnet msbuild`, and a replay run one set.

| [INDEX] | [CODE]   | [REPORTS]                                                             | [DEFAULT]           |
| :-----: | :------- | :-------------------------------------------------------------------- | :------------------ |
|  [01]   | `BC0101` | Two projects with one `OutputPath` or `IntermediateOutputPath`        | Warning             |
|  [02]   | `BC0102` | Two tasks that write one file, across projects or instances           | Warning             |
|  [03]   | `BC0103` | Property values read from an environment variable                     | Suggestion, project |
|  [04]   | `BC0104` | `Reference` to another project's output instead of `ProjectReference` | Warning             |
|  [05]   | `BC0105` | `EmbeddedResource` without `Culture` or `WithCulture=false` metadata  | Warning             |
|  [06]   | `BC0106` | `CopyToOutputDirectory="Always"` on an item                           | Warning             |
|  [07]   | `BC0107` | `TargetFramework` and `TargetFrameworks` both set                     | Warning             |
|  [08]   | `BC0108` | `TargetFramework` or `TargetFrameworks` in a project without the SDK  | Warning             |
|  [09]   | `BC0201` | Property reads that no declaration precedes                           | Warning, project    |
|  [10]   | `BC0202` | Property reads before the declaration that follows them               | Warning, project    |
|  [11]   | `BC0203` | Properties declared in the project and never read                     | None, project       |
|  [12]   | `BC0302` | `Exec` that runs `dotnet`, `msbuild`, or `nuget` to build a project   | Warning             |

- Suggestions print as `message` lines on the console logger at `-v:m` and higher
- `BC0201` and `BC0202` skip a read inside a `Condition` and a read of the property's own value, and `AllowUninitializedPropertiesInConditions=false` includes conditions, with the same value on both codes
- The project scope covers the project file only, and `scope=all` extends `BC0201`, `BC0202`, and `BC0203` to every import
- `BC0101` and `BC0102` report across nodes, and `-m:1` is not needed
- `-check` on a replay, `dotnet build <log>.binlog -check`, re-runs the checks over the stored events, writes no file, prints the original `BinaryLogger wrote to:` line, and doubles every count because the stored reports replay with them
- `binlog_warnings` with `category=BuildCheck` lists the reports of a `-check` capture with the same counts as the console
- `-check` reports on an incremental build, because the checks read declared paths and task inputs, not performed writes

`.editorconfig` configures each code under a section header, and MSBuild ignores a key outside a section:

```ini
[*.csproj]
build_check.BC0101.severity = error
build_check.BC0106.severity = none
build_check.BC0201.scope = all
build_check.BC0201.AllowUninitializedPropertiesInConditions = false
build_check.BC0202.AllowUninitializedPropertiesInConditions = false
```

- `severity` takes `default`, `none`, `suggestion`, `warning`, or `error`, and `scope` takes `project_file`, `work_tree_imports`, or `all`
- `MSBuildTreatWarningsAsErrors` and `-warnAsError` turn every reported warning into a build failure, and a code the build accepts gets `severity = none` instead of a lowered switch

### [04.1]-[WORKFLOW]

1. Run `dotnet build <solution> -t:Rebuild -tl:off -check -bl:<dir>/check-{}.binlog`
2. Read each `BC` line on the console, or run `binlog_warnings` with `category=BuildCheck` on the log
3. Fix the file the report names, where `BC0201` and `BC0202` name `file(line,col)` and `BC0101` and `BC0102` name the path and both projects
4. Run the same command again and confirm the code is gone from the console and the log

## [05]-[SHARED_OUTPUT_PATHS]

MSBuild creates one project instance per project path and global-property set. Two instances with one `OutputPath` or `IntermediateOutputPath`, or two projects with one directory, succeed more often than they fail: one project consumes the other's `project.assets.json`, `MSB3026` copy retries and file locks appear in parallel builds, and outputs disappear or come from the wrong instance. The console and `binlog_diagnose` show nothing on a successful build, and this order detects them:

1. Run the BuildCheck workflow command, then read `BC0101` for each shared directory and `BC0102` for each file two tasks wrote, where `Library.csproj and Library.csproj` names a second instance of one project
2. Run `binlog_compare_property` on `IntermediateOutputPath`, then `OutputPath`, where an absolute value that groups two projects is the shared directory, the relative SDK default groups every project and means nothing, and the tool never reports two instances of one project
3. Run `binlog_double_writes` for the directories that more than one project copied into, which covers projects `binlog_compare_property` reports as `NOT SET`
4. Run `binlog_search_targets` on `CoreCompile`, where two `skipped: false` rows for one project file are two instances
5. Run `binlog_evaluations` with the `project` filter, then `binlog_evaluation_global_properties` per evaluation, discard the restore pass marked `MSBuildIsRestoring`, and the global property that differs between the remaining evaluations names the extra instance
6. Run `dotnet build --no-restore -graph -isolate`, which turns an instance the graph did not declare into `MSB4252`

| [INDEX] | [GLOBAL_PROPERTY]                      | [IN_THE_PATH] | [MEANING]                                                                |
| :-----: | :------------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Configuration`                        | Always        | One path per configuration                                               |
|  [02]   | `TargetFramework`                      | Conditional   | Appended while `AppendTargetFrameworkToOutputPath` is `true`             |
|  [03]   | `RuntimeIdentifier`                    | Conditional   | Appended while `AppendRuntimeIdentifierToOutputPath` is `true`           |
|  [04]   | `Platform`                             | Conditional   | Non-default platforms add a segment, never under the artifacts layout    |
|  [05]   | `SolutionFileName`, `SolutionPath`     | No            | Different values mark one project built from two solutions               |
|  [06]   | `CurrentSolutionConfigurationContents` | No            | The project entries of the solution, the entry count tells two apart     |
|  [07]   | `MSBuildIsRestoring`                   | No            | The restore pass, expected and discarded                                 |
|  [08]   | `BuildProjectReferences`               | No            | Reference queries where only `Get*` targets ran, or `--no-dependencies`  |
|  [09]   | `_IsPublishing`                        | No            | Set by `dotnet publish`, an `<MSBuild>` call that passes it builds twice |
|  [10]   | `PublishReadyToRun`                    | No            | Publish setting that adds an instance without a path change              |

| [INDEX] | [SIGNAL]                                      | [CAUSE]                                   | [FIX]                                      |
| :-----: | :-------------------------------------------- | :---------------------------------------- | :----------------------------------------- |
|  [01]   | `BC0102` names `Csc` twice from one project   | `AppendTargetFrameworkToOutputPath=false` | Remove the append property                 |
|  [02]   | Two projects at one absolute `obj` or `bin`   | One `Base*OutputPath` for every project   | One directory per project                  |
|  [03]   | Evaluations differ only in `SolutionFileName` | One project in two solutions of one build | One solution per build, or a filter        |
|  [04]   | Evaluations differ only in `_IsPublishing`    | `<MSBuild>` passes `_IsPublishing=true`   | `dotnet-msbuild-antipatterns`, build graph |
|  [05]   | `TargetFramework` differs, single-targeting   | `SetTargetFramework` on the reference     | `dotnet-msbuild-antipatterns`, build graph |
|  [06]   | Properties outside the path differ            | Extra `Properties` on an `<MSBuild>` call | `GlobalPropertiesToRemove` on the edge     |

- `binlog_search` with `$task MSBuild` lists each call under the project that makes it, and `GlobalPropertiesToRemove` on a `ProjectReference` strips a property from the referenced build and never one a project passes to itself

```xml
<!-- BAD: the append is off and both inner builds write one bin/ and one obj/ -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/</OutputPath>

<!-- GOOD: the SDK appends the framework to OutputPath and IntermediateOutputPath -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
```

- `AppendTargetFrameworkToOutputPath=false` drops the framework from `IntermediateOutputPath` and `OutputPath` both, an `OutputPath` that contains `$(TargetFramework)` still shares `obj/`, and reading `$(TargetFramework)` in the project body adds `BC0202`

### [05.1]-[SHARED_DIRECTORY_ACROSS_PROJECTS]

Restore writes `project.assets.json` under `MSBuildProjectExtensionsPath`, which defaults to `BaseIntermediateOutputPath` with no framework segment, and two projects with one `obj` share one assets file that the last restore overwrites. One shared `bin` alone lets the second project's copy of a common dependency skip as unchanged, and the build succeeds with `BC0102` as the only report.

```xml
<!-- BAD: Directory.Build.props sets every project's bin/ and obj/ to one directory -->
<BaseOutputPath>../SharedOutput/</BaseOutputPath>
<BaseIntermediateOutputPath>../SharedObj/</BaseIntermediateOutputPath>

<!-- GOOD: one directory per project, or ArtifactsPath for the whole tree -->
<BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
<BaseOutputPath>$(MSBuildThisFileDirectory)bin/$(MSBuildProjectName)/</BaseOutputPath>
```

- See `dotnet-msbuild-evaluation` for the artifacts layout under `ArtifactsPath`

## [06]-[BUILD_PERFORMANCE]

Compare two captures taken under the comparable capture conditions in `references/execution-performance.md`, then follow the reference the evidence selects:
1. Run `binlog_overview` on each capture and record status, duration, and project count
2. Run `binlog_expensive_projects`, `binlog_expensive_targets`, and `binlog_expensive_tasks` on the slow capture
3. For a slow project chain, target, or task, follow `references/execution-performance.md`
4. For slow evaluation or a target that runs in a no-change build, follow `references/evaluation-and-incrementality.md`
5. When both contribute, complete both workflows and capture again after each change
