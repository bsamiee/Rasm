---
name: dotnet-msbuild-diagnostics
description: "enter a description here"
---

# [DOTNET_MSBUILD_DIAGNOSTICS]

This skill diagnoses MSBuild build failures, build optimization, and other diagnostic needs, using the binglog MCP server `Microsoft.AITools.BinlogMcp`.
- The MCP server exposes structured tools for inspecting a `.binlog` without parsing text logs. Call them directly instead of replaying the binlog to a text file.
- The `.binlog` file is a binary format, do NOT try to `cat`, `head`, `strings`, or read it directly. Use only the MCP tools to query it.
- Synthesize findings as you go:  Do NOT spend all available time investigating, once you have enough evidence, present your conclusions.

## [01]-[BINLOG_MCP_TOOLS]

Use the available MCP server tools to query the binary log for:
- Build errors and warnings
- MSBuild properties and their values
- MSBuild items
- Project evaluation data
- Target execution details
- File contents embedded in the binlog

| [INDEX] | [TOOL]                                | [DESCRIPTION]                                                                                                              |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `binlog_capabilities`                 | Reports the server contract version and which tools emit the canonical JSON envelope                                       |
|  [02]   | `binlog_overview`                     | Build overview: status, duration, project count, error/warning counts                                                      |
|  [03]   | `binlog_diagnose`                     | Automated build diagnosis: analyzes the build for common issues and returns a structured report                            |
|  [04]   | `binlog_errors`                       | Deduplicated build errors as compact triage text                                                                           |
|  [05]   | `binlog_warnings`                     | Deduplicated build warnings as compact triage text                                                                         |
|  [06]   | `binlog_projects`                     | List all projects in the build with status and duration                                                                    |
|  [07]   | `binlog_evaluations`                  | List project evaluations in the build, each represents a project+TFM combination with its own properties and items         |
|  [08]   | `binlog_evaluation_properties`        | Get properties from a specific project evaluation. Use binlog_evaluations first to find evaluation IDs                     |
|  [09]   | `binlog_evaluation_global_properties` | Get the global properties for a specific project evaluation                                                                |
|  [10]   | `binlog_properties`                   | MSBuild property values for a project, returns curated key properties, with filter returns matching properties             |
|  [11]   | `binlog_explain_property`             | Traces where a property gets its value — which file, target, or task set it, and returns a deduplicated summary            |
|  [12]   | `binlog_compare_property`             | Compare a property's value across ALL projects in one call                                                                 |
|  [13]   | `binlog_items`                        | MSBuild items for a project, lists available item types when called without itemType                                       |
|  [14]   | `binlog_imports`                      | Full import chain for a project — shows all .props/.targets files imported, including missing imports                      |
|  [15]   | `binlog_preprocess`                   | Show a preprocessed view of a project file — the project source with its full import chain                                 |
|  [16]   | `binlog_files`                        | Rtrieve source files embedded in the binlog                                                                                |
|  [17]   | `binlog_search_files`                 | Search text across all embedded source files in the binlog                                                                 |
|  [18]   | `binlog_search`                       | Search build events using StructuredLog Viewer query syntax, returns matching nodes as an indented tree                    |
|  [19]   | `binlog_explore_node`                 | Navigate the build log tree by node ID, shows the node's ancestor chain (path from root), details, and children            |
|  [20]   | `binlog_project_targets`              | List targets executed in a specific project with timing and skip status                                                    |
|  [21]   | `binlog_search_targets`               | Search for targets by name across all projects                                                                             |
|  [22]   | `binlog_target_reasons`               | Why a target ran or was skipped: shows input/output items, up-to-date checks, and dependency chain                         |
|  [23]   | `binlog_target_graph`                 | Executed-target timeline for a single project evaluation, returns targetGraph JSON envelope                                |
|  [24]   | `binlog_tasks_in_target`              | List all tasks within a specific target of a project, use to drill into what a target does                                 |
|  [25]   | `binlog_task_details`                 | Get detailed info about a specific task execution including its input parameters and output messages                       |
|  [26]   | `binlog_expensive_projects`           | Slowest projects by exclusive target duration. Use for performance investigation                                           |
|  [27]   | `binlog_expensive_targets`            | Slowest targets across the entire build, aggregated by name                                                                |
|  [28]   | `binlog_expensive_tasks`              | Slowest tasks across the entire build, aggregated by name                                                                  |
|  [29]   | `binlog_project_target_times`         | Target-level timing breakdown for a specific project                                                                       |
|  [30]   | `binlog_expensive_analyzers`          | Slowest Roslyn analyzers and source generators by duration                                                                 |
|  [31]   | `binlog_analyzer_summary`             | Analyzer execution summary, total time and invocation count per analyzer assembly, returns analyzerSummary JSON envelope   |
|  [32]   | `binlog_build_graph`                  | Project dependency graph: projects with durations, dependencies, and critical path, returns buildGraph JSON envelope       |
|  [33]   | `binlog_incremental_analysis`         | Incremental build analysis: which targets were skipped as up-to-date vs rebuilt, with inferred reason and triggering files |
|  [34]   | `binlog_double_writes`                | Detect files written by multiple tasks/targets during the build                                                            |
|  [35]   | `binlog_assembly_conflicts`           | Extracts all MSB3277 warnings and the (RAR) task parameters that caused them                                               |
|  [36]   | `binlog_compiler`                     | Extract compiler (Csc/Vbc/Fsc) command-line invocations from the build, including response files and key switches          |
|  [37]   | `binlog_nuget`                        | NuGet package restore information: resolved packages, versions, sources, and restore duration                              |
|  [38]   | `binlog_assets`                       | Investigate the project.assets.json files embedded in a binlog                                                             |
|  [39]   | `binlog_compare`                      | Diff two binlogs — compares properties, per-project packages, and solution-wide packages                                   |
|  [40]   | `binlog_extract_preview`              | Describe what a subtree extraction would produce without writing a file                                                    |
|  [41]   | `binlog_extract`                      | Write a new, valid, standalone .binlog containing only selected projects and minimum scaffolding needed to reach them      |
|  [42]   | `list_mcp_instances`                  | Lists all running binlog-mcp server instances on this machine, including the current one                                   |
|  [43]   | `stop_instance`                       | Stops a specific binlog-mcp instance by PID to free the memory held by its loaded binlog                                   |
|  [44]   | `stop`                                | Stops only this binlog-mcp server instance                                                                                 |

## [01]-[GENERATE_BINARY_LOGS]

Pass the `/bl` switch when running any MSBuild-based command. This is a non-negotiable requirement for all .NET builds. Use `{}` for Automatic Unique Names:
- The `{}` placeholder in the binlog filename is replaced by MSBuild with a unique identifier
- Gurantees no two builds ever overwrite each other without needing to track or check existing files

You MUST add the `/bl:{}` flag to:
- `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, `dotnet restore`, `msbuild` or `msbuild.exe`
- Any other command that invokes MSBuild

```bash
# Every invocation produces a distinct file automatically
dotnet build /bl:{}
dotnet test /bl:{}
dotnet build --configuration Release /bl:{}
```

Why This Matters:
- Unique names prevent overwrites: You can always go back and analyze previous builds
- Failure analysis: When a build fails, the binlog is already there for immediate analysis
- Comparison: You can compare builds before and after changes
- No re-running builds: You never need to re-run a failed build just to generate a binlog

One build = one binlog
- Add `/bl:{}` to every MSBuild invocation separately, never reuse a name and never rely on bare `/bl`
- Building several configurations, projects, or retrying a failed build? Each command still gets its own `/bl:{}` so the logs never overwrite each other

```bash
dotnet build -c Debug   /bl:{}   # unique file
dotnet build -c Release /bl:{}   # another unique file
```

Verify the binlog exists:
- After the build, confirm a `.binlog` was actually produced before moving on to analysis
- A build that fails before MSBuild starts (e.g. a bad argument) writes no binlog:

```bash
ls -1 *.binlog     # bash
dir /b *.binlog    # Windows cmd
```

NOTE: The resulting path so `binlog-failure-analysis` or `build-perf-diagnostics` can consume it.

If the binlog filename needs to be known upfront (e.g., for CI artifact upload), or if `{}` is not available in the installed MSBuild version, pick a name that won't collide with existing files:
1. Check for existing `*.binlog` files in the directory
2. Choose a name not already taken (e.g., by incrementing a counter from the highest existing number)

```bash
# Example: directory contains 3.binlog — use 4.binlog
dotnet build /bl:4.binlog
```

Cleaning the Repository:
- When cleaning the repository with `git clean`, always exclude binlog files to preserve your build history:

```bash
# ✅ CORRECT - Exclude binlog files from cleaning
git clean -fdx -e "*.binlog"

# ❌ WRONG - This deletes binlog files (they're usually in .gitignore)
git clean -fdx
```

NOTE: This is important when iterating on build fixes, you need the binlogs to analyze what changed between builds.

## Detecting OutputPath and IntermediateOutputPath Clashes

Identify when multiple MSBuild project evaluations share the same `OutputPath` or `IntermediateOutputPath`. This is a common source of build failures including:
- File access conflicts during parallel builds
- Missing or overwritten output files
- Intermittent build failures
- "File in use" errors
- NuGet restore errors like `Cannot create a file when that file already exists`, indicates multiple projects share the same `IntermediateOutputPath` where `project.assets.json` is written

Clashes can occur between:
- Different projects sharing the same output directory
- Multi-targeting builds (e.g., `TargetFrameworks=net8.0;net9.0`) where the path doesn't include the target framework
- Multiple solution builds where the same project is built from different solutions in a single build

NOTE: Project instances with `BuildProjectReferences=false` should be ignored when analyzing clashes, these are P2P reference resolution builds that only query metadata `GetTargetPath` and do not write to output directories.

- Step 1: Generate a binary log with the correct naming convention
- Step 2: Get an overview and list projects

- Step 3: Check evaluations and global properties, use `evaluations` and `evaluation_global_properties` tools to find all evaluations per project, for:
  - Multiple evaluations for the same project (indicates multi-targeting or multiple build configurations)
  - Differing global properties between evaluations (`TargetFramework`, `Configuration`, `RuntimeIdentifier`, `SolutionFileName`, `PublishReadyToRun`, etc.)

- Step 4: Get output paths for each evaluation, use `OutputPath`, `IntermediateOutputPath`, `BaseOutputPath`, and `BaseIntermediateOutputPath` for each project evaluation
- Step 5: Check for double writes, use `double_writes` it directly detects files written by multiple project instances

- Step 6: Identify clashes, compare the `OutputPath` and `IntermediateOutputPath` values across all evaluations:
1. Normalize paths: Convert to absolute paths and normalize separators
2. Group by path: Find evaluations that share the same OutputPath or IntermediateOutputPath
3. Filter out non-build evaluations: Exclude `BuildProjectReferences=false` instances (P2P queries)
4. Report clashes: Any group with more than one evaluation indicates a clash

## Common Causes and Fixes

Tips:
- The SDK default paths include `$(TargetFramework)`, clashes often occur when projects override these defaults; normalize relative paths to absolute before comparing.
- Cross-project `IntermediateOutputPath` clashes cannot be fixed with `AppendTargetFrameworkToOutputPath`, files like `project.assets.json` are written directly to the intermediate path. For multi-targeting clashes within the same project, `AppendTargetFrameworkToOutputPath=true` is the correct fix.
- Error messages that indicate a path clash: `Cannot create a file when that file already exists` (NuGet restore), `The process cannot access the file because it is being used by another process`, or intermittent failures that succeed on retry.

When multiple evaluations share an output path, compare these global properties to understand why:

| [INDEX] | [PROPERTY]                             | [AFFECTS_OUTPUTPATH] | [GUIDANCE]                                                                                         |
| :-----: | :------------------------------------- | :------------------: | :------------------------------------------------------------------------------------------------- |
|  [01]   | `TargetFramework`                      |         Yes          | Different TFMs should have different paths                                                         |
|  [02]   | `RuntimeIdentifier`                    |         Yes          | Different RIDs should have different paths                                                         |
|  [03]   | `Configuration`                        |         Yes          | Debug vs Release                                                                                   |
|  [04]   | `Platform`                             |         Yes          | AnyCPU vs x64 etc                                                                                  |
|  [05]   | `SolutionFileName`                     |          No          | Identifies which solution built the project, different values indicate multi-solution clash        |
|  [06]   | `SolutionName`                         |          No          | Solution name without extension                                                                    |
|  [07]   | `SolutionPath`                         |          No          | Full path to the solution file                                                                     |
|  [08]   | `SolutionDir`                          |          No          | Directory containing the solution file                                                             |
|  [09]   | `CurrentSolutionConfigurationContents` |          No          | XML with project entries, count of entries reveals which solution                                  |
|  [10]   | `BuildProjectReferences`               |          No          | `false` = P2P query, not a real build, ignore these                                                |
|  [11]   | `MSBuildRestoreSessionId`              |          No          | Present = restore phase evaluation                                                                 |
|  [12]   | `PublishReadyToRun`                    |          No          | Publish setting, doesn't change build output path, creates distinct project instances              |
|  [13]   | `_IsPublishing`                        |          No          | `<MSBuild>` Build/Publish call with this in this project or consuming one forks a publish instance |

### Multi-targeting without TargetFramework in path

- Problem: Project uses `TargetFrameworks` but OutputPath doesn't vary by framework
- Fix: Include TargetFramework in the path, or rely on SDK defaults which handle this automatically

```xml
<!-- BAD: Same path for all frameworks -->
<OutputPath>bin\$(Configuration)\</OutputPath>
```

```xml
<!-- GOOD: Path varies by framework -->
<OutputPath>bin\$(Configuration)\$(TargetFramework)\</OutputPath>
```

```xml
<AppendTargetFrameworkToOutputPath>true</AppendTargetFrameworkToOutputPath>
<AppendTargetFrameworkToIntermediateOutputPath>true</AppendTargetFrameworkToIntermediateOutputPath>
```

### Shared output directory across projects (CANNOT be fixed with AppendTargetFramework)

- Problem: Multiple projects explicitly set the same `BaseOutputPath` or `BaseIntermediateOutputPath`
- Fix: Each project MUST have a unique `BaseIntermediateOutputPath`, Or simply use the SDK defaults which place `obj` inside each project's directory

```xml
<!-- Project A - Directory.Build.props -->
<BaseOutputPath>..\SharedOutput\</BaseOutputPath>
<BaseIntermediateOutputPath>..\SharedObj\</BaseIntermediateOutputPath>

<!-- Project B - Directory.Build.props -->
<BaseOutputPath>..\SharedOutput\</BaseOutputPath>
<BaseIntermediateOutputPath>..\SharedObj\</BaseIntermediateOutputPath>
```

```xml
<!-- Project A -->
<BaseIntermediateOutputPath>..\obj\ProjectA\</BaseIntermediateOutputPath>

<!-- Project B -->
<BaseIntermediateOutputPath>..\obj\ProjectB\</BaseIntermediateOutputPath>
```

[IMPORTANT]: Even with `AppendTargetFrameworkToOutputPath=true`, this will still clash! .NET writes certain files directly to the `IntermediateOutputPath` without the TargetFramework suffix, including: `project.assets.json` (NuGet restore output), andother NuGet-related files This causes errors like `Cannot create a file when that file already exists` during parallel restore.

RuntimeIdentifier builds clashing
- Problem: Building for multiple RIDs without RID in path
- Fix: Ensure RuntimeIdentifier is in the path

```xml
<AppendRuntimeIdentifierToOutputPath>true</AppendRuntimeIdentifierToOutputPath>
```

### Multiple solutions building the same project

- Problem: A single build invokes multiple solutions (via MSBuild task or command line) that include the same project. Each solution build evaluates and builds the project independently, with different `Solution*` global properties that don't affect the output path.
- How to detect: Compare `SolutionFileName` and `CurrentSolutionConfigurationContents` across evaluations for the same project. Different values indicate multi-solution builds. For example:

| [INDEX] | [PROPERTY]                             | [EVAL_FROM_A]                 | [EVAL_FROM_B]                         |
| :-----: | :------------------------------------- | :---------------------------- | :------------------------------------ |
|  [01]   | `SolutionFileName`                     | `BuildAnalyzers.sln`          | `Main.slnx`                           |
|  [02]   | `CurrentSolutionConfigurationContents` | 1 project entry               | ~49 project entries                   |
|  [03]   | `OutputPath`                           | `bin\Release\netstandard2.0\` | `bin\Release\netstandard2.0\` ← clash |

Example: A repo build script builds `BuildAnalyzers.sln` then `Main.slnx`, and both solutions include `SharedAnalyzers.csproj`. Both builds write to `bin\Release\netstandard2.0\`. The first build compiles; the second skips compilation but still runs `CopyFilesToOutputDirectory`.

Fix: Options include:
1. Consolidate solutions: Ensure each project is only built from one solution in a single build
2. Use different configurations: Build solutions with different `Configuration` values that result in different output paths
3. Exclude duplicate projects: Use solution filters or conditional project inclusion to avoid building the same project twice

### Extra global properties creating redundant project instances

- Problem: A project is built multiple times within the same solution due to extra global properties (e.g., `PublishReadyToRun=false`) that create distinct MSBuild project instances. These properties don't affect output paths but prevent MSBuild from caching results across instances, causing redundant target execution.
- How to detect: Compare global properties across evaluations for the same project within the same solution (same `SolutionFileName`). Look for properties that differ but don't contribute to path differentiation:

| [INDEX] | [PROPERTY]          | [EVAL_FROM_A]                 | [EVAL_FROM_B]                         |
| :-----: | :------------------ | :---------------------------- | :------------------------------------ |
|  [01]   | `PublishReadyToRun` | not set                       | `false`                               |
|  [02]   | `OutputPath`        | `bin\Release\netstandard2.0\` | `bin\Release\netstandard2.0\` ← clash |

This is particularly wasteful for projects where the extra property has no effect (Ex: `PublishReadyToRun` on a `netstandard2.0` class library that doesn't use ReadyToRun compilation).

Fix: Options include:
1. Remove the extra global property: Investigate which parent target/task is injecting the property and prevent it from being passed to projects that don't need it
2. Use `RemoveGlobalProperties` metadata: On `ProjectReference` items, use `RemoveGlobalProperties="PublishReadyToRun"` to strip the property before building the referenced project
3. Condition the property: Only set the property on projects that actually use it (e.g., only for executable projects, not class libraries)

###  Explicit `<MSBuild>` Build/Publish with extra global properties (self or cross-project)

- Problem: A target uses the `<MSBuild>` task to build or publish a project with an extra global property, most commonly a "publish-on-build" target. The offending call can be in the target project itself or in another project that consumes it (e.g. a test or layout project publishing a tool).
- How to detect: Follow the Primary workflow above — the `evaluations` and `evaluation_global_properties` tools surface two evaluations of the target project that share the same `OutputPath`/`IntermediateOutputPath` but differ only by a path-neutral publish flag such as `_IsPublishing`, and the `double_writes` tool flags the resulting shared-file writes directly. To tell case (a) from (b), see which project the extra `{_IsPublishing=true}` evaluation runs *under* in the build tree (from the overview/projects tools): the target project itself for (a), or a consumer project that invoked the `<MSBuild>` task for (b).

```xml
<!-- (a) same project (publish-on-build) -->
<Target Name="PublishOnBuild" AfterTargets="Build">
  <MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
</Target>

<!-- (b) project A publishes project B that it consumes -->
<MSBuild Projects="..\tool\tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
```

Either way this forks a distinct instance of the target project (`path` + `{_IsPublishing=true}`) that shares the same `OutputPath`/`IntermediateOutputPath` as the instance the solution/graph already builds. Both write the same files — for NativeAOT this includes the `*.sourcelink` intermediate, which produces `SourceLinkWriter` / "file in use" failures under parallel builds.

Fix: Depends on where the call lives:
- Same project (a): you can't strip the property with `RemoveGlobalProperties` (the project injects it on itself).
- Set the flag as a static (non-global) property and run the target in the same instance via `DependsOnTargets`/`CallTarget`, with a guard against a target cycle when publish is the entry point:

```xml
<PropertyGroup>
  <_PublishWasInvokedDirectly Condition="'$(_IsPublishing)' == 'true'">true</_PublishWasInvokedDirectly>
  <_IsPublishing>true</_IsPublishing>
</PropertyGroup>
<Target Name="PublishOnBuild"
        AfterTargets="Build"
        DependsOnTargets="Publish"
        Condition="'$(_PublishWasInvokedDirectly)' != 'true'" />
```

- Cross-project (b): the consumer must not fork the producer with path-neutral global properties. Make the producer publish as part of its own build (the (a) fix in its project), then have the consumer sequence it and read its output instead of re-publishing it:

```xml
<ItemGroup>
  <ProjectReference Include="..\tool\tool.csproj" ReferenceOutputAssembly="false" />
</ItemGroup>
<!-- consumer reads tool's publish dir; it does NOT invoke Publish on tool -->
```

See the `msbuild-antipatterns` skill (AP-22) for the authoring-time smell and rationale.

### `SetTargetFramework` re-injecting a single-targeting project's own TFM on a `ProjectReference`

- Problem: A `ProjectReference` sets `SetTargetFramework="TargetFramework=<tfm>"` metadata pointing at a Single-targeting project (one that uses singular `<TargetFramework>`, not `<TargetFrameworks>`), where the injected `<tfm>` equals the TFM the project already targets. `SetTargetFramework` injects `TargetFramework` as a global property on the referenced project's build.
- How to detect: Follow the Primary workflow above. The `evaluations` and `evaluation_global_properties` tools surface two evaluations of the referenced project that share the same `OutputPath`/`IntermediateOutputPath` and differ only by a `TargetFramework` global property, while the project itself is single-targeting (its own `TargetFramework` already equals the injected value). The `double_writes` tool flags the resulting shared-file writes directly.
- Note: The P2P protocol itself does not inject `TargetFramework` for a non-multi-targeting reference — the clash comes specifically from the explicit `SetTargetFramework` metadata overriding that safe default.

```xml
<!-- BAD: Tool.csproj single-targets net8.0 and we inject that SAME net8.0 -->
<ProjectReference Include="..\Tool\Tool.csproj" SetTargetFramework="TargetFramework=net8.0" />
```

Injecting the TFM the project already targets is path-neutral — the project already resolves to `bin\<config>\net8.0\` and `obj\<config>\net8.0\` on its own. So it doesn't change the output path; it only forks a distinct instance `(project, {TargetFramework=net8.0})`. The solution/graph builds the very same project as `(project, {})`. Both share the same `OutputPath`/`IntermediateOutputPath`, so the project is built twice to the same location — a bin/obj clash under parallel builds.

Fix: Remove the redundant `SetTargetFramework` when it just restates the project's own single TFM:

```xml
<!-- GOOD -->
<ProjectReference Include="..\Tool\Tool.csproj" />
```

When `SetTargetFramework` is legitimate (not a clash):
- Multi-targeting reference: the referenced project uses `<TargetFrameworks>` and you need one specific TFM. Each TFM has a distinct output path, so no clash.
- Overriding to a different TFM:  you may use `SetTargetFramework` on a single-targeting project to build it under a TFM other than the one it declares. Because the injected TFM then changes the output path (`obj\<config>\<different-tfm>\`), the instance no longer collides with `(project, {})`. Only the same-TFM case is path-neutral and clashing.
- Framework-incompatible reference: Whenever the referencing and referenced projects target incompatible frameworks (e.g. a `.NETFramework` project referencing a `.NETCoreApp` project, or vice-versa) — regardless of single- or multi-targeting on either side — set `SkipGetTargetFrameworkProperties="true"` (the P2P `GetTargetFrameworkProperties` negotiation would otherwise fail) and `ReferenceOutputAssembly="false"` (an assembly built for an incompatible framework can't be consumed as a reference — you only want to trigger/sequence the build):

```xml
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  ReferenceOutputAssembly="false" />
```

With `SkipGetTargetFrameworkProperties="true"`, the negotiation no longer stops the referencing project's own `TargetFramework` global property (present when it builds for a specific TFM, e.g. it is multi-targeting) from flowing into the referenced project. For a single-targeting referenced project that would force it to build under the wrong TFM / output path. Prevent it by either setting `SetTargetFramework="TargetFramework=<tfm>"` (pin the TFM) or `UndefineProperties="TargetFramework"` (strip the inherited global property so the project builds as it declares) — use one, not both:

```xml
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  UndefineProperties="TargetFramework"
                  ReferenceOutputAssembly="false" />
```

See the `dotnet-msbuild-antipatterns` skill for the authoring-time smell and rationale.
