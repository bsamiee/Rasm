# [EVALUATION_AND_INCREMENTALITY]

Use the evaluation workflow for evaluation cost. Use the incrementality workflow when a later build executes unexpected work.

## [01]-[EVALUATION]

MSBuild evaluates a project before it executes targets.

### [01.1]-[BINLOG_DIAGNOSIS]

1. Run `binlog_evaluations` to identify costly project evaluations.
2. Run `binlog_evaluation_global_properties` for each suspect evaluation.
3. Run `binlog_evaluation_properties` when an evaluated property value is relevant.
4. Run `binlog_imports` for the import chain.
5. Run `binlog_items` to examine evaluated item counts and contents.
6. Run `binlog_search_files` for glob and property-function declarations.
7. Run `binlog_preprocess` only when the complete imported source is necessary.

GLOBS:
- Broad recursive globs can enumerate large directory trees.
- For SDK default items, add large excluded directories to `DefaultItemExcludes`.
- For a custom `Include`, put `Exclude` on the same item element.
- Disable a default item type only when the project supplies every required item of that type.
- Change a glob only when its evaluated items and measured cost identify it as a problem.

IMPORTS:
- Change an import only when measured evaluation cost identifies it.
- Preserve the required property, item, task, and target order.

MULTIPLE EVALUATIONS:
- Each unique project and global-property combination can require a separate evaluation.
- Outer builds, inner builds, and restore can create expected evaluations.
- Investigate only unexpected differences in global-property sets.
- Remove an evaluation only when its project instance is not required for the requested build.

PROPERTY FUNCTIONS:
- A property function in an evaluated property or item expression runs during evaluation.
- Change a file-system query or costly expression only when measured evaluation cost identifies it.
- Keep evaluation expressions deterministic and free of side effects.

## [02]-[INCREMENTALITY]

The `dotnet-msbuild-execution` skill owns the `Inputs`, `Outputs`, and `FileWrites` authoring rules. This workflow finds the target that breaks them.

### [02.1]-[BINLOG_DIAGNOSIS]

```bash
dotnet build -bl:prime-incremental-{}  # establish the outputs
dotnet build -bl:incremental-{}        # capture the no-change build
```

Analyze the second binlog:
1. Run `binlog_incremental_analysis` for target decisions, triggering files, and `IncrementalClean` deletions.
2. Run `binlog_project_target_times` for each suspect project.
3. Keep targets with `skipped: false` whose `staleOutputs` names a file path. A `staleOutputs` value that repeats the target name marks a target with no file outputs, which runs on every build by design.
4. Run `binlog_search` with `Building target "<name>" completely` for each unresolved target. The `Chain:` line of `binlog_target_reasons` reports a stale skip state and omits the reason.
5. Run `binlog_expensive_targets` to prioritize costly rebuilt targets.
6. Run `binlog_preprocess` only when the imported target declaration is necessary.

`Building target "X" completely` and its reason identify the stale input or missing output. A target without an up-to-date reason or a `Skipped:` line has no declared `Inputs` and `Outputs`.

### [02.2]-[COMMON_DEFECTS]

1. An output path contains a timestamp, build number, or random value.
2. The target writes a file that `Outputs` does not declare.
3. The declared inputs omit a file that affects the output.
4. A changed property alters a declared input or output path.
5. A task rewrites unchanged output content and changes its timestamp.
