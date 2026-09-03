# [REWRITES]

BEFORE is the text as found, AFTER the rewrite, WRONG the rewrite that looks right and fails, and KEEP text that stays.

## [01]-[COINED_TERM]

- BEFORE: `The payload rides in the runtime pack and lands in the consumer's output`
- AFTER: `The runtime pack holds the native library and the build copies it to the consumer output`
- WRONG: `The asset travels in the runtime pack and arrives in the consumer output` (one image swapped for another)

Identifier and file renames update every reference:

```xml
<!-- BEFORE -->
<_NativeAssetPayload Include="@(StagedFile)" Anchor="$(PackageId)" DirectDelivery="build" />

<!-- AFTER -->
<_NativeAssetFile Include="@(StagedFile)" ManagedPackage="$(PackageId)" SkipDirectConsumers="true" />
```

## [02]-[CONNECTIVE_DELETION]

- BEFORE: `The SDK sets the property after the project body, so a project-level assignment is lost`
- AFTER: `The SDK sets the property after the project body, a project-level assignment is lost`
- WRONG: `Project-level assignments are lost because the SDK sets the property after the project body` (a reorder where the deletion suffices)
- WRONG: `The SDK sets the property after the project body. Therefore a project-level assignment is lost` (the connective survived as a new sentence)

- BEFORE: `It returns counts plus the top hotspots per dimension`
- AFTER: `It returns counts with the top hotspots per dimension`

## [03]-[SUCH_AS_AND_WHOSE]

- BEFORE: `Effects such as IO and database calls run at the boundary`
- AFTER: `Effects (IO, database calls) run at the boundary`

- BEFORE: `Types whose Map obeys the functor laws are functors`
- AFTER: `Types with a lawful Map are functors`

- BEFORE: `Union whose cases are a value and its absence`
- AFTER: `Union of a value and its absence`

## [04]-[ENUMERATION_AND_COUNTS]

- BEFORE: `Several fixtures and a number of markers cover the two cases`
- AFTER: `Fixtures and markers cover both cases`

- BEFORE: `The three tables in this section list the banned words`
- AFTER: `The tables list the banned words`

- KEEP: `at least 2 nameservers`, `the first invocation`, `exactly one entry`, `one normalized path per package` (real values)

## [05]-[ARTICLE_LED_SENTENCE]

- BEFORE: `A recursive path calling itself with no base case loops forever`
- AFTER: `Recursive paths with no base case loop forever`
- WRONG: `The recursive path calling itself with no base case loops forever` (the article swapped, the structure kept)

- BEFORE: `` An `Inputs` or `Outputs` expression that evaluates to empty skips the target ``
- AFTER: `` `Inputs` or `Outputs` expressions that evaluate to empty skip the target ``

- BEFORE: `| A composite audit per project over seven dimensions |`
- AFTER: `| Composite audit per project over seven dimensions |`

## [06]-[SENTENCE_INTENT]

- BEFORE: `<tool> creates the <dir> directory at the root. This directory cannot be relocated. The gitignore excludes it. Reports still go to the configured artifact directory.`
- AFTER: `<tool> creates <dir> at the root with no relocation option, .gitignore excludes it, and reports still go to the configured artifact directory` (one fact, the exception around the directory)
- WRONG: `<tool> creates <dir>. It is fixed. Gitignore excludes it. Reports go to artifacts.` (fragments, dropped articles, dropped facts)

- BEFORE: `Language-specific idioms may differ, but the composition rules do not; define a result type when a language has no suitable one instead of introducing another error mechanism`
- AFTER: `Language idioms differ but the composition rules do not, and a language without a suitable result type defines one instead of adding another error mechanism`

- BEFORE: `Statements, list items, and table cells open with the subject, instructions open with the verb, a subject that needs "a" or "an" is pluralized or takes "the", and a run of items with one article and noun opener restructures around the verb or the category noun` (four rules in one list item)
- AFTER: `Statements, list items, and table cells open with the noun that names their subject, and instructions open with the verb` / `A generic singular subject becomes the plural or the instruction's verb` / `A run of list items with one noun opener restructures around the verb or the category noun` (one rule per item)

## [07]-[OVERLAP_AND_NO_OP]

- BEFORE: `Returns the result, which is the value the computation produced, as the return value of the call`
- AFTER: `Returns the computed value`

- BEFORE: `Every dataset open fails until <method> registers the drivers, and no dataset opens before registration`
- AFTER: `Every dataset open fails until <method> registers the drivers`

- BEFORE: `The build runs the target. The target is part of the build and runs during it. It copies the assets.`
- AFTER: `The build runs the target, which copies the assets`

## [08]-[NEAR_DUPLICATE_ACROSS_SECTIONS]

- BEFORE (layout): `Root <manifest> owns resolution and the single lock file`
- BEFORE (dependencies): `Every dependency resolves through the root <manifest>, and the lock file at the root is the only one`
- AFTER (dependencies): `Root <manifest> owns resolution, dependency groups, and the lock file`
- AFTER (layout): (deleted)

## [09]-[COMMENTS]

```python
# BEFORE
# Test support routes the <tool> database to .cache/<tool>; this catches
# runs that were started without that configuration in place.

# AFTER
# Catches <tool> databases from runs started before the .cache/<tool> path was configured
```

```csharp
// BEFORE
// <host> writes lock and backup files beside opened <ext> files.
// The host exposes no relocation option.

// AFTER
// <host> lock and backup files beside opened <ext> files, not relocatable
```

```xml
<!-- BEFORE: pin manifest for the shared version guard; stops repo build inheritance -->

<!-- AFTER: Version manifest read by the central package version check (the inheritance claim was wrong) -->
```

```csharp
/// <summary>Returns the required module names absent from the loaded native build</summary>
/// <param name="requiredModules">Module names the caller depends on</param>
```

## [10]-[MESSAGES]

- BEFORE: `"<library> native runtime failed to load; reference the <library> native runtime package for this RID and restart, the CLR caches the failed type initializer"`
- AFTER: `"<library> native runtime failed to load. The CLR caches the failed type initializer, reference the runtime package for this RID and restart"`

- BEFORE: `Expected {0}; got {1}.`
- AFTER: `Expected {0}, got {1}`

## [11]-[TABLE_CELLS]

- BEFORE: `` | Python `network`/`subprocess`; language-specific integration run. | ``
- AFTER: `` | Python `network`/`subprocess` markers, per-language integration run | ``

- BEFORE: `| Roslyn analyzers detecting common correctness and performance coding issues. |`
- AFTER: `| Correctness and performance analyzers |`

## [12]-[HEADINGS_AND_SELF_STATEMENT]

- BEFORE: `## [03]-[STANDING_LAW] (safety rules for irreversible acts)`
- AFTER: `## [03]-[SAFETY_RULES]`

- BEFORE: `` In <library>, `<type>` represents absence `` (page already titled after the library)
- AFTER: `` `<type>` represents absence ``

- BEFORE: `This file documents the collection types and how shared state works`
- AFTER: (deleted, the heading names the topic)

## [13]-[FACT_PRESERVATION]

- KEEP: `The request may have failed because the client version is outdated`
- WRONG: `The request failed because the client version is outdated` (a suspicion became a fact)
- WRONG: `If the client version is outdated, the request can fail` (a report on one request became a general statement)

- BEFORE: `GPU acceleration for <model> inference` (comment on a CPU-only runtime package)
- AFTER: `Default CPU inference runtime binaries for every platform` (longer, because the fact was wrong)

## [14]-[NARRATION_TO_INSTRUCTION]

- BEFORE: `Committed binaries, because the pipeline rebuilds every artifact from a pinned manifest` (an item in a list of what a directory excludes)
- AFTER: `The pipeline rebuilds every binary from a pinned manifest` (the fact, under the pipeline's heading)
- WRONG: `Never commit binaries, the pipeline rebuilds them` (the forbidden form leads)
- WRONG: `Every binary belongs to the pipeline, which rebuilds it from a pinned manifest` (an ownership statement in place of the fact)

- BEFORE: `The script finds the repository root as the nearest ancestor directory holding the root lock file, never the working directory or an environment variable`
- AFTER: `Find the repository root as the nearest ancestor directory holding the root lock file`

- BEFORE: `The target sets cache: false and parallelism: false, provisioning mutates shared directories`
- AFTER: `Set cache: false and parallelism: false on the target, because provisioning mutates shared directories`
- KEEP: the reason, the reader needs it to decide the same for the next target that mutates a shared directory

## [15]-[CONTEXT_SUPPLIED_SUBJECT]

- BEFORE: `` `dotnet-roslyn-codelens` owns trust `` (a step in a procedure)
- AFTER: `` Use `dotnet-roslyn-codelens` to trust the solution ``

- BEFORE: `` See `dotnet-msbuild-packaging` for the pack items, the `PackagePath` metadata, and the deterministic pack properties ``
- AFTER: `` Use `dotnet-msbuild-packaging` for the package layout `` (the contents stay in the skill)

- BEFORE: `# Nx plugin that infers the packaging projects, their stage and pack targets, and their edges` (a tree comment)
- AFTER: `# Nx plugin that adds the native packaging projects to the task graph`

- BEFORE: `` `get_diagnostics` with `includeAnalyzers=true` once. That result is the baseline. ``
- AFTER: `` `get_diagnostics` with `includeAnalyzers=true` once, as the baseline ``

- BEFORE: `` Condition these properties in `Directory.Build.targets` `` (a rule under an entry that names the properties)
- AFTER: `` Condition the properties in `Directory.Build.targets` ``
