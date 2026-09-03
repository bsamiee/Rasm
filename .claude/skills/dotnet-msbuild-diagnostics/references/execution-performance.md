# [EXECUTION_PERFORMANCE]

Covers target execution, project scheduling, and task cost in build duration, and every finding is a delta between two captures taken under the same conditions.

## [01]-[COMPARABLE_CAPTURES]

Change only the input or setting under measurement. Hold the command, properties, node count, node reuse, restore state, and build server state constant across the two captures, and keep binary logging on for both, because the logger has its own cost.

| [INDEX] | [CAPTURE]     | [COMMANDS]                                                                   | [MEASURES]                               |
| :-----: | :------------ | :--------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Clean build   | `dotnet build -t:Rebuild -tl:off -bl:<dir>/rebuild-{}.binlog`                | Every target and task from empty outputs |
|  [02]   | Changed input | One successful build, one representative edit, the same command with `-bl`   | The work one edit causes                 |
|  [03]   | No change     | One successful build, then the same command again with `-bl`                 | Targets that run with nothing changed    |
|  [04]   | Build only    | `dotnet restore -bl:<dir>/restore-{}.binlog`, then `--no-restore` with `-bl` | Execution without restore                |

- `dotnet build-server shutdown` stops the MSBuild server and the compiler server before a capture, and `--disable-build-servers` keeps them out of one capture
- `-nr:false` stops node reuse, and the next capture starts its worker nodes again
- Record the chosen state of each with the capture, because a warm server and reused nodes remove process startup from the measured duration
- Use `binlog_compare` for property and package drift between two captures
- Compare the build against its own captures

## [02]-[BINLOG_DIAGNOSIS]

1. Run `binlog_build_graph` for project dependencies, durations, and the critical path
2. Run `binlog_project_target_times` on each project in that path
3. Run `binlog_tasks_in_target` on each slow target
4. Run `binlog_task_details` when task parameters or messages explain the cost

`binlog_expensive_targets` and `binlog_expensive_tasks` aggregate elapsed duration by name across the build, and findings rank by measured duration and critical-path effect.

## [03]-[CRITICAL_PATH_AND_NODES]

The critical path is the duration-weighted chain of project dependencies that sets the minimum build time, and work outside that chain still delays it through contention for nodes and disks.

- `dotnet build` passes `-maxcpucount`, and the `MSBuildNodeCount` property in the binlog records the node count the capture used
- Each node builds one project at a time, and targets inside a project run one after another
- `ResolveProjectReferences` and `_GetProjectReferenceTargetFrameworkProperties` include the referenced builds in their inclusive duration, and their exclusive duration is the project's own cost
- `-clp:PerformanceSummary` prints target and task totals on the console, and `-ds` prints how projects were scheduled to nodes, for a build with no binlog

## [04]-[PROJECT_GRAPH]

Treat a `ProjectReference` as an ordering edge and an output dependency.

- Remove the reference only when the consumer needs neither the output nor the ordering edge
- Set `ReferenceOutputAssembly="false"` when the build needs the ordering edge and the compiler does not consume the output
- Replace a project reference with a package only when the dependency is a prebuilt artifact
- Use a solution filter to reduce the graph

After each graph change, capture the same build again and run `binlog_build_graph` for the new critical path.

## [05]-[STATIC_GRAPH]

`-graph` builds the project graph from declared references before execution and schedules referenced projects before their consumers. `-isolate` enforces the graph and is the only mode that reports `MSB4252`, and a clean `-graph` build without it proves nothing about missing edges.

```bash
dotnet restore Solution.slnx
dotnet build Solution.slnx --no-restore -tl:off -graph -isolate -bl:<dir>/graph-{}.binlog  # restore breaks isolation and runs first
```

- `<MSBuild Projects="...">` calls add no graph edge, and `ProjectReference` or a `ProjectReferenceTargets` entry declares it
- `MSB4252` names the calling project, the called project, and both global-property sets, and the difference between the sets is the undeclared instance
- `GraphIsolationExemptReference` with the full path of a project exempts one reference from the isolation check
- Both switches are experimental, for measurement and for finding edges

## [06]-[MSBUILD_TASK_PARALLELISM]

The `<MSBuild>` task submits the whole project list to the engine at once when one call receives the list and `BuildInParallel` is `true`, and builds each project alone in turn when it is `false`.

```xml
<!-- One call with the whole list, the engine schedules the projects across nodes -->
<MSBuild Projects="@(IndependentProjects)" Targets="Build" BuildInParallel="true" />
```

- Tasks batched with `%(IndependentProjects.Identity)` make one call per project, and each call finishes before the next starts
- `BuildInParallel` defaults to `true` in `Microsoft.Common.CurrentVersion.targets`, and an explicit `false` on a call is the cause when referenced projects build one after another

## [07]-[MULTITHREADED_MODE]

`-mt` builds projects on threads inside one MSBuild process instead of on worker processes, and `-maxCpuCount` still sets the thread count. The switch is experimental and unsupported, for measurement.

## [08]-[RESOLVE_ASSEMBLY_REFERENCE]

When `binlog_expensive_tasks` shows `ResolveAssemblyReference` cost:
1. Run `binlog_task_details` with `task_name=ResolveAssemblyReference` for the slow project
2. Read its `Assemblies` input and search paths
3. Apply the project graph rules before a reference changes

`ResolveAssemblyReference` runs in incremental builds, because targeting packs and installed assemblies can change between builds.

## [09]-[COMPILER_AND_ANALYZERS]

`Csc` includes analyzer and source generator work. The .NET SDK sends every compilation to the compiler server, and `UseSharedCompilation=false` runs `csc` as a process per project, which the `CompilerServer:` message under the `Csc` task records as `server processed compilation` or `using command line tool by design`.

```bash
dotnet build Solution.slnx -t:Rebuild -tl:off -p:ReportAnalyzer=true -bl:<dir>/analyzers-{}.binlog  # per-analyzer timing in the log
```

- Run `binlog_analyzer_summary` on the capture for time and invocation count per analyzer, and rank the outliers
- Analyzers run concurrently, and the reported analyzer time can exceed the `Csc` duration and is no share of it
- `GlobalPackageReference` in `Directory.Packages.props` gives every project the analyzer, and an outlier there costs every compilation
- Change analyzer coverage only when the evidence and the quality policy permit it

## [10]-[COPY_TASKS]

When `binlog_expensive_tasks` shows `Copy` cost:
1. Run `binlog_task_details` with `task_name=Copy` for the slow target
2. Read `SourceFiles`, `DestinationFiles`, and `DestinationFolder`

Then apply the fix the evidence selects:
- Combine independent files in one `Copy` task
- Use `SkipUnchangedFiles="true"` when a size and timestamp comparison is valid for the files
- Remove a copy only when nothing downstream reads the destination file
- `CopyToOutputDirectory="Always"` copies on every build, which `BC0106` reports, and use `dotnet-msbuild-execution` for the mode choice

## [11]-[RESTORE]

Run `binlog_nuget` when restore contributes to the measured build and read its duration, sources, and package count, and the build-only capture separates restore from execution without reducing the combined duration.
- Use `dotnet-msbuild-packaging` for `RestoreUseStaticGraphEvaluation`, lock files, and source configuration
