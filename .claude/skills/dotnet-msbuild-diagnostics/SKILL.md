---
name: dotnet-msbuild-diagnostics
description: "enter a description here"
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

Covers binary log capture, build failure triage, build performance, and output path clashes through the `binlog` MCP server, `Microsoft.AITools.BinlogMcp`, over a `.binlog` file.
- `.binlog` is binary. Never read it with `cat` or `strings`. Query it through the MCP tools — a text-log replay erases the tree they read.
- Once the evidence supports a conclusion, stop the investigation and present it.

[REFERENCES]:
- [01]-[PERFORMANCE_BASELINE](references/performance-baseline.md): Comparable captures and the first evidence route
- [02]-[EXECUTION_PERFORMANCE](references/execution-performance.md): Executed work, graph constraints, and task cost
- [03]-[EVALUATION_AND_INCREMENTALITY](references/evaluation-and-incrementality.md): Evaluation cost and unexpected repeated work

## [01]-[BINARY_LOG_CAPTURE]

Pass `-bl:{}` on every MSBuild invocation: `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, `dotnet restore`, and `msbuild`. MSBuild expands `{}` to a UTC date, time, process id, and random stamp, and appends `.binlog`. Each invocation writes one binlog to the current directory and prints its absolute path at the end of the build output. A failed build never needs a re-run to get a log.
- A bare `-bl` overwrites `msbuild.binlog`. A literal name without `.binlog` stops the build with `MSB1029` — only `{}` appends the extension.
- `-f` or `-p:TargetFramework=` spawns a separate restore process, and `-bl` forwards to it. A fixed name loses the restore log to the build's overwrite. `{}` keeps both files.
- A prefix or directory routes the file: `-bl:logs/build-{}`.
- Keep the default `ProjectImports=Embed`. `None` and `ZipFile` strip what `binlog_files`, `binlog_search_files`, and `binlog_preprocess` read.
- PowerShell consumes `{}`. Quote the switch there: `'-bl:{}'`. Bash and zsh keep it literal.
- Make sure that the file exists before analysis. A build that stops before MSBuild starts, such as one with a malformed `-bl` value, writes no binlog.
- `git clean -fdx` deletes ignored files, which includes `*.binlog`. Exclude them to keep the build history.

Under the Microsoft.Testing.Platform runner, `dotnet test -bl:{}` works unchanged. No binlog contains the test execution, because MTP runs the test modules outside MSBuild.
- A passing `dotnet test` writes two binlogs: the build log, and an evaluation-only discovery log that `binlog_overview` labels `FAILED` with an unknown MSBuild version. Analyze the log that reports `SUCCEEDED` with targets. A fixed name labels the pair: `-bl:test.binlog` writes `test.binlog` and `test-dotnet-test.binlog`.
- A failing build writes only the real build log.
- Tokens after `--` go to the test application. An unknown token there exits with code 5 after the build already ran and logged.

```bash
dotnet build -bl:{}                       # writes <utc-date>-<time>--<pid>--<random>.binlog
dotnet build -c Release -bl:release-{}    # writes release-<stamp>.binlog
dotnet test -bl:test.binlog               # writes test.binlog + test-dotnet-test.binlog (discovery, ignore)
git clean -fdx -e "*.binlog"              # keep the logs
```

## [02]-[BINLOG_MCP_TOOLS]

| [INDEX] | [TOOL]                                | [PURPOSE]                                                                                                      |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Server contract version and the tools that emit the JSON envelope                                              |
|  [02]   | `binlog_overview`                     | Build status, duration, project count, error and warning counts                                                |
|  [03]   | `binlog_diagnose`                     | Root causes of a failed build, grouped by code, file, and line, with fix hints                                 |
|  [04]   | `binlog_errors`                       | Deduplicated errors by location and project, `include_task_output=true` inlines each failing task's own output |
|  [05]   | `binlog_warnings`                     | Deduplicated warnings, filtered by `code` or `category`                                                        |
|  [06]   | `binlog_projects`                     | Every project with status and duration                                                                         |
|  [07]   | `binlog_evaluations`                  | One entry per project and TFM evaluation, with its evaluation id                                               |
|  [08]   | `binlog_evaluation_properties`        | Properties of one evaluation id, filtered by `property_names`                                                  |
|  [09]   | `binlog_evaluation_global_properties` | Global properties of one evaluation id                                                                         |
|  [10]   | `binlog_properties`                   | Key properties of a project, or the properties that match `filter`                                             |
|  [11]   | `binlog_explain_property`             | Final value of a property and the unique assignment sources the log recorded                                   |
|  [12]   | `binlog_compare_property`             | One property across every project: differs, set, inconsistent, not set                                         |
|  [13]   | `binlog_items`                        | Items of one type for a project, or the item types when `itemType` is omitted                                  |
|  [14]   | `binlog_imports`                      | Import chain of a project, with missing imports                                                                |
|  [15]   | `binlog_preprocess`                   | Project source with every import expanded in place, the `-pp` view                                             |
|  [16]   | `binlog_files`                        | Embedded source files, listed or read by path and line range                                                   |
|  [17]   | `binlog_search_files`                 | Text or regex search across the embedded source files                                                          |
|  [18]   | `binlog_search`                       | Build event search in StructuredLog syntax: `$error`, `$task Csc`, `under($project App) CS1234`                |
|  [19]   | `binlog_explore_node`                 | Ancestors, details, and children of one node id                                                                |
|  [20]   | `binlog_project_targets`              | Targets that ran in a project, with timing and skip status                                                     |
|  [21]   | `binlog_search_targets`               | Targets by name substring across every project                                                                 |
|  [22]   | `binlog_target_reasons`               | Why a target ran or was skipped: trigger, dependency chain, durations, and skip count                          |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline of one evaluation id                                                                  |
|  [24]   | `binlog_tasks_in_target`              | Tasks inside one target of a project                                                                           |
|  [25]   | `binlog_task_details`                 | Parameters and messages of one task, addressed by project, target, and task name                               |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration                                                                  |
|  [27]   | `binlog_expensive_targets`            | Slowest targets, aggregated by name                                                                            |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks, aggregated by name                                                                              |
|  [29]   | `binlog_project_target_times`         | Target timing of one project                                                                                   |
|  [30]   | `binlog_expensive_analyzers`          | Slowest Roslyn analyzers and source generators, from a `ReportAnalyzer=true` build                             |
|  [31]   | `binlog_analyzer_summary`             | Total time per analyzer, from a `ReportAnalyzer=true` build                                                    |
|  [32]   | `binlog_build_graph`                  | Project dependency graph with durations and the critical path                                                  |
|  [33]   | `binlog_incremental_analysis`         | Incrementality decisions for targets that declare `Outputs`, and outputs that `IncrementalClean` deleted       |
|  [34]   | `binlog_double_writes`                | Destinations that more than one performed copy wrote, with the shared output directories                       |
|  [35]   | `binlog_assembly_conflicts`           | `MSB3277` warnings with the `ResolveAssemblyReference` inputs behind them                                      |
|  [36]   | `binlog_compiler`                     | `Csc`, `Vbc`, and `Fsc` command lines with response files                                                      |
|  [37]   | `binlog_nuget`                        | Restored packages, versions, sources, and restore duration                                                     |
|  [38]   | `binlog_assets`                       | `project.assets.json` contents: frameworks, libraries, transitive pins, reverse dependencies of a `package`    |
|  [39]   | `binlog_compare`                      | Property and package diff between two binlogs                                                                  |
|  [40]   | `binlog_extract_preview`              | Size and project count of a subtree extraction, without a write                                                |
|  [41]   | `binlog_extract`                      | Standalone `.binlog` of the selected projects and their scaffolding                                            |
|  [42]   | `list_mcp_instances`                  | Running server instances with memory and `isOrphaned`, the ones to stop first                                  |
|  [43]   | `stop_instance`                       | Stop one instance by PID                                                                                       |
|  [44]   | `stop`                                | Stop this instance                                                                                             |

## [03]-[FAILED_BUILD_TRIAGE]

1. Run `binlog_overview` for status and counts.
2. Run `binlog_diagnose`. Its grouped root causes with fix hints are usually the whole diagnosis. Read errors yourself only when it finds nothing.
3. Run `binlog_errors` for file and line detail. If the error text does not explain the failure, run it again with `include_task_output=true`. A tool's explaining line is a plain message under the task and reaches no error list unless the tool printed it in canonical `error:` form.
4. Route the remaining error class:

| [INDEX] | [ERROR_CLASS]                   | [ROUTE]                                                                                           |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `CS*`, `FS*`, `BC*` compiler    | The error's file and line, `binlog_compiler` when the command line or analyzer set is in question |
|  [02]   | `NU*` restore                   | `binlog_nuget`, then `binlog_assets` for resolved versions and reverse dependencies               |
|  [03]   | `MSB4019` import not found      | `binlog_imports`, it reports the missing import                                                   |
|  [04]   | `MSB4057` target does not exist | `binlog_search_targets`                                                                           |
|  [05]   | `MSB3277` version conflicts     | `binlog_assembly_conflicts`                                                                       |
|  [06]   | `NETSDK*`                       | `binlog_explain_property` on the property the message names                                       |

- Fix the first error, then re-capture. `NETSDK1004` follows the error that stopped restore, and a failed reference blocks its dependent builds.
- Counts overstate: the solution node adds a synthetic `Build failed.`, and `×N` on a deduplicated line counts emissions across frameworks, not distinct problems.
- If the build succeeded but a target never ran, run `binlog_search` for `listed in a BeforeTargets attribute`. The typo message is neither a warning nor an error.
- On a very large binlog, `binlog_overview`, `binlog_errors`, `binlog_warnings`, and `binlog_projects` normally use a whole-build streaming index. A subtree notice limits the result. Act on the notice, not the tool name.
- Stale server instances hold their binlogs in memory. `list_mcp_instances` names them and `stop_instance` frees them.

## [04]-[BUILD_PERFORMANCE]

1. Read and follow `references/performance-baseline.md`.
2. Follow the reference selected by the baseline evidence:
  - For executed work, graph constraints, or task cost, read and follow `references/execution-performance.md`.
  - For evaluation cost or unexpected repeated work, read and follow `references/evaluation-and-incrementality.md`.

## [05]-[OUTPUT_PATH_CLASHES]

A clash builds green more often than it fails. Two project instances that share one `OutputPath` or `IntermediateOutputPath` produce:
- A green build where one project consumed the other's restore
- `MSB3026` copy retries and `The process cannot access the file because it is being used by another process` in a parallel build
- `Cannot create a file when that file already exists` from restore
- Overwritten or missing output files, and failures that pass on retry

A clash comes from one of these sources:
- Two projects set one shared output directory
- A multi-targeting or multi-RID build writes to a path that omits the pivot
- One build invokes two solutions that both contain the project
- A path-neutral global property forks a second instance of one project

If the build ran with `-check`, `binlog_warnings` answers directly: `BC0101` flags a shared `OutputPath` or `IntermediateOutputPath` between projects, and `BC0102` flags double writes. The switch also runs over an existing log: `dotnet build msbuild.binlog -check`. The checks are opt-in, so their absence proves nothing.

1. Run `binlog_compare_property` on `IntermediateOutputPath`, then `OutputPath`. A value that groups two or more projects is the clash.
2. Run `binlog_double_writes` for the output directories the build wrote. It covers the `NOT SET` projects, which step 1 reports without clearing.
3. If one project is inconsistent within itself, run `binlog_evaluations` with a project filter, then `binlog_evaluation_global_properties` per evaluation. Discard the restore pass, marked `MSBuildIsRestoring=true`. The global property that differs between the remaining evaluations names the fork.

A green clash shows nothing in the console and nothing in `binlog_diagnose`. The walk above is the detection.

### [05.1]-[GLOBAL_PROPERTIES]

| [INDEX] | [PROPERTY]                             | [SDK_DEFAULT_SEPARATION] | [MEANING]                                                                   |
| :-----: | :------------------------------------- | :----------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `TargetFramework`                      | Conditional              | Appended while `AppendTargetFrameworkToOutputPath` is `true`                |
|  [02]   | `RuntimeIdentifier`                    | Conditional              | Appended while `AppendRuntimeIdentifierToOutputPath` is `true`              |
|  [03]   | `Configuration`                        | Yes                      | One path per configuration                                                  |
|  [04]   | `Platform`                             | Non-`AnyCPU`             | Non-default platforms add a path segment                                    |
|  [05]   | `SolutionFileName`                     | No                       | Names the building solution, different values mark a multi-solution clash   |
|  [06]   | `SolutionName`, `BuildingSolutionFile` | No                       | Same signal as `SolutionFileName`                                           |
|  [07]   | `CurrentSolutionConfigurationContents` | No                       | Project entries of the solution, the entry count tells two solutions apart  |
|  [08]   | `BuildProjectReferences`               | No                       | Reference query if only `Get*` targets ran, also set by `--no-dependencies` |
|  [09]   | `MSBuildIsRestoring`                   | No                       | Restore-pass marker, with `MSBuildRestoreSessionId`                         |
|  [10]   | `PublishReadyToRun`                    | No                       | Publish setting, forks an instance without a path change                    |
|  [11]   | `_IsPublishing`                        | No                       | Set by `dotnet publish`, an `<MSBuild>` call that passes it forks the build |

### [05.2]-[PIVOT_MISSING_FROM_OUTPUT_PATH]

- PROBLEM: A multi-targeting or multi-RID project writes every framework or runtime to one path.
- DETECT: Evaluations of one project differ in `TargetFramework` or `RuntimeIdentifier` and share `OutputPath`.
- FIX: Keep the SDK default paths. The SDK appends the framework and runtime to explicit or default paths while the matching append properties remain `true`. If an append is off, the path must carry that pivot.

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
- DETECT: `binlog_compare_property` names both projects at one `IntermediateOutputPath`. On disk, the shared `project.assets.json` carries one `projectName`, the other project's `ProjectAssetsFile` points at it, and its own `.nuget.g.props` never exists. Restore can also stop with `Cannot create a file when that file already exists`.
- FIX: Give each project its own intermediate directory. The SDK default `obj/` in the project directory does this, and so does `ArtifactsPath` in `Directory.Build.props`. See `dotnet-msbuild-evaluation` skill for the artifacts layout.

```xml
<!-- BAD: Directory.Build.props sends every project to one obj/ -->
<BaseOutputPath>../SharedOutput/</BaseOutputPath>
<BaseIntermediateOutputPath>../SharedObj/</BaseIntermediateOutputPath>

<!-- GOOD: one intermediate directory per project -->
<BaseIntermediateOutputPath>../obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
```

### [05.4]-[ONE_PROJECT_IN_TWO_SOLUTIONS]

- PROBLEM: One build invokes two solutions, through the `<MSBuild>` task or the command line, and both contain the project. Each solution builds it with its own `Solution*` global properties, which never change the output path. The first build compiles. The second skips `CoreCompile` and runs `CopyFilesToOutputDirectory` against the same directory. Concurrent instances race on the copies and the `.copycomplete` marker.
- DETECT: `SolutionFileName` and `CurrentSolutionConfigurationContents` differ across evaluations of one project.

| [INDEX] | [PROPERTY]                             | [EVALUATION_A]                | [EVALUATION_B]                |
| :-----: | :------------------------------------- | :---------------------------- | :---------------------------- |
|  [01]   | `SolutionFileName`                     | `BuildAnalyzers.sln`          | `Main.slnx`                   |
|  [02]   | `CurrentSolutionConfigurationContents` | 1 project entry               | many project entries          |
|  [03]   | `OutputPath`                           | `bin\Release\netstandard2.0\` | `bin\Release\netstandard2.0\` |

- FIX: Apply one of:
1. Build each project from one solution per build.
2. Build the second solution under a different `Configuration`, so the paths differ.
3. Exclude the project from the second solution with a solution filter.

### [05.5]-[PATH_NEUTRAL_GLOBAL_PROPERTY]

- PROBLEM: A project builds twice inside one solution because an extra global property, such as `PublishReadyToRun=false`, creates a second instance. The property never changes the output path, so MSBuild cannot share the result between the instances, and every target runs twice. A `netstandard2.0` library never uses ReadyToRun and still builds twice.
- DETECT: Build-pass evaluations of one project with the same `SolutionFileName` differ in a global property that is not in the path.

| [INDEX] | [PROPERTY]          | [EVALUATION_A]                | [EVALUATION_B]                |
| :-----: | :------------------ | :---------------------------- | :---------------------------- |
|  [01]   | `PublishReadyToRun` | not set                       | `false`                       |
|  [02]   | `OutputPath`        | `bin\Release\netstandard2.0\` | `bin\Release\netstandard2.0\` |

- FIX: Apply one of:
1. Find the parent target or task that passes the property. Remove it from the call for projects that never read it.
2. Set `GlobalPropertiesToRemove="PublishReadyToRun"` on the `ProjectReference`, so the referenced build drops the property.
3. Set the property only on the projects that consume it, such as executables.

### [05.6]-[MSBUILD_TASK_PUBLISH_FORK]

- PROBLEM: A target calls the `<MSBuild>` task on a project with a path-neutral property, most often `_IsPublishing=true` in a publish-on-build target. The call lives in the project itself (a), or in a consumer that publishes a tool (b). The second instance shares `OutputPath` and `IntermediateOutputPath` with the instance the solution builds and re-runs the same write targets against them. Parallel scheduling races the writes. A serialized run doubles the work.
- DETECT: Two build-pass evaluations of the project share the paths and differ only by `_IsPublishing`. That pair is the diagnosis. The second `CopyFilesToOutputDirectory` run logs `Did not copy` under `SkipUnchangedFiles`, so `binlog_double_writes` stays clean. `binlog_search` with `$task MSBuild` renders each call under its project, which tells (a) from (b).
- FIX: See `dotnet-msbuild-antipatterns` skill, `AP-20`. For (a), set `_IsPublishing` as a normal property and run `Publish` through `DependsOnTargets` in the same instance. For (b), apply that fix in the tool and sequence it through a `ProjectReference` with `ReferenceOutputAssembly="false"`. `GlobalPropertiesToRemove` cannot strip a property that a project injects on itself.

```xml
<!-- BAD (a): the project publishes itself; the guard only stops the recursion -->
<Target Name="PublishOnBuild" AfterTargets="Build" Condition="'$(_IsPublishing)' != 'true'">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- BAD (b): a consumer publishes the tool it references -->
<MSBuild Projects="../tool/tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
```

### [05.7]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_REFERENCE]

- PROBLEM: A `ProjectReference` sets `SetTargetFramework="TargetFramework=<tfm>"` on a single-targeting project, and the value equals the framework the project declares. The project already resolves to `bin/<config>/<tfm>/`, so the global property forks `(project, {TargetFramework=<tfm>})` beside `(project, {})` on the same paths. The project-reference protocol removes `TargetFramework` for a single-targeting reference. The metadata reintroduces it.
- DETECT: Two build-pass evaluations of the referenced project share the paths and differ only by the `TargetFramework` global property.
- FIX: Remove the metadata. `SetTargetFramework` is correct on a multi-targeting reference, and on a single-targeting reference built under a different framework, because both change the path. See `dotnet-msbuild-antipatterns` skill, `AP-21`, for framework-incompatible references and the `SkipGetTargetFrameworkProperties` guard.

```xml
<!-- BAD: Tool.csproj declares net10.0 and the reference injects net10.0 -->
<ProjectReference Include="../Tool/Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />

<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" />
```
