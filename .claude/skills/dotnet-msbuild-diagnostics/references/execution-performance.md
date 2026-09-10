# [EXECUTION_PERFORMANCE]

Target execution, project scheduling, and task cost in build duration. Every finding is a delta between two captures taken under the same conditions.

## [01]-[COMPARABLE_CAPTURES]

Change the input or setting under measurement alone. Hold the command, properties, node count, node reuse, restore state, and build server state constant across both captures, keep binary logging on for both, the logger has its own cost.

| [INDEX] | [CAPTURE]     | [COMMANDS]                                                                   | [MEASURES]                               |
| :-----: | :------------ | :--------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Clean build   | `dotnet build -t:Rebuild -bl:<dir>/rebuild-{}.binlog`                        | Every target and task from empty outputs |
|  [02]   | Changed input | One successful build, one representative edit, the same command with `-bl`   | Work one edit causes                     |
|  [03]   | No change     | One successful build, then the same command again with `-bl`                 | Targets that run with nothing changed    |
|  [04]   | Build only    | `dotnet restore -bl:<dir>/restore-{}.binlog`, then `--no-restore` with `-bl` | Execution without restore                |

- `dotnet build-server shutdown` stops the MSBuild and compiler servers before a capture, `--disable-build-servers` keeps them out of one capture
- `-nr:false` stops node reuse, the next capture starts its worker nodes again
- Record the chosen state of each with the capture, a warm server and reused nodes remove process startup from the measured duration
- Restore and capture both under `--artifacts-path <dir>`, a build another session runs into the shared `ArtifactsPath` changes the measured work
- `binlog_compare` shows property and package drift between two captures
- Compare the build against its own captures

## [02]-[BINLOG_DIAGNOSIS]

1. Run `binlog_build_graph` for project dependencies, durations, and the critical path
2. Run `binlog_project_target_times` on each project in that path
3. Run `binlog_tasks_in_target` on each slow target
4. Run `binlog_task_details` when task parameters or messages explain the cost

`binlog_expensive_targets` and `binlog_expensive_tasks` aggregate elapsed duration by name across the build, findings rank by measured duration and critical-path effect.

## [03]-[CRITICAL_PATH_AND_NODES]

The critical path is the duration-weighted chain of project dependencies setting the minimum build time, work outside that chain delays it through contention for nodes and disks.

- `dotnet build` passes `-maxcpucount`, the `MSBuildNodeCount` property in the binlog records the node count
- Each node builds one project at a time, targets inside a project run one after another
- `ResolveProjectReferences` and `_GetProjectReferenceTargetFrameworkProperties` include the referenced builds in their inclusive duration
- Exclusive duration of those targets is the project's own cost
- `-clp:PerformanceSummary` prints target and task totals on the console, `-ds` prints how projects were scheduled to nodes

## [04]-[PROJECT_GRAPH]

A `ProjectReference` is an ordering edge and an output dependency.

- Remove the reference when the consumer needs neither the output nor the ordering edge
- Set `ReferenceOutputAssembly="false"` when the build needs the ordering edge and the compiler does not consume the output
- Replace a project reference with a package when the dependency is a prebuilt artifact
- A solution filter reduces the graph

After each graph change, capture the same build again and run `binlog_build_graph` for the new critical path.

## [05]-[STATIC_GRAPH]

`-graph` builds the project graph from declared references before execution and schedules referenced projects before their consumers. `-isolate` enforces the graph and is the one mode that reports `MSB4252`, a clean `-graph` build without it proves nothing about missing edges.

```bash
dotnet restore Solution.slnx
dotnet build Solution.slnx --no-restore -graph -isolate -bl:<dir>/graph-{}.binlog
```

- `dotnet restore` runs first, restore breaks isolation under `--no-restore -graph -isolate`
- `<MSBuild Projects="...">` calls add no graph edge, `ProjectReference` or a `ProjectReferenceTargets` entry declares it
- `MSB4252` names the calling project, the called project, and both global-property sets, the difference between the sets is the undeclared instance
- `GraphIsolationExemptReference` with the full path of a project exempts one reference from the isolation check
- Both switches are experimental, for measurement and for finding edges

## [06]-[MSBUILD_TASK_PARALLELISM]

The `<MSBuild>` task submits the whole project list to the engine at once when one call receives the list and `BuildInParallel` is `true`, and builds each project alone in turn when it is `false`.

```xml
<MSBuild Projects="@(IndependentProjects)" Targets="Build" BuildInParallel="true" />
```

- A task batched with `%(IndependentProjects.Identity)` makes one call per project, each call finishes before the next starts
- `BuildInParallel` defaults to `true` in `Microsoft.Common.CurrentVersion.targets`, an explicit `false` on a call serializes the referenced projects

## [07]-[MULTITHREADED_MODE]

`-mt` builds projects on threads inside one MSBuild process in place of worker processes, `-maxCpuCount` sets the thread count. The switch is experimental and unsupported, for measurement.

## [08]-[RESOLVE_ASSEMBLY_REFERENCE]

When `binlog_expensive_tasks` shows `ResolveAssemblyReference` cost:
1. Run `binlog_task_details` with `task_name=ResolveAssemblyReference` for the slow project
2. Read its `Assemblies` input and search paths
3. Apply the project graph rules before a reference changes

`ResolveAssemblyReference` runs in incremental builds, targeting packs and installed assemblies can change between builds.

## [09]-[COMPILER_AND_ANALYZERS]

`Csc` includes analyzer and source generator work. The SDK sends every compilation to the compiler server, `UseSharedCompilation=false` runs `csc` as a process per project, the `CompilerServer:` message under the `Csc` task records `server processed compilation` or `using command line tool by design`.

```bash
dotnet build Solution.slnx -t:Rebuild -p:ReportAnalyzer=true -bl:<dir>/analyzers-{}.binlog
```

- Run `binlog_analyzer_summary` on the capture for time and invocation count per analyzer, rank the outliers
- Analyzers run concurrently, the reported analyzer time can exceed the `Csc` duration
- `GlobalPackageReference` in `Directory.Packages.props` gives every project the analyzer, an outlier there costs every compilation
- Change analyzer coverage when the evidence and the quality policy permit it

## [10]-[COPY_TASKS]

When `binlog_expensive_tasks` shows `Copy` cost:
1. Run `binlog_task_details` with `task_name=Copy` for the slow target
2. Read `SourceFiles`, `DestinationFiles`, and `DestinationFolder`

Then the fix the evidence selects:
- Combine independent files in one `Copy` task
- `SkipUnchangedFiles="true"` when a size and timestamp comparison is valid for the files
- Remove a copy when nothing downstream reads the destination file

## [11]-[RESTORE]

Run `binlog_nuget` when restore contributes to the measured build, read its duration, sources, and package count. The build-only capture separates restore from execution without reducing the combined duration.
