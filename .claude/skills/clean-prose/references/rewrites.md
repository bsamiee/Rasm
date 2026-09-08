# [REWRITES]

BEFORE is text as found, AFTER rewrite, WRONG rewrite that looks right and fails, and KEEP text that stays.

## [01]-[COINED_TERM]

- BEFORE: `The payload rides in the runtime pack and lands in the consumer's output`
- AFTER: `Builds copy runtime pack's native library to consumer output`
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
- AFTER: `SDK targets overwrite property after project body`
- WRONG: `Project-level assignments are lost because the SDK sets the property after the project body` (reorder where deletion suffices)
- WRONG: `The SDK sets the property after the project body. Therefore a project-level assignment is lost` (connective survived as a new sentence)

- BEFORE: `The chain becomes an ordered table of pairs, where the first matching predicate wins`
- AFTER: `Chains become ordered tables of pairs, first matching predicate wins`
- WRONG: `The chain becomes an ordered table of pairs, while the first matching predicate wins` (connective swapped for another)

- BEFORE: `It returns counts plus the top hotspots per dimension`
- AFTER: `Returns counts with top hotspots per dimension`

- BEFORE: `Effects such as IO and database calls run at the boundary`
- AFTER: `Effects (IO, database calls) run at boundary`

- BEFORE: `Types whose Map obeys the functor laws are functors`
- AFTER: `Types with a lawful Map are functors`

- BEFORE: `Union whose cases are a value and its absence`
- AFTER: `Union of a value and its absence`

- BEFORE: `Several fixtures and a number of markers cover the two cases`
- AFTER: `Fixtures and markers cover cases`

- BEFORE: `The three tables in this section list the banned words`
- AFTER: `Tables list banned words`

- KEEP: `Every <package> member routes through the lookup, and boundaries between packages name both sides`
- WRONG: `Every <package> member, a boundary between packages names both sides` (verb went with deletion, subject dangles)

## [03]-[RESTATEMENT]

- BEFORE: `Returns the result, which is the value the computation produced, as the return value of the call`
- AFTER: `Returns computed value`

- BEFORE: `Every dataset open fails until <method> registers the drivers, and no dataset opens before registration`
- AFTER: `Every dataset open fails until <method> registers drivers`

- BEFORE: `The build runs the target. The target is part of the build and runs during it. It copies the assets.`
- AFTER: `` `<target>` copies assets during build ``
- WRONG: `The build runs the target, which copies the assets` (second fact hangs off noun as a relative clause)
- WRONG: `The build runs the target that copies the assets` (same clause with `that`)

- BEFORE (layout): `Root <manifest> owns resolution and the single lock file`
- BEFORE (dependencies): `Every dependency resolves through the root <manifest>, and the lock file at the root is the only one`
- AFTER (dependencies): `Every dependency resolves through root <manifest>. One lock file sits at root`
- AFTER (layout): (deleted)

- BEFORE: `This file documents the collection types and how shared state works`
- AFTER: (deleted, heading names topic)

## [04]-[RUN_FACTS]

- BEFORE: `Step 7 names the files in scope, because the expanded outline over <dir> prints over 2,000 lines against under 30 for a policy file with its adapter`
- AFTER: `Step 7 names files in scope. Expanded directory outlines print every module`

- BEFORE: `Draw-path hooks pay the module's warm-up on their first frame near 70 ms, later frames settle between 0.5 and 25 ms, one <line> per frame`
- AFTER: `Draw-path hooks pay module's warm-up on first frame, gate reads later frames, one <line> per frame`

- BEFORE: `Facts of <server> 2.18.0 the arm works under and of <host> 2.1.263 the declarations work under, each issue with the release change that retires it`
- AFTER: `Defects of <server> release arm works under and <host> release declarations work under, each with release change that retires it`

- KEEP: `` `<method>()` returns at most 4096 rows `` (declared limit), `` `timeout: 600000` `` (value hook sets), `at least 2 nameservers` (real value)

## [05]-[FACT_PRESERVATION]

- KEEP: `Request may have failed on an outdated client version`
- WRONG: `The request failed on an outdated client version` (suspicion became fact)

- BEFORE: `GPU acceleration for <model> inference` (comment on CPU-only runtime package)
- AFTER: `Default runtime binaries for CPU inference on every platform` (longer, fact was wrong)

## [06]-[JOINED_SENTENCES]

- BEFORE: `<tool> creates the <dir> directory at the root. This directory cannot be relocated. The gitignore excludes it. Reports still go to the configured artifact directory.`
- AFTER: `<tool> creates <dir> at root with no relocation option, .gitignore excludes it, reports go to configured artifact directory` (one fact, exception around directory)
- WRONG: `<tool> creates <dir>. It is fixed. Gitignore excludes it. Reports go to artifacts.` (fragments, dropped facts)
- WRONG: `<tool> creates <dir>, it cannot be relocated, .gitignore excludes it, and reports go to the artifact directory` (joined, intent unstated)

- BEFORE: `Language-specific idioms may differ, but the composition rules do not; define a result type when a language has no suitable one instead of introducing another error mechanism`
- AFTER: `Language idioms differ but composition rules do not. When a language lacks a result type, define one` (forbidden alternative moves to an anti-pattern entry)

- BEFORE: `` `get_diagnostics` with `includeAnalyzers=true` once. That result is the baseline. ``
- AFTER: `` `get_diagnostics` with `includeAnalyzers=true` once, as the baseline ``

- BEFORE: `Statements, list items, and table cells open with the subject, instructions open with the verb, a subject that needs "a" or "an" is pluralized or takes "the", and a run of items with one article and noun opener restructures around the verb or the category noun` (four rules in one list item)
- AFTER: `Statements and entries open with a subject noun, plural when generic, or with an instruction verb` (one rule, run of one noun opener is rule seen across items)
- WRONG: three items, `Statements, list items, and table cells open with the noun that names their subject`, `Generic singular subjects become the plural`, `Runs of list items with one noun opener restructure around the verb` (sentence sheared into parts, each restating rule)

- BEFORE: `` `<rule>` raises a foreground call with a slow leaf (`<a>`, `<b>`) to `timeout: 600000` silently when the call sets none or a smaller one, and runs `<target>` with `run_in_background: true` and a context line naming its completion notification ``
- AFTER: `` `<rule>` raises a foreground call with a slow leaf (`<a>`, `<b>`) to `timeout: 600000` with no context line when call sets none or a smaller one. `<rule>` runs `<target>` with `run_in_background: true` and a context line naming completion notification ``
- WRONG: `` `<rule>` raises slow calls to `timeout: 600000` and backgrounds `<target>` `` (conditions and context line went)

## [07]-[ARTICLES]

- BEFORE: `A recursive path calling itself with no base case loops forever`
- AFTER: `Recursive paths with no base case loop forever`
- WRONG: `The recursive path calling itself with no base case loops forever` (article swapped, structure kept)

- BEFORE: `` An `Inputs` or `Outputs` expression that evaluates to empty skips the target ``
- AFTER: `` `Inputs` or `Outputs` expressions that evaluate to empty skip target ``

- BEFORE: `# NuGet sources and the package source mapping` (tree comment)
- AFTER: `# NuGet sources and package source mapping`

- BEFORE: `` `libs/` and `tests/` hold the language-specific code ``
- AFTER: `` `libs/` and `tests/` hold language-specific code ``

- BEFORE: `# The instance, an effect under a condition with a None arm` (test case comment)
- AFTER: `# Instance, an effect under a condition with a None arm` (opener deleted, rest as written)

- BEFORE: `` Condition these properties in `Directory.Build.targets` `` (rule under an entry that names properties)
- AFTER: `` Condition properties in `Directory.Build.targets` ``

- BEFORE: `| A composite audit per project over seven dimensions: complexity, naming, unused symbols |`
- AFTER: `| Composite audit per project: complexity, naming, unused symbols |` (count of a visible list goes with article)

- BEFORE: `The composition, text, and host modules export the carriers, the text operations, and the boundary values the policies and events build on`
- AFTER: `Composition, text, and host modules export carriers, text operations, and boundary values that policies and events build on`
- WRONG: `Each composition, text, and host module exports its carriers, its text operations, and its boundary values` (article swapped for a possessive)

- BEFORE: `Read in order before the first edit, with <agent> each file in scope and <dir> its agents directory`
- AFTER: `Read in order before first edit, with <agent> each file in scope and <dir> its directory`

- BEFORE: `` `<file>` and `<manifest>` whole, the layout, the store keys, the options, the events, the proof lines, and the known issues ``
- AFTER: `` `<file>` and `<manifest>` whole `` (apposition restated headings of file)

- KEEP: `` `<check>`, parse state of every file in directory `` (one reading decides next step)

## [08]-[INSTRUCTIONS]

- BEFORE: `Committed binaries, because the pipeline rebuilds every artifact from a pinned manifest` (item in a list of what a directory excludes)
- AFTER: `Rebuilds every artifact from a pinned manifest` (fact, under pipeline's heading)
- WRONG: `Never commit binaries, the pipeline rebuilds them` (forbidden form leads)
- WRONG: `Every binary belongs to the pipeline, which rebuilds it from a pinned manifest` (ownership statement in place of fact)

- BEFORE: `The script finds the repository root as the nearest ancestor directory holding the root lock file, never the working directory or an environment variable`
- AFTER: `Find repository root as nearest ancestor directory with root lock file`

- BEFORE: `The target sets cache: false and parallelism: false, provisioning mutates shared directories`
- AFTER: `` When provisioning mutates shared directories, set `cache: false` and `parallelism: false` `` (reason as condition, reader decides same for next target that mutates a shared directory)
- WRONG: `Set cache: false and parallelism: false on the target, because provisioning mutates shared directories` (reason bolted on after instruction)

- BEFORE: `Confidence comes from tests, not from inspecting the implementation`
- AFTER: `Confidence comes from tests`
- WRONG: `Confidence comes from tests, inspecting the implementation proves nothing` (one negative swapped for another)

- BEFORE: `` Read a `.binlog` only through the `binlog` MCP tools, never directly ``
- AFTER: `` Read a `.binlog` through `binlog` MCP tools `` (direct read goes in anti-pattern table)
- WRONG: `` Read a `.binlog` through the `binlog` MCP tools, not directly `` (negative kept as tail)

- KEEP: `` `AfterBuild` in a `.csproj` never runs `` (fact about behavior)
- WRONG: `` `AfterBuild` in a `.csproj` does not run `` (synonym swap of fact)

- KEEP: `Save-only consumers receive save function` (restriction that is fact)
- WRONG: `A consumer that saves receives the save function` (restriction went with word, fact changed)

- BEFORE: `Return the fix, never apply it` (an agent constraint)
- AFTER: `Return fix unapplied`
- WRONG: `Return the fix` (constraint went with word)

## [09]-[CONTEXT]

- BEFORE: `` `dotnet-roslyn-codelens` owns trust `` (step in a procedure)
- AFTER: `` Use `dotnet-roslyn-codelens` for solution trust `` (longer, step was an ownership statement)

- BEFORE: `` See `dotnet-msbuild-packaging` for the pack items, the `PackagePath` metadata, and the deterministic pack properties ``
- AFTER: `` Use `dotnet-msbuild-packaging` for packing `` (contents stay in skill)

- BEFORE: `# Nx plugin that infers the packaging projects, their stage and pack targets, and their edges` (tree comment)
- AFTER: `# Nx plugin that adds packaging projects to task graph`

- BEFORE: `## [03]-[STANDING_LAW] (safety rules for irreversible acts)`
- AFTER: `## [03]-[SAFETY_RULES]` (scope, irreversible acts, opens section sentence)

- BEFORE: `` In <library>, `<type>` represents absence `` (page already titled after library)
- AFTER: `` `<type>` represents absence ``

## [10]-[PARAPHRASED_CODE]

- BEFORE: `` `<script>` reads `<manifest>`, downloads the pinned archive, checks its hash, and extracts it under `.cache/` `` (README line)
- AFTER: `` `<script>` places pinned archive under `.cache/` `` (steps stay in script)
- WRONG: `` `<script>` downloads and extracts the pinned archives `` (steps shortened, purpose still unstated)

## [11]-[TABLES]

- BEFORE: `` | `<module>` | Operations | `map`, `flatMap`, `getOrElse`, `toArray` | `` (README table over every export)
- AFTER: (deleted) layout line `` `<module>` # Option with constructors and data-last operations `` names file, outline prints exports

- BEFORE: `` | Description of each file in scope | `[07]-[DESCRIPTION]` of the skill, graded by a fresh agent | `` (role table row, with grading as procedure step 9)
- AFTER: (deleted) procedure step names grading, role sentence names skill

- BEFORE: `` | Whether the description matches the form | `sed -n 's/^description: //p' <agent> \| wc -w` | `` (sources row, same command as a gate line)
- AFTER: (deleted) gate line holds command

- BEFORE: `| Roslyn analyzers detecting common correctness and performance coding issues. |`
- AFTER: `| Correctness and performance analyzers |`

- BEFORE: `` | Python `network`/`subprocess`; language-specific integration run. | ``
- AFTER: `` | Python `network`/`subprocess` markers, per-language integration run | `` (longer, marker noun was missing)

## [12]-[COMMENTS]

```python
# BEFORE
# Test support routes the <tool> database to .cache/<tool>; this catches
# runs that were started without that configuration in place.

# AFTER
# Catches runs started without .cache/<tool> database path
```

```csharp
// BEFORE
// <host> writes lock and backup files beside opened <ext> files.
// The host exposes no relocation option.

// AFTER
// <host> lock and backup files beside opened <ext> files with no relocation option
```

```xml
<!-- BEFORE: pin manifest for the shared version guard; stops repo build inheritance -->

<!-- AFTER: Version manifest for checking central package versions (inheritance claim was wrong) -->
```

```csharp
/// <summary>Returns required module names absent from loaded native build</summary>
/// <param name="requiredModules">Module names caller depends on</param>
```

## [13]-[MESSAGES]

- BEFORE: `"<library> native runtime failed to load; reference the <library> native runtime package for this RID and restart, the CLR caches the failed type initializer"`
- AFTER: `"<library> native runtime failed to load. CLR caches failed type initializer. Reference runtime package for host RID and restart"`

- BEFORE: `Expected {0}; got {1}.`
- AFTER: `Expected {0}, got {1}`
