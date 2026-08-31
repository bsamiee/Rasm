---
name: dotnet-msbuild-diagnostics
description: "Use when a build fails, runs slow, or overwrites its own outputs, or when any .binlog needs capture or analysis using the binlog MCP."
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

Covers binary log capture, build failure triage, build performance, and output path clashes through the `binlog` MCP server, `Microsoft.AITools.BinlogMcp`, over a `.binlog` file.
- `.binlog` is binary. Never read it with `cat` or `strings`. Query it through the MCP tools — a text-log replay erases the tree they read.

[REFERENCES]:
- [01]-[PERFORMANCE_BASELINE](references/performance-baseline.md): Comparable captures and the first evidence route
- [02]-[EXECUTION_PERFORMANCE](references/execution-performance.md): Executed work, graph constraints, and task cost
- [03]-[EVALUATION_AND_INCREMENTALITY](references/evaluation-and-incrementality.md): Evaluation cost and unexpected repeated work

## [01]-[BINARY_LOG_CAPTURE]

Pass `-bl:{}` on every MSBuild invocation: `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, `dotnet restore`, and `msbuild`. MSBuild expands `{}` to a UTC date, time, process id, and random stamp, and appends `.binlog`. Each invocation writes one binlog to the current directory. `dotnet build` prints the absolute path of the log at the end of the output. Every other command prints that line only at `-v:n` or higher, so list the directory. A failed build never needs a re-run to get a log.
- A bare `-bl` overwrites `msbuild.binlog`. A literal name without `.binlog` stops the build with `MSB1029` — only `{}` appends the extension.
- `-f` or `-p:TargetFramework=` runs restore as a separate MSBuild invocation, and `-bl` applies to both. A fixed name loses the restore log to the build's overwrite. `{}` keeps both files.
- A prefix or directory routes the file: `-bl:logs/build-{}`.
- Keep the default `ProjectImports=Embed`. `None` erases every embedded file, so `binlog_files` and `binlog_search_files` return nothing. `ZipFile` moves them to a `<name>.ProjectImports.zip` sidecar that the tools read only while it sits next to the log.
- Make sure that the file exists before analysis. A build that stops before MSBuild starts, such as one with a malformed `-bl` value, writes no binlog.
- `git clean -fdx` deletes ignored files, which includes `*.binlog`. Exclude them to keep the build history.

Under the Microsoft.Testing.Platform runner, `dotnet test -bl:{}` works unchanged. No binlog contains the test execution, because MTP runs the test modules outside MSBuild.
- A passing `dotnet test` writes two binlogs: the build log, and an evaluation-only discovery log that `binlog_overview` labels `FAILED` with an unknown MSBuild version. Analyze the log that reports `SUCCEEDED` with targets. The `dotnet test` CLI labels each of its two logs, so a fixed name keeps both: `-bl:test.binlog` writes `test.binlog` and `test-dotnet-test.binlog`.
- A failing build writes only the real build log.
- Tokens after `--` go to the test application. An unknown token there exits with code 5 after the build already ran and logged.

```bash
dotnet build -bl:{}                       # writes <utc-date>-<time>--<pid>--<random>.binlog
dotnet build -c Release -bl:release-{}    # writes release-<stamp>.binlog
dotnet test -bl:test.binlog               # writes test.binlog + test-dotnet-test.binlog (discovery, ignore)
git clean -fdx -e "*.binlog"              # keep the logs
```

## [02]-[BINLOG_MCP_TOOLS]

| [INDEX] | [TOOL]                                | [PURPOSE]                                                                                     |
| :-----: | :------------------------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Server contract version and the tools that emit the JSON envelope                             |
|  [02]   | `binlog_overview`                     | Build status, duration, project count, error and warning counts                               |
|  [03]   | `binlog_diagnose`                     | Failed targets, missing references, double writes, slow analyzers, distinct-root-cause count  |
|  [04]   | `binlog_errors`                       | Deduplicated errors. `include_task_output=true` inlines each failing task's output            |
|  [05]   | `binlog_warnings`                     | Deduplicated warnings, filtered by `code` or `category`                                       |
|  [06]   | `binlog_projects`                     | Every project with status and duration                                                        |
|  [07]   | `binlog_evaluations`                  | One entry per project and TFM evaluation, with its evaluation id                              |
|  [08]   | `binlog_evaluation_properties`        | Properties of one evaluation id, filtered by `property_names`                                 |
|  [09]   | `binlog_evaluation_global_properties` | Global properties of one evaluation id                                                        |
|  [10]   | `binlog_properties`                   | Key properties of a project, or the properties that match `filter`                            |
|  [11]   | `binlog_explain_property`             | Final value of a property and the unique assignment sources the log recorded                  |
|  [12]   | `binlog_compare_property`             | One property across every project: differs, set, inconsistent, not set                        |
|  [13]   | `binlog_items`                        | Items of one type for a project, or the item types when `itemType` is omitted                 |
|  [14]   | `binlog_imports`                      | Import chain of a project, with missing imports                                               |
|  [15]   | `binlog_preprocess`                   | Import tree of a project, or its source when embedded. Not the `-pp` expansion                |
|  [16]   | `binlog_files`                        | Embedded source files, listed or read by path and line range                                  |
|  [17]   | `binlog_search_files`                 | Text or regex search across the embedded source files                                         |
|  [18]   | `binlog_search`                       | Build event search, StructuredLog syntax: `$error`, `$task Csc`, `under($project App) CS1234` |
|  [19]   | `binlog_explore_node`                 | Ancestors, details, and children of one node id. A `binlog_search` id addresses another node  |
|  [20]   | `binlog_project_targets`              | Targets that ran in a project, with timing and skip status                                    |
|  [21]   | `binlog_search_targets`               | Targets by name substring across every project                                                |
|  [22]   | `binlog_target_reasons`               | Why a target ran or was skipped: trigger, dependency chain, durations, and skip count         |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline of one evaluation, addressed as `eval-<id>`                          |
|  [24]   | `binlog_tasks_in_target`              | Tasks inside one target of a project                                                          |
|  [25]   | `binlog_task_details`                 | Parameters and messages of one task, addressed by project, target, and task name              |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration                                                 |
|  [27]   | `binlog_expensive_targets`            | Slowest targets, aggregated by name                                                           |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks, aggregated by name                                                             |
|  [29]   | `binlog_project_target_times`         | Target timing of one project                                                                  |
|  [30]   | `binlog_expensive_analyzers`          | Empty on the current server. Use `binlog_analyzer_summary`                                    |
|  [31]   | `binlog_analyzer_summary`             | Total time and invocation count per analyzer assembly, from a `ReportAnalyzer=true` build     |
|  [32]   | `binlog_build_graph`                  | Project dependency graph with durations and the critical path                                 |
|  [33]   | `binlog_incremental_analysis`         | Incrementality decisions per target with `Outputs`, and `IncrementalClean` deletions          |
|  [34]   | `binlog_double_writes`                | Destinations that more than one performed copy wrote, with the shared output directories      |
|  [35]   | `binlog_assembly_conflicts`           | `MSB3277` warnings with the `ResolveAssemblyReference` inputs behind them                     |
|  [36]   | `binlog_compiler`                     | `Csc`, `Vbc`, and `Fsc` command lines with response files                                     |
|  [37]   | `binlog_nuget`                        | Restored packages, versions, sources, and restore duration                                    |
|  [38]   | `binlog_assets`                       | `project.assets.json`: frameworks, libraries, transitive pins, reverse deps of `package`      |
|  [39]   | `binlog_compare`                      | Property and package diff between two binlogs                                                 |
|  [40]   | `binlog_extract_preview`              | Size and project count of a subtree extraction, without a write                               |
|  [41]   | `binlog_extract`                      | Standalone `.binlog` of the selected projects, without the embedded source files              |
|  [42]   | `list_mcp_instances`                  | Running server instances with memory and `isOrphaned`, the ones to stop first                 |
|  [43]   | `stop_instance`                       | Stop one instance by PID                                                                      |
|  [44]   | `stop`                                | Stop this instance                                                                            |

- The reader drops records that a newer MSBuild wrote. `binlog_warnings` reports the loss as one warning of its own, `Skipped some data unknown to this version of Viewer`, and `binlog_overview` counts it. Subtract it from the warning count. A diagnostic the console printed and no tool reports sits in a dropped record, so read the console output for it.
- `binlog_analyzer_summary`, `binlog_incremental_analysis`, and `binlog_task_details` accept no size limit. On a real solution the result lands in a file.
- An id from `binlog_search` reaches `binlog_explore_node` and addresses a different node there, with no error.

## [03]-[FAILED_BUILD_TRIAGE]

1. When the error text does not explain the failure, run `binlog_errors` with `include_task_output=true`. A tool's explaining line is a plain message under the task. It reaches no error list unless the tool printed it in canonical `error:` form.
2. Route the remaining error class:

| [INDEX] | [ERROR_CLASS]                   | [ROUTE]                                                                                           |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `CS*`, `FS*`, `BC*` compiler    | The error's file and line, `binlog_compiler` when the command line or analyzer set is in question |
|  [02]   | `NU*` restore                   | `binlog_nuget`, then `binlog_assets` for resolved versions and reverse dependencies               |
|  [03]   | `MSB4019` import not found      | `binlog_imports`, it reports the missing import                                                   |
|  [04]   | `MSB4057` target does not exist | `binlog_project_targets` on that project                                                          |
|  [05]   | `MSB3277` version conflicts     | `binlog_assembly_conflicts`                                                                       |
|  [06]   | `NETSDK*`                       | `binlog_explain_property` on the property the message names                                       |

- Fix the first error, then capture again. A failed restore stops the build before any project compiles, and a failed reference blocks its dependent builds. `NETSDK1004` appears only under `--no-restore` with no assets file.
- Counts overstate: the solution node adds a synthetic `Build failed.`.
- A solution build synthesizes the requested target name on the solution node, so `binlog_search_targets` reports a false hit for `MSB4057`.
- If the build succeeded but a target never ran, run `binlog_search` for `listed in a BeforeTargets attribute`. The typo message is neither a warning nor an error.
- Stale server instances hold their binlogs in memory. `list_mcp_instances` names them and `stop_instance` frees them.

## [04]-[BUILD_PERFORMANCE]

1. Read and follow `references/performance-baseline.md`.
2. Follow the reference selected by the baseline evidence:
  - For executed work, graph constraints, or task cost, read and follow `references/execution-performance.md`.
  - For evaluation cost or unexpected repeated work, read and follow `references/evaluation-and-incrementality.md`.
3. If both classes contribute, complete both workflows.

## [05]-[OUTPUT_PATH_CLASHES]

A clash builds green more often than it fails. Two project instances that share one `OutputPath` or `IntermediateOutputPath` produce:
- A green build where one project consumed the other's restore
- `MSB3026` copy retries and file-lock errors in a parallel build
- A restore race that reports an existing file
- Overwritten or missing output files, and failures that pass on retry

A clash comes from one of these sources:
- Two projects set one shared output directory
- A multi-targeting or multi-RID build writes to a path that omits the pivot
- One build invokes two solutions that both contain the project
- A path-neutral global property forks a second instance of one project

A green clash shows nothing in the console and nothing in `binlog_diagnose`. Walk these steps:

1. Delete the output directories. Run `dotnet build <solution> -check -m:1`. Read the console. `BC0101` names each directory that two projects share. `BC0102` names two tasks that write one file, which covers every fork of one project. `-m:1` is required, because the check keeps its state per node. The reader drops part of these warnings, so the console, not `binlog_warnings`, is the record.
2. Run `binlog_compare_property` on `IntermediateOutputPath`, then `OutputPath`. An absolute value that groups two or more projects is the clash. The relative SDK default groups every project and means nothing. The tool groups by project, so it never reports two instances of one project.
3. Run `binlog_double_writes` on a build over deleted outputs. It reads `Copy` operations only, so an incremental build reports no shared directory. It covers the projects that `binlog_compare_property` reports as `NOT SET`.
4. If one project is inconsistent within itself, run `binlog_evaluations` with a project filter, then `binlog_evaluation_global_properties` per evaluation. Discard the restore pass, marked `MSBuildIsRestoring=true`. The global property that differs between the remaining evaluations names the fork.

`-check` also runs over an existing log: `dotnet build msbuild.binlog -check`. That replay writes no file, and its final `BinaryLogger wrote to:` line is an echo from the original capture. `dotnet build --no-restore -graph -isolate` turns a path-neutral fork between projects into `MSB4252`, which names both global-property sets.

### [05.1]-[GLOBAL_PROPERTIES]

| [INDEX] | [PROPERTY]                             | [PATH_PIVOT] | [MEANING]                                                                   |
| :-----: | :------------------------------------- | :----------- | :-------------------------------------------------------------------------- |
|  [01]   | `TargetFramework`                      | Conditional  | Appended while `AppendTargetFrameworkToOutputPath` is `true`                |
|  [02]   | `RuntimeIdentifier`                    | Conditional  | Appended while `AppendRuntimeIdentifierToOutputPath` is `true`              |
|  [03]   | `Configuration`                        | Always       | One path per configuration                                                  |
|  [04]   | `Platform`                             | Conditional  | Non-default platforms add a path segment                                    |
|  [05]   | `SolutionFileName`                     | No           | Names the building solution, different values mark a multi-solution clash   |
|  [06]   | `SolutionName`, `SolutionPath`         | No           | Same signal as `SolutionFileName`, `SolutionPath` gives the full path       |
|  [07]   | `CurrentSolutionConfigurationContents` | No           | Project entries of the solution, the entry count tells two solutions apart  |
|  [08]   | `BuildProjectReferences`               | No           | Reference query if only `Get*` targets ran, also set by `--no-dependencies` |
|  [09]   | `MSBuildIsRestoring`                   | No           | Restore-pass marker, with `MSBuildRestoreSessionId`                         |
|  [10]   | `PublishReadyToRun`                    | No           | Publish setting, forks an instance without a path change                    |
|  [11]   | `_IsPublishing`                        | No           | Set by `dotnet publish`, an `<MSBuild>` call that passes it forks the build |

### [05.2]-[PIVOT_MISSING_FROM_OUTPUT_PATH]

- PROBLEM: A multi-targeting or multi-RID project writes every framework or runtime to one path. The append property also drops the framework from `IntermediateOutputPath`, so `obj/` collapses too.
- DETECT: The build is green and one framework output is missing. Run the build again with `-check`. `BC0102` names `Csc` twice from one project and the file they share.
- FIX: Keep the SDK default paths. The SDK appends the framework and runtime to explicit or default paths while the matching append properties remain `true`. If an append is off, the path must carry that pivot. Under `UseArtifactsOutput` the append properties do nothing.

```xml
<!-- BAD: the append is off and the path omits the framework -->
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/</OutputPath>

<!-- GOOD: the append is off and the path carries the framework -->
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/$(TargetFramework)/</OutputPath>
```

### [05.3]-[SHARED_DIRECTORY_ACROSS_PROJECTS]

- PROBLEM: Two projects set one `BaseOutputPath` or `BaseIntermediateOutputPath`. Restore writes `project.assets.json` to `MSBuildProjectExtensionsPath`, which defaults to `BaseIntermediateOutputPath`, with no framework suffix, so `AppendTargetFrameworkToOutputPath` does not separate them. One project restores. The other's restore is skipped against the winner's cache, and that project consumes the foreign assets file. The winner changes between runs.
- DETECT: `binlog_compare_property` names both projects at one `IntermediateOutputPath`. On disk, the shared `project.assets.json` carries one `projectName`, the other project's `ProjectAssetsFile` points at it, and its own `.nuget.g.props` never exists. Restore can also stop with a message that the file already exists.
- FIX: Give each project its own intermediate directory. The SDK default `obj/` in the project directory does this, and so does `ArtifactsPath` in `Directory.Build.props`. See `dotnet-msbuild-evaluation` skill for the artifacts layout.

```xml
<!-- BAD: Directory.Build.props sends every project to one bin/ and one obj/ -->
<BaseOutputPath>../SharedOutput/</BaseOutputPath>
<BaseIntermediateOutputPath>../SharedObj/</BaseIntermediateOutputPath>

<!-- GOOD: one intermediate directory and one output directory per project -->
<BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
<BaseOutputPath>$(MSBuildThisFileDirectory)bin/$(MSBuildProjectName)/</BaseOutputPath>
```

### [05.4]-[ONE_PROJECT_IN_TWO_SOLUTIONS]

- PROBLEM: One build invokes two solutions, through the `<MSBuild>` task or the command line, and both contain the project. Each solution builds it with its own `Solution*` global properties, which never change the output path. The first build compiles. The second skips `CoreCompile` and runs `CopyFilesToOutputDirectory` against the same directory. Concurrent instances race on the copies and on the copy marker, `$(IntermediateOutputPath)$(MSBuildProjectFile).Up2Date`.
- DETECT: `SolutionFileName` and `CurrentSolutionConfigurationContents` differ across evaluations of one project.

| [INDEX] | [PROPERTY]                             | [EVALUATION_A]                | [EVALUATION_B]                |
| :-----: | :------------------------------------- | :---------------------------- | :---------------------------- |
|  [01]   | `SolutionFileName`                     | `BuildAnalyzers.sln`          | `Main.slnx`                   |
|  [02]   | `CurrentSolutionConfigurationContents` | 1 project entry               | many project entries          |
|  [03]   | `OutputPath`                           | `bin\Release/netstandard2.0/` | `bin\Release/netstandard2.0/` |

- FIX: Apply one of:
1. Build each project from one solution per build.
2. Build the second solution under a different `Configuration`, so the paths differ.
3. Exclude the project from the second solution with a solution filter. A filter accepts a `.slnx` path. It cannot drop a project that a listed project references.

### [05.5]-[PATH_NEUTRAL_GLOBAL_PROPERTY]

- PROBLEM: A project builds twice inside one solution because an extra global property, such as `PublishReadyToRun=false`, creates a second instance. The property never changes the output path, so MSBuild cannot share the result between the instances, and every target runs twice. A `netstandard2.0` library never uses ReadyToRun and still builds twice.
- DETECT: Build-pass evaluations of one project with the same `SolutionFileName` differ in a global property that is not in the path.

| [INDEX] | [PROPERTY]          | [EVALUATION_A]                | [EVALUATION_B]                |
| :-----: | :------------------ | :---------------------------- | :---------------------------- |
|  [01]   | `PublishReadyToRun` | not set                       | `false`                       |
|  [02]   | `OutputPath`        | `bin\Release/netstandard2.0/` | `bin\Release/netstandard2.0/` |

- FIX: Apply one of:
1. Find the parent target or task that passes the property. Remove it from the call for projects that never read it.
2. Set `GlobalPropertiesToRemove="PublishReadyToRun"` on the `ProjectReference`, so the referenced build drops the property.
3. Set the property only on the projects that consume it, such as executables.

### [05.6]-[MSBUILD_TASK_PUBLISH_FORK]

- PROBLEM: A target calls the `<MSBuild>` task on a project with a path-neutral property, most often `_IsPublishing=true` in a publish-on-build target. The call lives in the project itself (a), or in a consumer that publishes a tool (b).
- DETECT: Two build-pass evaluations of the project share the paths and differ only by `_IsPublishing`. That pair is the diagnosis. The second `CopyFilesToOutputDirectory` run logs `Did not copy` under `SkipUnchangedFiles`, so `binlog_double_writes` stays clean. `binlog_search` with `$task MSBuild` renders each call under its project, which tells (a) from (b).
- FIX: For (a), run `Publish` through `DependsOnTargets` in the same instance. Skip that target when `_IsPublishing` is already `true`, because `dotnet publish` sets it as a global property. For (b), apply that fix in the tool and sequence it through a `ProjectReference` with `ReferenceOutputAssembly="false"` and `UndefineProperties="_IsPublishing"`. The `dotnet-msbuild-antipatterns` skill shows both corrected files. `GlobalPropertiesToRemove` cannot strip a property that a project injects on itself.

### [05.7]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_REFERENCE]

- PROBLEM: A `ProjectReference` sets `SetTargetFramework="TargetFramework=<tfm>"` on a single-targeting project, and the value equals the framework the project declares.
- DETECT: Two build-pass evaluations of the referenced project share the paths and differ only by the `TargetFramework` global property.
- FIX: Remove the metadata. `SetTargetFramework` is correct on a multi-targeting reference, and on a single-targeting reference built under a different framework, because both change the path. The `dotnet-msbuild-antipatterns` skill covers framework-incompatible references and the `SkipGetTargetFrameworkProperties` guard.
