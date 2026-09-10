# [EVALUATION_AND_INCREMENTALITY]

Evaluation time, projects with repeated evaluations, and the work a no-change build still executes.

## [01]-[EVALUATION]

MSBuild evaluates a project before it executes targets, once per project instance, walking every import, property, item glob, and property function in order.

### [01.1]-[MEASUREMENT]

```bash
dotnet build <project> -profileEvaluation:<dir>/evaluation-{}.md
dotnet build <project> -v:diag | rg 'Property reassignment'
```

- `-profileEvaluation` prints one row per import, property, item, and target
- `-profileEvaluation` reports inclusive and exclusive time grouped by evaluation pass, the glob rows include the directory enumeration cost
- SDK files produce most `Property reassignment:` lines, the lines naming a file under the repository matter

### [01.2]-[BINLOG_DIAGNOSIS]

1. Run `binlog_evaluations` for the slow or repeated evaluations
2. Run `binlog_evaluation_global_properties` for each evaluation of a repeated project
3. Run the tool the evidence selects:
   - `binlog_evaluation_properties` when an evaluated value is in question
   - `binlog_imports` for the import chain and each missing import
   - `binlog_items` for the count and content of one item type
   - `binlog_search_files` for the glob or property function declaration in the embedded sources
   - `-pp:` on the project file when the whole expansion is necessary

Change a glob, an import, or a property function when the measured evaluation cost names it.

[GLOBS]:
- `EnableDefaultItems` and `EnableDefaultCompileItems` include `**/*.cs` minus `DefaultItemExcludes` and `DefaultExcludesInProjectFolder`
- `None` and `EmbeddedResource` follow the same pattern
- `DefaultItemExcludes` holds `$(BaseOutputPath)/**`, `$(BaseIntermediateOutputPath)/**`, `**/*.user`, and the project and solution file patterns
- A large directory no item type reads appends to `DefaultItemExcludes`
- A custom `Include` takes `Exclude` on the same element
- Disable a default item type when the project declares every item of that type

```xml
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);fixtures/**</DefaultItemExcludes>
</PropertyGroup>
```

[REPEATED_EVALUATIONS]:
- The restore pass, the outer build, and each inner build of a multi-targeting project are expected evaluations
- A difference in the global-property sets the requested build does not need is the finding

[PROPERTY_FUNCTIONS]:
- Property functions inside a property or item expression run on every evaluation, design-time builds and `-getProperty` queries included
- Evaluation expressions stay deterministic and free of file reads

## [02]-[INCREMENTALITY]

The workflow finds the target that breaks incremental rules. Use `dotnet-msbuild-execution` for `Inputs`, `Outputs`, and `FileWrites`.

### [02.1]-[BINLOG_DIAGNOSIS]

```bash
dotnet restore Solution.slnx --artifacts-path <dir>/artifacts
dotnet build Solution.slnx --no-restore --artifacts-path <dir>/artifacts -bl:<dir>/establish-{}.binlog
dotnet build Solution.slnx --no-restore --artifacts-path <dir>/artifacts -bl:<dir>/no-change-{}.binlog
```

Analyze the second binlog:
1. Run `binlog_incremental_analysis`, read each `targets` row with `skipped: false` for its `reason`, `triggerInputs`, and `staleOutputs`
2. Read `incrementalCleanDeletions` for a file a skipped target had declared
3. Run `binlog_project_target_times` for each project the rows name
4. Keep the rows with a file path in `staleOutputs`, a `staleOutputs` value repeating the target name marks a target with no `Outputs`
5. Run `binlog_search` with `under($project <name>) $target <target>` per unresolved target, its message names the stale input or missing output
6. Run `binlog_expensive_targets` to order the rebuilt targets by cost
7. Run `binlog_search_files` for the target declaration when its `Inputs` and `Outputs` are in question

- A target without `Inputs` and `Outputs` runs on every build and logs no up-to-date reason
- A file in `incrementalCleanDeletions` vanishes on every second build

### [02.2]-[COMPILATION]

`CoreCompile` declares its sources, references, and analyzers as `Inputs` and the assembly, reference assembly, and documentation file as `Outputs`, a no-change build skips it.
- `Deterministic` is `true` by default, identical inputs produce an identical assembly
- `ProduceReferenceAssembly` is `true` by default, `Csc` writes `obj/<config>/<tfm>/refint/<name>.dll`
- `CopyRefAssembly` updates `ref/<name>.dll` when the public surface changes
- A change inside a method body recompiles the library and leaves every consumer's `CoreCompile` skipped
- A new public type rewrites the reference assembly and recompiles every consumer, `CoreCompile` shows `skipped: false` in each dependent

### [02.3]-[COMMON_DEFECTS]

- Output paths contain a timestamp, build number, or random value
- The target writes a file `Outputs` does not declare
- The declared inputs omit a file that affects the output
- Changed properties alter a declared input or output path
- Tasks rewrite unchanged output content and change its timestamp
