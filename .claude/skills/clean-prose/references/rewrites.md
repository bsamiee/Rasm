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
<_NativeFile Include="@(StagedFile)" Package="$(PackageId)" SkipDirectConsumers="true" />
```

## [02]-[CONNECTIVE_DELETION]

- BEFORE: `The SDK sets the property after the project body, so a project-level assignment is lost`
- AFTER: `The SDK sets the property after the project body, a project-level assignment is lost`
- WRONG: `Project-level assignments are lost because the SDK sets the property after the project body` (a reorder where the deletion suffices)
- WRONG: `The SDK sets the property after the project body. Therefore a project-level assignment is lost` (the connective survived as a new sentence)

- BEFORE: `The chain becomes an ordered table of pairs, where the first matching predicate wins`
- AFTER: `The chain becomes an ordered table of pairs, the first matching predicate wins`
- WRONG: `The chain becomes an ordered table of pairs, while the first matching predicate wins` (a connective swapped for another)

- BEFORE: `It returns counts plus the top hotspots per dimension`
- AFTER: `It returns counts with the top hotspots per dimension`

- BEFORE: `Effects such as IO and database calls run at the boundary`
- AFTER: `Effects (IO, database calls) run at the boundary`

- BEFORE: `Types whose Map obeys the functor laws are functors`
- AFTER: `Types with a lawful Map are functors`

- BEFORE: `Union whose cases are a value and its absence`
- AFTER: `Union of a value and its absence`

- BEFORE: `Several fixtures and a number of markers cover the two cases`
- AFTER: `Fixtures and markers cover both cases`

- BEFORE: `The three tables in this section list the banned words`
- AFTER: `The tables list the banned words`

- KEEP: `Every <package> member routes through the lookup, and a boundary between packages names both sides`
- WRONG: `Every <package> member, a boundary between packages names both sides` (the verb went with the deletion, the subject dangles)

## [03]-[RESTATEMENT]

- BEFORE: `Returns the result, which is the value the computation produced, as the return value of the call`
- AFTER: `Returns the computed value`

- BEFORE: `Every dataset open fails until <method> registers the drivers, and no dataset opens before registration`
- AFTER: `Every dataset open fails until <method> registers the drivers`

- BEFORE: `The build runs the target. The target is part of the build and runs during it. It copies the assets.`
- AFTER: `The build runs the target, which copies the assets`

- BEFORE (layout): `Root <manifest> owns resolution and the single lock file`
- BEFORE (dependencies): `Every dependency resolves through the root <manifest>, and the lock file at the root is the only one`
- AFTER (dependencies): `Every dependency resolves through the root <manifest>, and the root holds the one lock file`
- AFTER (layout): (deleted)

- BEFORE: `This file documents the collection types and how shared state works`
- AFTER: (deleted, the heading names the topic)

## [04]-[RUN_FACTS]

- BEFORE: `Step 7 names the files in scope, because the expanded outline over <dir> prints over 2,000 lines against under 30 for a policy file with its adapter`
- AFTER: `Step 7 names the files in scope, the whole-folder outline prints every module`

- BEFORE: `Draw-path hooks pay the module's warm-up on their first frame near 70 ms, later frames settle between 0.5 and 25 ms, one <line> per frame`
- AFTER: `Draw-path hooks pay the module's warm-up on their first frame, and the gate reads the later frames, one <line> per frame`

- BEFORE: `Facts of <server> 2.18.0 the arm works under and of <host> 2.1.263 the declarations work under, each issue with the release change that retires it`
- AFTER: `Defects of the installed <server> and <host> releases the arm works under, each with the release change that retires it`

- KEEP: `` `<method>()` answers 4096 rows at most `` (a declared limit), `` `timeout: 600000` `` (a value the hook sets), `at least 2 nameservers` (a real value)

## [05]-[FACT_PRESERVATION]

- KEEP: `The request may have failed on an outdated client version`
- WRONG: `The request failed on an outdated client version` (a suspicion became a fact)
- WRONG: `If the client version is outdated, the request can fail` (a report on one request became a general statement)

- BEFORE: `GPU acceleration for <model> inference` (comment on a CPU-only runtime package)
- AFTER: `Default CPU inference runtime binaries for every platform` (longer, the fact was wrong)

## [06]-[JOINED_SENTENCES]

- BEFORE: `<tool> creates the <dir> directory at the root. This directory cannot be relocated. The gitignore excludes it. Reports still go to the configured artifact directory.`
- AFTER: `<tool> creates <dir> at the root with no relocation option, .gitignore excludes it, and reports still go to the configured artifact directory` (one fact, the exception around the directory)
- WRONG: `<tool> creates <dir>. It is fixed. Gitignore excludes it. Reports go to artifacts.` (fragments, dropped facts)
- WRONG: `<tool> creates <dir>, it cannot be relocated, .gitignore excludes it, and reports go to the artifact directory` (joined, intent unstated)

- BEFORE: `Language-specific idioms may differ, but the composition rules do not; define a result type when a language has no suitable one instead of introducing another error mechanism`
- AFTER: `Language idioms differ but the composition rules do not. When a language lacks a result type, define one` (the forbidden alternative moves to an anti-pattern entry)

- BEFORE: `` `get_diagnostics` with `includeAnalyzers=true` once. That result is the baseline. ``
- AFTER: `` `get_diagnostics` with `includeAnalyzers=true` once, as the baseline ``

- BEFORE: `Statements, list items, and table cells open with the subject, instructions open with the verb, a subject that needs "a" or "an" is pluralized or takes "the", and a run of items with one article and noun opener restructures around the verb or the category noun` (four rules in one list item)
- AFTER: `Entries open with their subject noun, plural when generic, or with the verb of an instruction` (one rule, a run of one noun opener is that rule seen across items)
- WRONG: three items, `Statements, list items, and table cells open with the noun that names their subject`, `Generic singular subjects become the plural`, `Runs of list items with one noun opener restructure around the verb` (the sentence sheared into parts, each restating the rule)

- BEFORE: `` `<rule>` raises a foreground call with a slow leaf (`<a>`, `<b>`) to `timeout: 600000` silently when the call sets none or a smaller one, and runs `<target>` with `run_in_background: true` and a context line naming its completion notification ``
- AFTER: `` `<rule>` raises a foreground call with a slow leaf (`<a>`, `<b>`) to `timeout: 600000` with no context line when the call sets none or a smaller one. `<target>` runs with `run_in_background: true` and a context line naming its completion notification ``
- WRONG: `` `<rule>` raises slow calls to `timeout: 600000` and backgrounds `<target>` `` (the conditions and the context line went)

## [07]-[ARTICLES]

- BEFORE: `A recursive path calling itself with no base case loops forever`
- AFTER: `Recursive paths with no base case loop forever`
- WRONG: `The recursive path calling itself with no base case loops forever` (the article swapped, the structure kept)

- BEFORE: `` An `Inputs` or `Outputs` expression that evaluates to empty skips the target ``
- AFTER: `` `Inputs` or `Outputs` expressions that evaluate to empty skip the target ``

- BEFORE: `# NuGet sources and the package source mapping` (a tree comment)
- AFTER: `# NuGet sources and package source mapping`

- BEFORE: `` `libs/` and `tests/` hold the language-specific code ``
- AFTER: `` `libs/` and `tests/` hold language-specific code ``

- BEFORE: `# The instance, an effect under a condition with a None arm` (a test case comment)
- AFTER: `# Instance, an effect under a condition with a None arm` (the opener deleted, the rest as written)

- BEFORE: `` Condition these properties in `Directory.Build.targets` `` (a rule under an entry that names the properties)
- AFTER: `` Condition the properties in `Directory.Build.targets` ``

- BEFORE: `| A composite audit per project over seven dimensions: complexity, naming, unused symbols |`
- AFTER: `| Composite audit per project: complexity, naming, unused symbols |` (the count of a visible list goes with the article)

- BEFORE: `The composition, text, and host modules export the carriers, the text operations, and the boundary values the policies and events build on`
- AFTER: `Composition, text, and host modules export carriers, text operations, and boundary values that policies and events build on`
- WRONG: `Each composition, text, and host module exports its carriers, its text operations, and its boundary values` (the article swapped for a possessive)

- BEFORE: `Read in order before the first edit, with <agent> each file in scope and <dir> its agents directory`
- AFTER: `Read in order before the first edit, with <agent> each file in scope and <dir> its directory` (the second `the` was unique and stays)

- BEFORE: `` `<file>` and `<manifest>` whole, the layout, the store keys, the options, the events, the proof lines, and the known issues ``
- AFTER: `` `<file>` and `<manifest>` whole `` (the apposition restated the headings of the file)

- KEEP: `` `<check>`, the parse state of every file in the directory `` (one reading that decides the next step)

## [08]-[INSTRUCTIONS]

- BEFORE: `Committed binaries, because the pipeline rebuilds every artifact from a pinned manifest` (an item in a list of what a directory excludes)
- AFTER: `The pipeline rebuilds every binary from a pinned manifest` (the fact, under the pipeline's heading)
- WRONG: `Never commit binaries, the pipeline rebuilds them` (the forbidden form leads)
- WRONG: `Every binary belongs to the pipeline, which rebuilds it from a pinned manifest` (an ownership statement in place of the fact)

- BEFORE: `The script finds the repository root as the nearest ancestor directory holding the root lock file, never the working directory or an environment variable`
- AFTER: `Find the repository root as the nearest ancestor directory holding the root lock file`

- BEFORE: `The target sets cache: false and parallelism: false, provisioning mutates shared directories`
- AFTER: `When provisioning mutates shared directories, set cache: false and parallelism: false on the target`
- KEEP: the reason as the condition, a reader decides the same for the next target that mutates a shared directory
- WRONG: `Set cache: false and parallelism: false on the target, because provisioning mutates shared directories` (the reason bolted on after the instruction)

- BEFORE: `Confidence comes from tests, not from inspecting the implementation`
- AFTER: `Confidence comes from tests`
- WRONG: `Confidence comes from tests, inspecting the implementation proves nothing` (one negative swapped for another)

- BEFORE: `` Read a `.binlog` only through the `binlog` MCP tools, never directly ``
- AFTER: `` Read a `.binlog` through the `binlog` MCP tools `` (the direct read goes in the anti-pattern table)
- WRONG: `` Read a `.binlog` through the `binlog` MCP tools, not directly `` (the negative kept as a tail)

- KEEP: `` `AfterBuild` in a `.csproj` never runs `` (a fact about behavior)
- WRONG: `` `AfterBuild` in a `.csproj` does not run `` (a synonym swap of a fact)

- KEEP: `A consumer that only saves receives the save function` (a restriction that is the fact)
- WRONG: `A consumer that saves receives the save function` (the restriction went with the word, the fact changed)

- BEFORE: `Return the fix, never apply it` (an agent constraint)
- AFTER: `Return the fix for the caller to apply`
- WRONG: `Return the fix` (the constraint went with the word)

## [09]-[CONTEXT]

- BEFORE: `` `dotnet-roslyn-codelens` owns trust `` (a step in a procedure)
- AFTER: `` Use `dotnet-roslyn-codelens` to trust the solution ``

- BEFORE: `` See `dotnet-msbuild-packaging` for the pack items, the `PackagePath` metadata, and the deterministic pack properties ``
- AFTER: `` Use `dotnet-msbuild-packaging` for the package layout `` (the contents stay in the skill)

- BEFORE: `# Nx plugin that infers the packaging projects, their stage and pack targets, and their edges` (a tree comment)
- AFTER: `# Nx plugin that adds the native packaging projects to the task graph`

- BEFORE: `## [03]-[STANDING_LAW] (safety rules for irreversible acts)`
- AFTER: `## [03]-[SAFETY_RULES]`

- BEFORE: `` In <library>, `<type>` represents absence `` (page already titled after the library)
- AFTER: `` `<type>` represents absence ``

## [10]-[PARAPHRASED_CODE]

- BEFORE: `` `<script>` reads `<manifest>`, downloads the pinned archive, checks its hash, and extracts it under `.cache/` `` (a README line)
- AFTER: `` `<script>` places the pinned release archives under `.cache/` `` (the steps stay in the script)
- WRONG: `` `<script>` downloads and extracts the pinned archives `` (the steps shortened, the purpose still unstated)

## [11]-[TABLES]

- BEFORE: `` | `<module>` | Operations | `map`, `flatMap`, `getOrElse`, `toArray` | `` (a README table over every export)
- AFTER: (deleted) the layout line `` `<module>` # Option with its constructors and data-last operations `` names the file, and the outline prints the exports

- BEFORE: `` | Description of each file in scope | `[07]-[DESCRIPTION]` of the skill, graded by a fresh agent | `` (a role table row, with the grading as procedure step 9)
- AFTER: (deleted) the procedure step names the grading, and the role sentence names the skill

- BEFORE: `` | Whether the description matches the form | `sed -n 's/^description: //p' <agent> \| wc -w` | `` (a sources row, with the same command as a gate line)
- AFTER: (deleted) the gate line holds the command

- BEFORE: `| Roslyn analyzers detecting common correctness and performance coding issues. |`
- AFTER: `| Correctness and performance analyzers |`

- BEFORE: `` | Python `network`/`subprocess`; language-specific integration run. | ``
- AFTER: `` | Python `network`/`subprocess` markers, per-language integration run | ``

## [12]-[COMMENTS]

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

## [13]-[MESSAGES]

- BEFORE: `"<library> native runtime failed to load; reference the <library> native runtime package for this RID and restart, the CLR caches the failed type initializer"`
- AFTER: `"<library> native runtime failed to load. The CLR caches the failed type initializer. Reference the runtime package for this RID and restart"`

- BEFORE: `Expected {0}; got {1}.`
- AFTER: `Expected {0}, got {1}`
