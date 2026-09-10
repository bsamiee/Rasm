---
name: dotnet-msbuild-diagnostics
description: "Use when diagnosing a .NET build from a .binlog, covering capture, MCP tools, failure triage, BuildCheck, shared output paths, and performance."
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

Diagnosing a build from its binary log, from capture through the `binlog` MCP server (`Microsoft.AITools.BinlogMcp`) to build performance. `.binlog` files are binary, the MCP tools read them.

[REFERENCES]:
- [01]-[EXECUTION_PERFORMANCE](references/execution-performance.md): Scheduling and task cost measured as a delta between two comparable captures
- [02]-[EVALUATION_AND_INCREMENTALITY](references/evaluation-and-incrementality.md): Evaluation cost, repeat evaluations, and no-change build work

## [01]-[CAPTURE]

Pass `-bl:<dir>/<purpose>-{}.binlog` on every MSBuild invocation. `<dir>` is an artifacts directory the build owns, MSBuild replaces `{}` with a UTC date, time, process id, and random string, one file per invocation. Every `dotnet` command that runs MSBuild accepts the switch.

| [INDEX] | [SWITCH]                                             | [EFFECT]                                                                 |
| :-----: | :--------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `-bl:<dir>/<purpose>-{}.binlog`                      | One file per invocation, imports embedded                                |
|  [02]   | `-bl`                                                | Writes and overwrites `msbuild.binlog` in the current directory          |
|  [03]   | `-bl:LogFile=<path>.binlog;ProjectImports=ZipFile`   | Imports go to `<name>.ProjectImports.zip` beside the log                 |
|  [04]   | `-bl:<path>.binlog;ProjectImports=None`              | No imports, `binlog_files` and `binlog_search_files` then return nothing |
|  [05]   | `-check`                                             | BuildCheck reports as build diagnostics                                  |
|  [06]   | `--no-restore -graph -isolate`                       | Static graph build, `MSB4252` on an undeclared project instance          |
|  [07]   | `-p:Name=Value`                                      | Global property for the restore pass and the build pass                  |
|  [08]   | `-restoreProperty:Name=Value`                        | Global property for the restore pass, the build pass reads empty         |
|  [09]   | `-pp:<file>.xml`                                     | Every import expanded in place with file boundaries, no build            |
|  [10]   | `-getProperty:A,B -getItem:C -getResultOutputFile:f` | Evaluated values as JSON in `f`, no build, one project file per call     |
|  [11]   | `-t:X -getTargetResult:X`                            | Runs `X` and prints its returned items as JSON                           |
|  [12]   | `-profileEvaluation:<file>.md`                       | Evaluation time per element, `.md` gives a markdown table                |
|  [13]   | `-v:diag`                                            | One `Property reassignment:` message per overwritten property            |
|  [14]   | `MSBuildDebugEngine=1` with `MSBUILDDEBUGPATH=<dir>` | Every MSBuild process writes a binlog under `<dir>/.MSBuild_Logs/`       |

- A `-bl` value without the `.binlog` extension fails `MSB1029` before the build
- `-f` and `-p:TargetFramework=` run restore as a separate invocation with its own log, a fixed name keeps the last one
- `--no-restore` needs an assets file, `NETSDK1004` without one
- A failed build keeps its log
- `dotnet test` under Microsoft.Testing.Platform writes `<name>-dotnet-test.binlog` beside the build log `<name>.binlog`
- The `-dotnet-test` log holds the platform's project evaluation alone and reports `FAILED`
- `MSBuildDebugEngine` covers a build a tool starts without `-bl`, both variable names are case-sensitive on macOS
- Without `MSBUILDDEBUGPATH` the logs go under `.MSBuild_Logs/` in the current directory, the central node's file name starts with `CentralNode_`
- `-check`, `-profileEvaluation`, and `-v:diag` write no binlog of their own, `-bl` beside them captures the run
- `dotnet msbuild` prints the `BinaryLogger wrote to:` line at `-v:n` or higher
- `git clean -fdx -e "*.binlog"` keeps the logs
- `-getProperty`, `-getItem`, `-getTargetResult`, and `-pp` take one project or `.props` file, a solution path fails `MSB1063` or refuses the switch
- `Directory.Build.props` evaluates the root values without a project

```bash
dotnet build Solution.slnx -bl:artifacts/logs/build-{}.binlog
dotnet build Solution.slnx -check -bl:artifacts/logs/check-{}.binlog
dotnet test --project Item.Tests/Item.Tests.csproj -bl:artifacts/logs/test-{}.binlog
dotnet msbuild Item/Item.csproj -getProperty:OutputPath -getItem:Compile -getResultOutputFile:artifacts/logs/item.json
```

## [02]-[BINLOG_TOOLS]

Run in order: `binlog_overview`, `binlog_diagnose` on a failed build, `binlog_errors`, `binlog_warnings`, then the drill-down tool the finding names.

| [INDEX] | [TOOL]                                | [PURPOSE]                                                                      |
| :-----: | :------------------------------------ | :----------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Server contract version and the tools that emit the JSON envelope              |
|  [02]   | `binlog_overview`                     | Status, duration, MSBuild version, project count, error and warning counts     |
|  [03]   | `binlog_diagnose`                     | Failed targets, root causes, missing references, double writes, analyzers      |
|  [04]   | `binlog_errors`                       | Deduplicated errors, `category` filter, task output on `include_task_output`   |
|  [05]   | `binlog_warnings`                     | Deduplicated warnings, filtered by `code` or `category`                        |
|  [06]   | `binlog_projects`                     | Every project with status and duration                                         |
|  [07]   | `binlog_evaluations`                  | One entry per evaluation with id and duration, filtered by `project`           |
|  [08]   | `binlog_evaluation_properties`        | Properties of one `evaluation_id`, filtered by `property_names`                |
|  [09]   | `binlog_evaluation_global_properties` | Global properties of one `evaluation_id`                                       |
|  [10]   | `binlog_properties`                   | Key properties of a project, or the ones matching `filter`                     |
|  [11]   | `binlog_explain_property`             | Final value of one property and every source that assigned it                  |
|  [12]   | `binlog_compare_property`             | One property across every project: differs, set, inconsistent, not set         |
|  [13]   | `binlog_items`                        | Items of one `itemType` for a project, or the item types when omitted          |
|  [14]   | `binlog_imports`                      | Import chain of a project with each missing import marked                      |
|  [15]   | `binlog_preprocess`                   | Project file source alone, no import expansion                                 |
|  [16]   | `binlog_files`                        | Embedded source files, listed or read by `filePath` and line range             |
|  [17]   | `binlog_search_files`                 | Text or regex search across the embedded source files                          |
|  [18]   | `binlog_search`                       | Build event search in the StructuredLog query syntax                           |
|  [19]   | `binlog_explore_node`                 | Ancestors, details, and children of one `node_id`                              |
|  [20]   | `binlog_project_targets`              | Targets of one project with timing and skip status                             |
|  [21]   | `binlog_search_targets`               | Targets by name substring across every project, with `skipped` per instance    |
|  [22]   | `binlog_target_reasons`               | Trigger, dependency chain, and summed duration per target matching the name    |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline of one evaluation, addressed as `eval-<id>`           |
|  [24]   | `binlog_tasks_in_target`              | Tasks inside one target of a project                                           |
|  [25]   | `binlog_task_details`                 | Parameters and messages of one task by `project`, `target_name`, `task_name`   |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration                                  |
|  [27]   | `binlog_expensive_targets`            | Slowest targets, aggregated by name                                            |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks, aggregated by name                                              |
|  [29]   | `binlog_project_target_times`         | Target timing of one project                                                   |
|  [30]   | `binlog_expensive_analyzers`          | Slowest analyzers and generators from a `ReportAnalyzer=true` build            |
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
|  [44]   | `stop`                                | Stop the current instance                                                      |

- The reader drops records a newer MSBuild wrote, `binlog_warnings` reports the loss as `Skipped some data unknown to this version of Viewer`
- `binlog_extract` refuses a log with dropped records unless `allow_unsupported_records=true`, the extract omits them
- `binlog_analyzer_summary`, `binlog_incremental_analysis`, and `binlog_task_details` accept no size limit and return the whole result
- `binlog_search` marks a project, target, or task `[id]` and a message `[in id]` with its owner
- `binlog_search_targets`, `binlog_tasks_in_target`, and `binlog_projects` give ids `binlog_explore_node` accepts
- `binlog_target_reasons` sums duration, executions, and skips of one target name, `binlog_search_targets` reports `skipped` per instance
- `binlog_double_writes` reads performed copies, a `Copy` that skipped an unchanged file is no write
- `binlog_double_writes` misses a second instance of one project and a copy-local file two projects copy, `BC0102` reports both
- The solution node adds one synthetic `Build failed.` error to every failed count and matches the requested name in `binlog_search_targets`
- `binlog_files` reads the embedded copy of a file, the file as the build saw it
- `binlog_properties` can answer from the restore evaluation, `MSBuildIsRestoring=True` in its output shows it
- `binlog_evaluation_properties` on the build-pass evaluation id reads a value that differs between passes
- Stale server instances hold their binlogs in memory until `stop_instance` on each `isOrphaned` entry of `list_mcp_instances`
- `binlog_incremental_analysis` returns `{kind, schemaVersion, data}` with `data.summary` counts and one `data.targets[]` row per target
- `binlog_task_details` returns `{id, name, projectFile, targetName, durationMs, parameters, outputMessages}`
- `binlog_task_details` by names prints nothing when `target_name` is not the target holding the task
- `project` of `binlog_task_details` takes a framework suffix (`App.csproj net10.0`) to pick one inner build
- `binlog_search` with `$target` names the holding target and the `[id]` the `task_id` form takes
- `binlog_explain_property` reports a global property `Set by: evaluation` with the project as `Source`, `binlog_compare_property` marks it `global`
- `binlog_explain_property` names the evaluated project as `Source` for a value an import assigns, `binlog_search_files` names the declaring file
- `binlog_diagnose` counts an error with no file and line once across every project reporting it

### [02.1]-[SEARCH_SYNTAX]

`binlog_search` matches nodes, a match on a target or task includes its child messages up to `context` levels, task output, copy details, and up-to-date reasons sit there.

| [INDEX] | [QUERY]                       | [MATCHES]                                                |
| :-----: | :---------------------------- | :------------------------------------------------------- |
|  [01]   | `$error`, `$warning`          | Every error or warning node                              |
|  [02]   | `$task Csc`                   | Every invocation of that task                            |
|  [03]   | `$target Build`               | Every target with that name                              |
|  [04]   | `$project Item`               | Every project with the text in its name                  |
|  [05]   | `under($project Item) CS1234` | Nodes under that project containing the text             |
|  [06]   | `$task $time`                 | Tasks with timing, slowest first                         |
|  [07]   | `"exact phrase"`              | Message text, a phrase with an inner `"` matches nothing |
|  [08]   | `name=value`                  | Field match, a property assignment                       |

- A text match prints the matched line alone, the `$target` form reaches the reason line under it

### [02.2]-[LARGE_LOGS]

Start with `binlog_overview`, `binlog_errors`, `binlog_warnings`, and `binlog_projects` on the original log. Their streaming index answers whole-build queries without loading the structured tree. Read each response's scope notice, a query can use a substituted subtree when the index cannot answer it. An extract supplied as input limits the scope with no notice.

Above 200 MB the server answers tree queries from an automatically extracted subtree, error-seeded projects for diagnostic tools and the heaviest projects for performance tools, and caches the extract beside the index for seven days. `BINLOG_MCP_AUTO_EXTRACT_MB` sets the threshold, `0` forces a full load. `binlog_double_writes`, `binlog_diagnose`, `binlog_compare`, and `binlog_compare_property` answer about the whole build, they load the full log when process memory allows and refuse with the reason otherwise.

For a targeted investigation:
1. Select projects from the original log's diagnostics
2. Run `binlog_extract_preview` with the error, warning, project, or project-context selection
3. Run `binlog_extract` with the same selection, an `output_file`, and the returned `plan_token`
4. Add `include_descendants=true` for referenced projects and `include_ancestors=true` for callers
5. Read `skippedUnsupportedRecords` and state the selected scope with the result

The preview token is valid for ten minutes in the same server process. A defect in an extract is evidence for its selected projects, an empty result excludes no defect elsewhere. Keep every participant when investigating a cross-project relationship.

The original log serves `binlog_files`, `binlog_search_files`, `binlog_preprocess`, and `binlog_assets`. An extract omits the embedded source archive. When the original query cannot run, report its refusal and the available evidence.

## [03]-[FAILED_BUILD_TRIAGE]

Start at `binlog_diagnose`, route the error class by the table, fix the first error before the next capture. A failed restore stops every project, a failed reference blocks its dependents.

| [INDEX] | [SYMPTOM]                               | [FIRST_TOOL]                                 | [NEXT_STEP]                                   |
| :-----: | :-------------------------------------- | :------------------------------------------- | :-------------------------------------------- |
|  [01]   | `CS*` or `FS*` compiler error           | `binlog_errors`, then `binlog_compiler`      | `get_diagnostics`                             |
|  [02]   | `CA*`, `IDE*`, `RS*` analyzer error     | `binlog_errors`                              | `get_diagnostics`, `includeAnalyzers=true`    |
|  [03]   | `MSB3073`, the reason is in task output | `binlog_errors`, `include_task_output=true`  | Fix the tool input the task output names      |
|  [04]   | `MSB4019` import not found              | `binlog_imports`                             | The import path                               |
|  [05]   | `MSB4057` target does not exist         | `binlog_project_targets` on that project     | The target name                               |
|  [06]   | `MSB4092` or `MSB4113` condition        | `binlog_errors`, the file and line           | The condition form                            |
|  [07]   | `MSB4252` under `-isolate`              | Error message, both global-property sets     | Declare the edge or remove the extra property |
|  [08]   | `MSB3026` copy retry or a file lock     | `binlog_double_writes`                       | Shared output paths                           |
|  [09]   | `NU1*` restore                          | `binlog_nuget`, then `binlog_assets`         | The version graph                             |
|  [10]   | `NETSDK1004` assets file missing        | Command line                                 | Remove `--no-restore` or restore first        |
|  [11]   | `NETSDK1005` no target for framework    | `binlog_evaluations`, then global properties | Two evaluations of one project                |
|  [12]   | Other `NETSDK*`                         | `binlog_explain_property`, named property    | The assignment                                |
|  [13]   | `MSB3277` assembly version conflict     | `binlog_assembly_conflicts`                  | `binlog_assets` with `package`, both chains   |
|  [14]   | One target ran twice                    | `binlog_search_targets` on the target        | Shared output paths, two evaluations          |
|  [15]   | One file written by two projects        | `dotnet build -check`, `BC0102`              | Shared output paths                           |
|  [16]   | One property has the wrong value        | `binlog_explain_property`                    | The assignment order                          |
|  [17]   | One target never ran, build succeeded   | `binlog_search` for the `BeforeTargets` text | Fix the target name                           |
|  [18]   | Failed status with no error record      | `binlog_overview` failing project            | `binlog_project_targets`, the failed target   |
|  [19]   | Native asset missing at run time        | `binlog_assets` with `package`               | `binlog_items` on `NativeCopyLocalItems`      |
|  [20]   | Slow build                              | `binlog_expensive_projects`                  | `references/execution-performance.md`         |
|  [21]   | One analyzer dominates the build        | `binlog_analyzer_summary`                    | Capture with `-p:ReportAnalyzer=true` first   |

- `MSB3073` reports the exit code alone, the tool's reason is a plain message under the task that no error list holds
- A tool line in canonical `error:` form is an error record
- A missing `BeforeTargets` name logs `does not exist in the project, and will be ignored` with the file and line, neither a warning nor an error
- `binlog_overview` status decides whether a build failed, a target can fail without an error record and leave `binlog_errors` empty
- `binlog_diagnose` counts distinct root causes by code, file, and line, one error repeating per target framework is one cause
- `binlog_compare_property` compares a wrong value across every project and names the projects the solution passed no `Configuration` to
- A native asset a package holds under `build/` alone reaches direct consumers through the package `.targets`, a transitive consumer receives none

## [04]-[BUILDCHECK]

`dotnet build -check` runs every inbox check and reports each finding as a build diagnostic with a `BC` code. The checks belong to MSBuild, `dotnet build`, `dotnet msbuild`, and a replay run one set.

| [INDEX] | [CODE]   | [REPORTS]                                                            | [DEFAULT]           |
| :-----: | :------- | :------------------------------------------------------------------- | :------------------ |
|  [01]   | `BC0101` | Two projects with one `OutputPath` or `IntermediateOutputPath`       | Warning             |
|  [02]   | `BC0102` | Two tasks writing one file, across projects or instances             | Warning             |
|  [03]   | `BC0103` | Property values read from an environment variable                    | Suggestion, project |
|  [04]   | `BC0104` | `Reference` to a project output in place of `ProjectReference`       | Warning             |
|  [05]   | `BC0105` | `EmbeddedResource` without `Culture` or `WithCulture=false` metadata | Warning             |
|  [06]   | `BC0106` | `CopyToOutputDirectory="Always"` on an item                          | Warning             |
|  [07]   | `BC0107` | `TargetFramework` and `TargetFrameworks` both set                    | Warning             |
|  [08]   | `BC0108` | `TargetFramework` or `TargetFrameworks` in a project without the SDK | Warning             |
|  [09]   | `BC0201` | Property reads no declaration precedes                               | Warning, project    |
|  [10]   | `BC0202` | Property reads before the declaration that follows them              | Warning, project    |
|  [11]   | `BC0203` | Properties declared in the project and never read                    | None, project       |
|  [12]   | `BC0301` | Project under the Downloads folder or another untrusted directory    | Error, project      |
|  [13]   | `BC0302` | `Exec` that runs `dotnet`, `msbuild`, or `nuget` to build a project  | Warning             |

- Suggestions print as `message` lines on the console logger at `-v:m` and higher
- `BC0201` and `BC0202` accept a self-reference and an emptiness check, and report a read inside a `Condition`
- `AllowUninitializedPropertiesInConditions=true` on both codes accepts the condition reads, `false` is the default
- The project scope covers the project file, `scope=all` extends `BC0201`, `BC0202`, and `BC0203` to every import
- `-check` on a replay, `dotnet build <log>.binlog -check`, reruns the checks over the stored events and writes no file
- A replay prints the original `BinaryLogger wrote to:` line and doubles every count
- `binlog_warnings` with `category=BuildCheck` lists the reports of a `-check` capture with the console counts
- Under `MSBuildTreatWarningsAsErrors` the reports print as `error BC`, fail the build, and land in `binlog_errors`
- `-check` reports on an incremental build, the checks read declared paths and task inputs

`.editorconfig` configures each code under a section header, MSBuild ignores a key outside a section:

```ini
[*.csproj]
build_check.BC0101.severity = error
build_check.BC0106.severity = none
build_check.BC0201.scope = all
build_check.BC0201.AllowUninitializedPropertiesInConditions = true
build_check.BC0202.AllowUninitializedPropertiesInConditions = true
```

- `severity` takes `default`, `none`, `suggestion`, `warning`, or `error`, `scope` takes `project_file`, `work_tree_imports`, or `all`
- A code the build accepts gets `severity = none`

### [04.1]-[WORKFLOW]

1. Run `dotnet build <solution> -t:Rebuild -check -bl:<dir>/check-{}.binlog`
2. Read each `BC` line on the console, or run `binlog_errors` then `binlog_warnings` with `category=BuildCheck`
3. Fix the file the report names, `BC0201` and `BC0202` name `file(line,col)`, `BC0101` and `BC0102` name the path and both projects
4. Run the same command again, the code is gone from the console and the log

## [05]-[SHARED_OUTPUT_PATHS]

MSBuild creates one project instance per project path and global-property set. Two instances with one `OutputPath` or `IntermediateOutputPath`, or two projects with one directory, fail by build order: one project consumes the other's `project.assets.json`, `MSB3026` copy retries and file locks appear in parallel builds, outputs come from the wrong instance. A successful build's console reports none of them without `-check`, the steps detect them:
1. Run the BuildCheck workflow, read `BC0101` per shared directory and `BC0102` per file two tasks wrote
2. Run `binlog_compare_property` on `IntermediateOutputPath`, then `OutputPath`, an absolute value grouping two projects is the shared directory
3. Run `binlog_double_writes` for directories more than one project copied into, it covers projects `binlog_compare_property` reports as `NOT SET`
4. Run `binlog_search_targets` on `CoreCompile`, two `skipped: false` rows for one project file are two instances
5. Run `binlog_evaluations` with the `project` filter, then `binlog_evaluation_global_properties` per evaluation
6. Compare the build-pass evaluations by the table, the global property differing between them names the extra instance
7. Run `dotnet restore`, then `dotnet build --no-restore -graph -isolate`, an instance the graph did not declare fails `MSB4252`

- `BC0101` naming `Library.csproj and Library.csproj` reports a second instance of one project
- The relative SDK default groups every project in `binlog_compare_property` and means nothing, a second instance of one project shows no group

| [INDEX] | [GLOBAL_PROPERTY]                      | [IN_THE_PATH] | [MEANING]                                                                |
| :-----: | :------------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Configuration`                        | Always        | One path per configuration                                               |
|  [02]   | `TargetFramework`                      | Conditional   | Appended while `AppendTargetFrameworkToOutputPath` is `true`             |
|  [03]   | `RuntimeIdentifier`                    | Conditional   | Appended while `AppendRuntimeIdentifierToOutputPath` is `true`           |
|  [04]   | `Platform`                             | Conditional   | A non-default platform adds a segment, none under the artifacts layout   |
|  [05]   | `SolutionFileName`, `SolutionPath`     | No            | Different values mark one project built from two solutions               |
|  [06]   | `CurrentSolutionConfigurationContents` | No            | Project entries of the solution, the entry count tells two apart         |
|  [07]   | `MSBuildIsRestoring`                   | No            | Restore pass, expected and discarded                                     |
|  [08]   | `BuildProjectReferences`               | No            | Reference queries where `Get*` targets alone ran, or `--no-dependencies` |
|  [09]   | `_IsPublishing`                        | No            | Set by `dotnet publish`, an `<MSBuild>` call passing it builds twice     |
|  [10]   | `PublishReadyToRun`                    | No            | Publish setting adding an instance without a path change                 |

| [INDEX] | [SIGNAL]                                    | [CAUSE]                                   | [FIX]                                        |
| :-----: | :------------------------------------------ | :---------------------------------------- | :------------------------------------------- |
|  [01]   | `BC0102` names `Csc` twice from one project | `AppendTargetFrameworkToOutputPath=false` | Remove the append property                   |
|  [02]   | Two projects at one absolute `obj` or `bin` | One `Base*OutputPath` for every project   | One directory per project                    |
|  [03]   | `SolutionFileName` is the one difference    | One project in two solutions of one build | One solution per build, or a filter          |
|  [04]   | `_IsPublishing` is the one difference       | `<MSBuild>` passes `_IsPublishing=true`   | `DependsOnTargets="Publish"` in one instance |
|  [05]   | `TargetFramework` differs, single-targeting | `SetTargetFramework` on the reference     | Remove `SetTargetFramework`                  |
|  [06]   | Properties outside the path differ          | Extra `Properties` on an `<MSBuild>` call | `GlobalPropertiesToRemove` on the edge       |

- `binlog_search` with `$task MSBuild` lists each call under the project making it
- `GlobalPropertiesToRemove` on a `ProjectReference` strips a property from the referenced build, none a project passes to itself

```xml
<!-- BAD -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/</OutputPath>

<!-- GOOD -->
<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>
```

- `AppendTargetFrameworkToOutputPath=false` drops the framework from `IntermediateOutputPath` and `OutputPath` both
- An `OutputPath` containing `$(TargetFramework)` still shares `obj/`, and reading `$(TargetFramework)` in the project body adds `BC0202`

### [05.1]-[SHARED_DIRECTORY_ACROSS_PROJECTS]

Restore writes `project.assets.json` under `MSBuildProjectExtensionsPath`, which defaults to `BaseIntermediateOutputPath` with no framework segment, two projects with one `obj` share one assets file the last restore overwrites. One shared `bin` lets the second project's copy of a common dependency skip as unchanged, the build succeeds with `BC0102` as the one report.

```xml
<!-- BAD: Directory.Build.props -->
<BaseOutputPath>../SharedOutput/</BaseOutputPath>
<BaseIntermediateOutputPath>../SharedObj/</BaseIntermediateOutputPath>

<!-- GOOD: Directory.Build.props -->
<BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
<BaseOutputPath>$(MSBuildThisFileDirectory)bin/$(MSBuildProjectName)/</BaseOutputPath>
```

The GOOD form is one output directory per project, or `ArtifactsPath` for the whole tree. Use `dotnet-msbuild-packaging` for the artifacts layout.

## [06]-[BUILD_PERFORMANCE]

Compare two captures under the capture conditions of `references/execution-performance.md`:
1. Run `binlog_overview` on each capture, record status, duration, and project count
2. Run `binlog_expensive_projects`, `binlog_expensive_targets`, and `binlog_expensive_tasks` on the slow capture
3. For a slow project chain, target, or task, follow `references/execution-performance.md`
4. For slow evaluation or a target running in a no-change build, follow `references/evaluation-and-incrementality.md`
5. When both contribute, complete both and capture again after each change
