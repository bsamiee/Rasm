---
name: dotnet-msbuild-diagnostics
description: "enter a description here"
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

Covers build failure diagnosis, build performance, and output path clashes through the `binlog` MCP server, `Microsoft.AITools.BinlogMcp`, over a `.binlog` file.
- A `.binlog` is a binary format. Never read it with `cat`, `head`, or `strings`, and never replay it to a text log. Query it only through the MCP tools.
- Once the evidence supports a conclusion, stop the investigation and present it.

## [01]-[BINARY_LOG_CAPTURE]

Pass `-bl:{}` on every MSBuild invocation: `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, `dotnet restore`, and `msbuild`. MSBuild expands `{}` to a date, time, process id, and random stamp, and appends `.binlog` when the name does not end in it. Every build keeps its own log. A failed build never needs a re-run to get one.
- Add `-bl:{}` to each command separately. A bare `-bl` writes `msbuild.binlog` and overwrites the previous log.
- If a CI upload needs a recognizable name, keep a prefix: `-bl:build-{}`.
- PowerShell consumes `{}`. Quote the switch there: `'-bl:{}'`. Bash and zsh keep it literal.
- Make sure that the file exists before analysis. A build that stops before MSBuild starts, such as one with a bad argument, writes no binlog.
- `git clean -fdx` deletes ignored files, which includes `*.binlog`. Exclude them to keep the build history.

```bash
dotnet build -bl:{}                       # writes <date>-<time>--<pid>--<random>.binlog
dotnet build -c Release -bl:release-{}    # writes release-<date>-<time>--<pid>--<random>.binlog
ls *.binlog                               # make sure that the file exists
git clean -fdx -e "*.binlog"              # keep the logs
```

## [02]-[BINLOG_MCP_TOOLS]

| [INDEX] | [TOOL]                                | [PURPOSE]                                                                                                         |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Server contract version and the tools that emit the JSON envelope                                                 |
|  [02]   | `binlog_overview`                     | Build status, duration, project count, error and warning counts                                                   |
|  [03]   | `binlog_diagnose`                     | Failed targets, missing references, double writes, and slow analyzers in one report                               |
|  [04]   | `binlog_errors`                       | Deduplicated errors by location and project, `include_task_output=true` adds the task output that names the cause |
|  [05]   | `binlog_warnings`                     | Deduplicated warnings, filtered by `code` or `category`                                                           |
|  [06]   | `binlog_projects`                     | Every project with status and duration                                                                            |
|  [07]   | `binlog_evaluations`                  | One entry per project and TFM evaluation, with its evaluation id                                                  |
|  [08]   | `binlog_evaluation_properties`        | Properties of one evaluation id, filtered by `property_names`                                                     |
|  [09]   | `binlog_evaluation_global_properties` | Global properties of one evaluation id                                                                            |
|  [10]   | `binlog_properties`                   | Key properties of a project, or the properties that match `filter`                                                |
|  [11]   | `binlog_explain_property`             | Final value of a property and every file, target, or task that assigned it                                        |
|  [12]   | `binlog_compare_property`             | One property across every project: differs, set, inconsistent, not set                                            |
|  [13]   | `binlog_items`                        | Items of one type for a project, or the item types when `itemType` is omitted                                     |
|  [14]   | `binlog_imports`                      | Import chain of a project, with missing imports                                                                   |
|  [15]   | `binlog_preprocess`                   | Project source with every import expanded in place, the `-pp` view                                                |
|  [16]   | `binlog_files`                        | Embedded source files, listed or read by path and line range                                                      |
|  [17]   | `binlog_search_files`                 | Text or regex search across the embedded source files                                                             |
|  [18]   | `binlog_search`                       | Build event search in StructuredLog syntax: `$error`, `$task Csc`, `under($project App) CS1234`                   |
|  [19]   | `binlog_explore_node`                 | Ancestors, details, and children of one node id                                                                   |
|  [20]   | `binlog_project_targets`              | Targets that ran in a project, with timing and skip status                                                        |
|  [21]   | `binlog_search_targets`               | Targets by name substring across every project                                                                    |
|  [22]   | `binlog_target_reasons`               | Why a target ran or was skipped: inputs, outputs, up-to-date check, dependency chain                              |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline of one evaluation id                                                                     |
|  [24]   | `binlog_tasks_in_target`              | Tasks inside one target of a project                                                                              |
|  [25]   | `binlog_task_details`                 | Parameters and messages of one task, addressed by project, target, and task name                                  |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration                                                                     |
|  [27]   | `binlog_expensive_targets`            | Slowest targets, aggregated by name                                                                               |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks, aggregated by name                                                                                 |
|  [29]   | `binlog_project_target_times`         | Target timing of one project                                                                                      |
|  [30]   | `binlog_expensive_analyzers`          | Slowest Roslyn analyzers and source generators                                                                    |
|  [31]   | `binlog_analyzer_summary`             | Total time and invocation count per analyzer assembly                                                             |
|  [32]   | `binlog_build_graph`                  | Project dependency graph with durations and the critical path                                                     |
|  [33]   | `binlog_incremental_analysis`         | Targets skipped or rebuilt, the reason, and outputs that `IncrementalClean` deleted                               |
|  [34]   | `binlog_double_writes`                | Files and directories that more than one task, target, or project wrote                                           |
|  [35]   | `binlog_assembly_conflicts`           | `MSB3277` warnings with the `ResolveAssemblyReference` inputs behind them                                         |
|  [36]   | `binlog_compiler`                     | `Csc`, `Vbc`, and `Fsc` command lines with response files                                                         |
|  [37]   | `binlog_nuget`                        | Restored packages, versions, sources, and restore duration                                                        |
|  [38]   | `binlog_assets`                       | `project.assets.json` contents: frameworks, libraries, transitive pins, reverse dependencies of a `package`       |
|  [39]   | `binlog_compare`                      | Property and package diff between two binlogs                                                                     |
|  [40]   | `binlog_extract_preview`              | Size and project count of a subtree extraction, without a write                                                   |
|  [41]   | `binlog_extract`                      | Standalone `.binlog` of the selected projects and their scaffolding                                               |
|  [42]   | `list_mcp_instances`                  | Running server instances with memory and `isOrphaned`, the ones to stop first                                     |
|  [43]   | `stop_instance`                       | Stop one instance by PID                                                                                          |
|  [44]   | `stop`                                | Stop this instance                                                                                                |

## [03]-[OUTPUT_PATH_CLASHES]

Two project instances that share one `OutputPath` or `IntermediateOutputPath` produce:
- `The process cannot access the file because it is being used by another process` in a parallel build
- `Cannot create a file when that file already exists` from restore, because both instances write `project.assets.json` to one directory
- Overwritten or missing output files
- Failures that pass on retry

A clash comes from one of these sources:
- Two projects set one shared output directory
- A multi-targeting or multi-RID build writes to a path that omits the pivot
- One build invokes two solutions that both contain the project
- A path-neutral global property forks a second instance of one project

Ignore instances with `BuildProjectReferences=false`. They are project-reference metadata queries, such as `GetTargetPath`, and write no output.

1. Capture the binlog with `-bl:{}`.
2. Run `binlog_overview` and `binlog_projects`.
3. Run `binlog_evaluations`, then `binlog_evaluation_global_properties` per evaluation id. Look for more than one evaluation per project and for global properties that differ.
4. Run `binlog_evaluation_properties` with `OutputPath,IntermediateOutputPath,BaseOutputPath,BaseIntermediateOutputPath` per evaluation id.
5. Run `binlog_double_writes`. It names the files and directories that more than one instance wrote.
6. Normalize every path to absolute.
7. Group the evaluations by path. A group with more than one evaluation is a clash.

### [03.1]-[GLOBAL_PROPERTIES]

| [INDEX] | [PROPERTY]                                    | [IN_OUTPUT_PATH] | [MEANING]                                                                            |
| :-----: | :-------------------------------------------- | :--------------: | :----------------------------------------------------------------------------------- |
|  [01]   | `TargetFramework`                             |       Yes        | One path per framework                                                               |
|  [02]   | `RuntimeIdentifier`                           |       Yes        | One path per runtime                                                                 |
|  [03]   | `Configuration`                               |       Yes        | One path per configuration                                                           |
|  [04]   | `Platform`                                    |       Yes        | One path per platform                                                                |
|  [05]   | `SolutionFileName`                            |        No        | Solution that built the project, different values mark a multi-solution clash        |
|  [06]   | `SolutionName`, `SolutionPath`, `SolutionDir` |        No        | Same signal as `SolutionFileName`                                                    |
|  [07]   | `CurrentSolutionConfigurationContents`        |        No        | Project entries of the solution, the entry count tells two solutions apart           |
|  [08]   | `BuildProjectReferences`                      |        No        | `false` marks a project-reference query, not a build                                 |
|  [09]   | `MSBuildRestoreSessionId`                     |        No        | Marks a restore evaluation                                                           |
|  [10]   | `PublishReadyToRun`                           |        No        | Publish setting, forks an instance without a path change                             |
|  [11]   | `_IsPublishing`                               |        No        | Set by `dotnet publish`, an `<MSBuild>` call that passes it forks a publish instance |

### [03.2]-[PIVOT_MISSING_FROM_OUTPUT_PATH]

- PROBLEM: A multi-targeting or multi-RID project writes every framework or runtime to one path.
- DETECT: Evaluations of one project differ in `TargetFramework` or `RuntimeIdentifier` and share `OutputPath`.
- FIX: Keep the SDK defaults. `AppendTargetFrameworkToOutputPath` and `AppendRuntimeIdentifierToOutputPath` default to `true` and append the pivot to `OutputPath` and `IntermediateOutputPath`, explicit or default. If the append is off, the explicit `OutputPath` must include the pivot.

```xml
<!-- BAD: the append is off and the path omits the framework -->
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/</OutputPath>

<!-- GOOD: the append is off and the path carries the framework -->
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<OutputPath>bin/$(Configuration)/$(TargetFramework)/</OutputPath>
```

### [03.3]-[SHARED_DIRECTORY_ACROSS_PROJECTS]

- PROBLEM: Two projects set one `BaseOutputPath` or `BaseIntermediateOutputPath`. Restore writes `project.assets.json` to `MSBuildProjectExtensionsPath`, which defaults to `BaseIntermediateOutputPath`, with no framework suffix. `AppendTargetFrameworkToOutputPath` does not separate them.
- DETECT: Evaluations of two projects share `BaseIntermediateOutputPath`, and restore reports `Cannot create a file when that file already exists`.
- FIX: Give each project its own intermediate directory. The SDK default `obj/` in the project directory does this, and so does `ArtifactsPath` in `Directory.Build.props`. See `dotnet-msbuild-evaluation` skill for the artifacts layout.

```xml
<!-- BAD: Directory.Build.props sends every project to one obj/ -->
<BaseOutputPath>../SharedOutput/</BaseOutputPath>
<BaseIntermediateOutputPath>../SharedObj/</BaseIntermediateOutputPath>

<!-- GOOD: one intermediate directory per project -->
<BaseIntermediateOutputPath>../obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
```

### [03.4]-[ONE_PROJECT_IN_TWO_SOLUTIONS]

- PROBLEM: One build invokes two solutions, through the `<MSBuild>` task or the command line, and both contain the project. Each solution builds it with its own `Solution*` global properties, which never change the output path. The first build compiles. The second skips `CoreCompile` and still runs `CopyFilesToOutputDirectory` into the same directory.
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

### [03.5]-[PATH_NEUTRAL_GLOBAL_PROPERTY]

- PROBLEM: A project builds twice inside one solution because an extra global property, such as `PublishReadyToRun=false`, creates a second instance. The property never changes the output path, so MSBuild cannot share the result between the instances, and every target runs twice. A `netstandard2.0` library never uses ReadyToRun and still builds twice.
- DETECT: Evaluations of one project with the same `SolutionFileName` differ in a global property that is not in the path.

| [INDEX] | [PROPERTY]          | [EVALUATION_A]                | [EVALUATION_B]                |
| :-----: | :------------------ | :---------------------------- | :---------------------------- |
|  [01]   | `PublishReadyToRun` | not set                       | `false`                       |
|  [02]   | `OutputPath`        | `bin\Release\netstandard2.0\` | `bin\Release\netstandard2.0\` |

- FIX: Apply one of:
1. Find the parent target or task that passes the property. Remove it from the call for projects that never read it.
2. Set `GlobalPropertiesToRemove="PublishReadyToRun"` on the `ProjectReference`, so the referenced build drops the property.
3. Set the property only on the projects that consume it, such as executables.

### [03.6]-[MSBUILD_TASK_PUBLISH_FORK]

- PROBLEM: A target calls the `<MSBuild>` task on a project with a path-neutral property, most often `_IsPublishing=true` in a publish-on-build target. The call lives in the project itself (a), or in a consumer that publishes a tool (b). The second instance shares `OutputPath` and `IntermediateOutputPath` with the instance the solution builds, and both write the same files.
- DETECT: Two evaluations of the project share the paths and differ only by `_IsPublishing`. `binlog_double_writes` lists the shared files. The parent project of the extra evaluation, from `binlog_explore_node`, tells (a) from (b).
- FIX: See `dotnet-msbuild-antipatterns` skill, `AP-20`. For (a), set `_IsPublishing` as a normal property and run `Publish` through `DependsOnTargets` in the same instance. For (b), apply that fix in the tool and sequence it through a `ProjectReference` with `ReferenceOutputAssembly="false"`. `GlobalPropertiesToRemove` cannot strip a property that a project injects on itself.

```xml
<!-- BAD (a): the project publishes itself -->
<Target Name="PublishOnBuild" AfterTargets="Build">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- BAD (b): a consumer publishes the tool it references -->
<MSBuild Projects="../tool/tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
```

### [03.7]-[SETTARGETFRAMEWORK_ON_A_SINGLE_TARGETING_REFERENCE]

- PROBLEM: A `ProjectReference` sets `SetTargetFramework="TargetFramework=<tfm>"` on a single-targeting project, and the value equals the framework the project declares. The project already resolves to `bin/<config>/<tfm>/`, so the global property forks `(project, {TargetFramework=<tfm>})` beside `(project, {})` on the same paths. The project-reference protocol removes `TargetFramework` for a single-targeting reference. The metadata reintroduces it.
- DETECT: Two evaluations of the referenced project share the paths and differ only by a `TargetFramework` global property, while the project sets `<TargetFramework>`, not `<TargetFrameworks>`.
- FIX: Remove the metadata. `SetTargetFramework` is correct on a multi-targeting reference, and on a single-targeting reference built under a different framework, because both change the path. See `dotnet-msbuild-antipatterns` skill, `AP-21`, for framework-incompatible references and the `SkipGetTargetFrameworkProperties` guard.

```xml
<!-- BAD: Tool.csproj declares net10.0 and the reference injects net10.0 -->
<ProjectReference Include="../Tool/Tool.csproj" SetTargetFramework="TargetFramework=net10.0" />

<!-- GOOD -->
<ProjectReference Include="../Tool/Tool.csproj" />
```
