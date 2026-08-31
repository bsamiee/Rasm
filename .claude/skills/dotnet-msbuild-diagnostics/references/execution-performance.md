# [EXECUTION_PERFORMANCE]

Use this reference when target execution, project scheduling, or task cost contributes to build duration.

## [01]-[BINLOG_DIAGNOSIS]

1. Run `binlog_build_graph` for project dependencies, durations, and the critical path.
2. Run `binlog_project_target_times` on each suspect project in that path.
3. Run `binlog_tasks_in_target` on each slow target.
4. Run `binlog_task_details` when task parameters or messages can explain the cost.
5. Run `binlog_expensive_projects`, `binlog_expensive_targets`, and `binlog_expensive_tasks` for build-wide cost.

The target and task tools aggregate elapsed duration by name across the build. Rank findings by measured duration and critical-path effect. Do not apply universal timing or percentage thresholds.

## [02]-[CRITICAL_PATH_AND_PARALLELISM]

The critical path is the duration-weighted project chain that sets the dependency graph's minimum completion time. Work outside this chain can still delay it through contention.

- `dotnet build` and `dotnet msbuild` both start multiprocess MSBuild. `MSBuildNodeCount` in the binlog reports the node count that the capture used.
- An MSBuild node builds one project at a time. Project dependencies, limited nodes, and shared resources constrain parallel execution.
- `ResolveProjectReferences` can invoke referenced-project builds. Its inclusive duration includes their execution and the wait while the node yields. `_GetProjectReferenceTargetFrameworkProperties` has the same inflated duration. It builds each reference to negotiate its framework.
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

- `-graph` schedules referenced projects before their consumers. It does not make targets incremental or cache results between builds.
- `-isolate` enforces the graph. `MSB4252` reports only under `-isolate`. A clean `-graph` build proves nothing about missing edges. Restore breaks isolation. Pass `--no-restore`.

```bash
dotnet build --no-restore -graph -isolate -bl:static-graph-{} # capture an isolated static graph build
```
- A dynamic `<MSBuild Projects="...">` call does not add a static graph edge. Declare the edge with `ProjectReference`, or add the target to `ProjectReferenceTargets`.
- If the build reports `MSB4252`, read the named project and global properties, then declare the missing edge.
- `-graph` is experimental. Use it to measure scheduling, not as a shipped build setting.

## [05]-[MSBUILD_TASK_PARALLELISM]

An `<MSBuild>` task can build independent projects in parallel when one task receives the project list.

```xml
<!-- Build independent projects in parallel. -->
<MSBuild Projects="@(IndependentProjects)"
         Targets="Build"
         BuildInParallel="true" />
```

Do not batch this task into one call per project. Batching serializes the calls before `BuildInParallel` can schedule the project list.

## [06]-[MULTITHREADED_MODE]

`-mt` builds projects on threads inside one MSBuild process instead of on separate processes. The switch is experimental and unsupported. Use it to measure, never as a build setting.

```bash
dotnet msbuild -mt -bl:multithreaded-{}  # capture a multi-threaded build
```

A task class needs `[MSBuildMultiThreadableTask]` from `Microsoft.Build.Framework` to run in-process on a thread node. Every other task runs in a separate `TaskHost` process and pays the inter-process cost. The attribute is not inherited. `IMultiThreadableTask` supplies `TaskEnvironment` and does not opt the task in.

## [07]-[RESOLVE_ASSEMBLY_REFERENCE]

When `binlog_expensive_tasks` shows `ResolveAssemblyReference` cost:
1. Run `binlog_task_details` for the slow task.
2. Examine its references and search paths.
3. Apply the project graph rules before you change a reference.

`ResolveAssemblyReference` can run during incremental builds. Targeting packs, installed assemblies, and other external inputs can change between builds.

## [08]-[ANALYZERS_AND_GENERATORS]

`Csc` includes analyzer and source-generator work. Capture analyzer timing only after the first binlog identifies compiler cost.

```bash
dotnet build -p:ReportAnalyzer=true -bl:analyzers-{} # capture analyzer and generator timing
```

- Run `binlog_analyzer_summary`. Analyzers run concurrently. Reported analyzer time can exceed elapsed time.
- Rank analyzer outliers. Do not subtract reported analyzer time from `Csc` duration.
- A `GlobalPackageReference` applies to every importing project unless its declaration has a condition.
- Change analyzer coverage only when the evidence and quality policy permit it.

## [09]-[COPY_TASKS]

When `binlog_expensive_tasks` shows `Copy` cost:
1. Run `binlog_task_details` for the slow task.
2. Examine `SourceFiles`, `DestinationFiles`, and `DestinationFolder`.

Then apply the fix the evidence selects:
- Combine independent files in one `Copy` task instead of one task per file.
- Use `SkipUnchangedFiles="true"` only when timestamp and size comparisons are valid for those files.
- Remove a copy only when downstream work does not require the destination file.

## [10]-[RESTORE]

Run `binlog_nuget` when restore contributes to the measured build. Examine restore duration, packages, versions, and sources.

To measure build execution without restore, capture a compatible restore first:

```bash
dotnet restore -bl:restore-{}       # capture the restore that creates the assets
dotnet build --no-restore -bl:{}    # reuse the compatible restore result
```

Use `--no-restore` only when the earlier restore used the same project inputs, properties, SDK, packages, and sources. This separation does not reduce the combined restore and build duration.
