# [EVALUATION_AND_INCREMENTALITY]

Covers evaluation time, projects that evaluate more often than expected, and the work a no-change build still executes.

## [01]-[EVALUATION]

MSBuild evaluates a project before it executes targets, once per project instance, and the evaluation walks every import, property, item glob, and property function in order.

### [01.1]-[MEASUREMENT]

```bash
dotnet build <project> -tl:off -profileEvaluation:<dir>/evaluation-{}.md   # one row per import, property, item, and target
dotnet build <project> -tl:off -v:diag | rg 'Property reassignment'
```

- `-profileEvaluation` reports inclusive and exclusive time, grouped into the passes of evaluation, and the glob rows include the directory enumeration cost
- Class libraries with no custom files print more than a hundred `Property reassignment:` lines from the SDK, and only the lines that name a repository file matter

### [01.2]-[BINLOG_DIAGNOSIS]

1. Run `binlog_evaluations` to find the slow or repeated evaluations
2. Run `binlog_evaluation_global_properties` for each evaluation of a repeated project
3. Run the tool the evidence selects:
   - `binlog_evaluation_properties` when an evaluated value is in question
   - `binlog_imports` for the import chain and each missing import
   - `binlog_items` for the count and content of one item type
   - `binlog_search_files` for the glob or property function declaration in the embedded sources
   - `-pp:` on the project file when the whole expansion is necessary

Change a glob, an import, or a property function only when the measured evaluation cost names it.

GLOBS:
- The SDK includes `**/*.cs` when `EnableDefaultItems` and `EnableDefaultCompileItems` are `true`, minus `DefaultItemExcludes` and `DefaultExcludesInProjectFolder`, and the same pattern applies to `None` and `EmbeddedResource`
- `DefaultItemExcludes` holds `$(BaseOutputPath)/**`, `$(BaseIntermediateOutputPath)/**`, `**/*.user`, and the project and solution file patterns, and a large directory appends to it
- For a custom `Include`, put `Exclude` on the same element
- Disable a default item type only when the project declares every item of that type

```xml
<PropertyGroup>
  <!-- Large tree inside the project directory that no item type reads -->
  <DefaultItemExcludes>$(DefaultItemExcludes);fixtures/**</DefaultItemExcludes>
</PropertyGroup>
```

MULTIPLE EVALUATIONS:
- The restore pass, the outer build, and each inner build of a multi-targeting project are expected evaluations
- Investigate only a difference in the global-property sets that the requested build does not need

PROPERTY FUNCTIONS:
- Property functions inside a property or item expression run on every evaluation, including design-time builds and `-getProperty` queries
- Keep evaluation expressions deterministic and free of file reads, and use `dotnet-msbuild-antipatterns` for the correction

## [02]-[INCREMENTALITY]

Use `dotnet-msbuild-execution` for the `Inputs`, `Outputs`, and `FileWrites` authoring rules, the workflow finds the target that breaks them.

### [02.1]-[BINLOG_DIAGNOSIS]

```bash
dotnet build Solution.slnx -tl:off -bl:<dir>/establish-{}.binlog   # establishes the outputs
dotnet build Solution.slnx -tl:off -bl:<dir>/no-change-{}.binlog   # the capture to analyze
```

Analyze the second binlog:
1. Run `binlog_incremental_analysis`, and read `targets` for each row with `skipped: false`, its `reason`, `triggerInputs`, and `staleOutputs`, then `incrementalCleanDeletions` for a file a skipped target had declared
2. Run `binlog_project_target_times` for each project the rows name
3. Keep the rows with a file path in `staleOutputs`, because a `staleOutputs` value that repeats the target name marks a target with no `Outputs`
4. Run `binlog_search` with `Building target "<name>" completely` for each unresolved target, because the message under it names the stale input or the missing output
5. Run `binlog_expensive_targets` to order the rebuilt targets by cost
6. Run `binlog_search_files` for the target declaration when its `Inputs` and `Outputs` are in question

- Targets without `Inputs` and `Outputs` run on every build and log no up-to-date reason
- `IncrementalClean` deletes a file that a prior build wrote and the current build did not record, and a file that vanishes on every second build belongs in `FileWrites` from an `ItemGroup` inside the target

### [02.2]-[COMPILATION]

`CoreCompile` declares its sources, references, and analyzers as `Inputs` and the assembly, reference assembly, and documentation file as `Outputs`, and a no-change build skips it.
- `Deterministic` is `true` by default in the SDK, and identical inputs produce an identical assembly
- `ProduceReferenceAssembly` is `true` by default, `Csc` writes `obj/<config>/<tfm>/refint/<name>.dll`, and `CopyRefAssembly` updates `ref/<name>.dll` only when the public surface changes. A change inside a method body recompiles the library and leaves every consumer's `CoreCompile` skipped.
- New public types rewrite the reference assembly and recompile every consumer, which `binlog_search_targets` on `CoreCompile` shows as `skipped: false` in each dependent

### [02.3]-[COMMON_DEFECTS]

- Output paths contain a timestamp, build number, or random value
- The target writes a file that `Outputs` does not declare
- The declared inputs omit a file that affects the output
- Changed properties alter a declared input or output path
- Tasks rewrite unchanged output content and change its timestamp, and `WriteLinesToFile` needs `WriteOnlyWhenDifferent="true"`
