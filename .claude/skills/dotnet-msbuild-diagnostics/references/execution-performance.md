# [EXECUTION_PERFORMANCE]

Use this reference when target execution, project scheduling, or task cost contributes to build duration.

## [01]-[BINLOG_DIAGNOSIS]

Capture the measured build with `-bl:{}`. Keep its command, properties, and parallelism unchanged.

1. Run `binlog_overview` for the total duration and build status.
2. Run `binlog_build_graph` for project dependencies, durations, and the critical path.
3. Run `binlog_project_target_times` on each suspect project in that path.
4. Run `binlog_tasks_in_target` on each slow target.
5. Run `binlog_task_details` when task parameters or messages can explain the cost.
6. Run `binlog_expensive_projects`, `binlog_expensive_targets`, and `binlog_expensive_tasks` for build-wide cost.

`binlog_expensive_projects` ranks projects by exclusive target duration. The target and task tools aggregate cost by name across the build. Rank findings by measured duration and critical-path effect. Do not apply universal timing or percentage thresholds.

## [02]-[CRITICAL_PATH_AND_PARALLELISM]

The critical path is the duration-weighted project chain that bounds build completion. Work outside this chain can still delay it through contention.

- `dotnet build` enables multiprocess MSBuild. A direct `msbuild` invocation uses one node unless you pass `-m` or `-maxCpuCount`.
- An MSBuild node usually builds one project at a time. Project dependencies, limited nodes, and shared resources constrain parallel execution.
- Do not infer underuse from project count alone. Compare the graph timeline with the available node count.

## [03]-[PROJECT_GRAPH]

Treat a `ProjectReference` as both an ordering edge and an output dependency.

- Remove the reference only when the consumer needs neither the output nor the ordering edge.
- Set `ReferenceOutputAssembly="false"` when the build needs the ordering edge but the compiler does not consume the output.
- Replace a project reference with a package only when the dependency is a prebuilt artifact.
- Use a solution filter to reduce the graph scope, not to increase parallelism inside the retained graph.

After each graph change, capture the same build again. Run `binlog_build_graph` to examine the new critical path.

## [04]-[STATIC_GRAPH]

Static graph mode creates the project graph from declared project references before target execution.

```bash
dotnet build -graph -bl:static-graph-{} # capture a static graph build
```

- Static graph mode schedules referenced projects before their consumers. It does not make targets incremental or cache results between builds.
- A dynamic `<MSBuild Projects="...">` call does not add a static graph edge. Declare the edge with `ProjectReference`, or isolate that build.
- If the build reports `MSB4252`, examine the named project and global properties. Correct the graph or isolation problem that the message identifies.

## [05]-[MSBUILD_TASK_PARALLELISM]

An `<MSBuild>` task can build independent projects in parallel when one task receives the project list.

```xml
<!-- Build independent projects in parallel. -->
<MSBuild Projects="@(IndependentProjects)"
         Targets="Build"
         BuildInParallel="true" />
```

Do not batch this task into one call per project. Batching serializes the calls before `BuildInParallel` can schedule the project list.

## [06]-[TASK_CONCURRENCY]

MSBuild 18.4 and later can run eligible tasks concurrently inside one MSBuild process with `-mt`.

```bash
dotnet msbuild -mt -bl:task-concurrency-{}  # capture concurrent task execution
```

A concrete task class needs `[MSBuildMultiThreadableTask]` before MSBuild can run it in-process. `IMultiThreadableTask` supplies `TaskEnvironment`; it does not opt in the task.

## [07]-[RESOLVE_ASSEMBLY_REFERENCE]

Use this workflow when `binlog_expensive_tasks` shows material `ResolveAssemblyReference` cost.

1. Run `binlog_task_details` for the slow task.
2. Examine its references and search paths.
3. Apply the project-reference rules in section `[03]-[PROJECT_GRAPH]` before you change a reference.

`ResolveAssemblyReference` can run during incremental builds. Targeting packs, installed assemblies, and other external inputs can change between builds.

## [08]-[ANALYZERS_AND_GENERATORS]

`Csc` includes analyzer and source-generator work. Capture analyzer timing only after the first binlog identifies compiler cost.

```bash
dotnet build -p:ReportAnalyzer=true -bl:analyzers-{} # capture analyzer and generator timing
```

- Run `binlog_expensive_analyzers` and `binlog_analyzer_summary`. Reported analyzer time can exceed elapsed time because analyzers run concurrently.
- Rank analyzer outliers. Do not subtract reported analyzer time from `Csc` duration.
- A `GlobalPackageReference` applies to every importing project unless its declaration has a condition.
- Change analyzer coverage only when the evidence and quality policy permit it.

## [09]-[COPY_TASKS]

Use this workflow when `binlog_expensive_tasks` shows material `Copy` cost.

1. Run `binlog_task_details` for the slow task.
2. Examine `SourceFiles`, `DestinationFiles`, and `DestinationFolder`.
3. Combine independent files in one `Copy` task instead of one task per file.
4. Use `SkipUnchangedFiles="true"` only when timestamp and size comparisons are valid for those files.
5. Remove copies only when downstream work does not require the destination files.

## [10]-[RESTORE]

Run `binlog_nuget` when restore contributes to the measured build. Examine restore duration, packages, versions, and sources.

To measure build execution without restore, capture a compatible restore first:

```bash
dotnet restore -bl:restore-{}       # capture the restore that creates the assets
dotnet build --no-restore -bl:{}    # reuse the compatible restore result
```

Use `--no-restore` only when the earlier restore used the same project inputs, properties, SDK, packages, and sources. This separation does not reduce the combined restore and build duration.
