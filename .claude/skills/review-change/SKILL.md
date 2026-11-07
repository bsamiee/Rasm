---
name: review-change
description: "Use when judging, challenging, or codifying a change to code, covering diff reading, owner routing per fact and checker, verification, adversary rounds, and the principles and checks a change leaves behind"
---

# [REVIEW_CHANGE]

Reviews of a change prove each hunk against its file whole, its consumers, package source at pinned release, and repository checks, then leave each settled fact in the skill or checker owning it.

Arguments name a subject and a stage, a missing stage means `judge`:

| [INDEX] | [SUBJECT] | [EXAMPLE]                                  | [SPLIT]                                                 |
| :-----: | :-------- | :----------------------------------------- | :------------------------------------------------------ |
|  [01]   | File      | One source file and its call sites         | One reader, consumers read whole                        |
|  [02]   | Folder    | Package, crate, or module family           | `deepen` workflow, one reader and editor per file group |
|  [03]   | Range     | `<base>..HEAD` of one language             | `tune` workflow, one reader per unit of the diff        |
|  [04]   | Guidance  | Modified lines of one skill or rule folder | One adversary over the diff alone                       |

| [INDEX] | [STAGE]     | [RESULT]                                                                        |
| :-----: | :---------- | :------------------------------------------------------------------------------ |
|  [01]   | `judge`     | Each hunk kept, reverted, or rebuilt, counts before and after, verification run |
|  [02]   | `challenge` | Adversary rounds over the same scope, edits in place, verdict hunk by hunk      |
|  [03]   | `codify`    | Principles in their language skill, checks at their owner, prose checked        |

## [01]-[READING]

Read before judging. A verdict drafted from a diff alone, a search over one file, or a summary beside the code is rewritten or dropped later:
1. Run `git log -5 --stat` and `git status --short` for work in progress, then read the live tree, uncommitted edits included
2. Read `CLAUDE.md` `[04]-[CODE_QUALITY]`, the language skill, its `.claude/workflows/` companion, memory entries, and cited `docs/research` sheets
3. Choose what to read with `ast-grep outline <path>`, then read a file judged line by line whole, with no outline standing in for the read
4. Read every consumer: outline `--items exports` names exports, `rg -F '<name>'` and `ast-grep run` find call sites, each followed to its effect
5. Read the diff as syntax, `GIT_EXTERNAL_DIFF=difft git diff <base>` separates reshaped from moved code, `git diff --numstat <base>` counts lines
6. Read each dependency member a hunk calls or replaces through `search-code`, source at the pinned release decides what hand code it replaces
7. Read `git log -S '<literal>'` for why a form exists, a flag or guard arriving with a file serves it until its cause is read
8. Run each check the subject owns (`nx run <project>:check`, `nx run rasm:lint:<tool> -- <paths>`) before any edit, findings at base belong to base

Measurements are counts a reader reproduces: lines, declarations, exports, and nesting depth per file from `loc` and outline, timings from `hyperfine` on the real command, each stated before and after.

## [02]-[ROUTING]

Each fact a review settles has one owner, read before a review states it and edited by a review changing it, `manage-repo` `[04]-[CHECKS]` orders owners and `.claude/workflows/languages.js` lists them per language:

| [INDEX] | [FACT]                                                | [OWNER]                                                               |
| :-----: | :---------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | Level, option, threshold, or ban of an included rule  | Checker configuration file `languages.js` names per checker           |
|  [02]   | Form wrong whatever its types, in any parsed language | `tools/ast-grep/rules/<language>`, shared forms in `utils/<language>` |
|  [03]   | Form resolved names, types, or paths decide           | `tools/dylint` (Rust), `tools/roslyn` (C#), `tools/refurb` (Python)   |
|  [04]   | Type-resolved form in TypeScript                      | `tsconfig.base.json` row, an `@effect/language-service` diagnostic    |
|  [05]   | Banned API symbol                                     | `tools/roslyn/BannedSymbols*.txt`                                     |
|  [06]   | Fact no checker decides soundly for every instance    | Language skill, in the section with a heading covering it             |
|  [07]   | Placement, target, or entry point of a check          | `manage-repo` and its references                                      |

Routing tests:
- Category over instance: a rule states a defect form code showed, widened to its category, a rule for one file or one session is noise
- One owner per site: a site more than one checker reports takes the owner proving it at least cost, each other excludes it by one shared predicate
- Configuration before rule, rule before compiler extension: a checker row is edited before a rule, a rule before a `tools/` checker
- No weakening for speed: a slow check is timed by stage and fixed at its cause (grammar, regex class, rule shape, util, file walk), capability stays
- No bending: a lowered level, a `files` exclusion, or a suppression added so one change passes is a regression the review reverts
- Causes outside the tree (a machine setting, a host build) are named with their owner, a workaround inside the tree is a hidden cause

## [03]-[JUDGING]

Hunks are judged by what they remove or add, one doing neither is a shuffle and is reverted or finished:

| [INDEX] | [REGRESSION]    | [TEST]                                                                                 |
| :-----: | :-------------- | :------------------------------------------------------------------------------------- |
|  [01]   | Shuffle         | Move, split, rename, alias, forwarder, or extracted helper holding the same logic      |
|  [02]   | New shape       | Struct, enum, type, constant, or string added where a package or declaration states it |
|  [03]   | Hidden cause    | Wrapper, fallback, guard, retry, clamp, or lock standing where a cause needs a fix     |
|  [04]   | Lost capability | Input range, grammar, rule, or output the old code served and the new does not         |
|  [05]   | Growth          | Lines, declarations, or exports above base with no capability named for them           |
|  [06]   | Hand-rolled     | Logic a dependency member or host at its installed release performs                    |
|  [07]   | String-typed    | String or string union deciding behavior where a typed handle exists                   |
|  [08]   | Enumerated      | Member, arm, or branch per subject or mode a parameter or table derives                |
|  [09]   | Self-check      | Code proving its own code, config, or output                                           |
|  [10]   | Coupled         | Library code naming a product, host, test, or one caller's need                        |

Improvements are the inverse: a shape removed, a value derived, a member replacing hand code, one parameterized form replacing a family, nesting flattened by a match or table, a cost cut by better logic. Use `refactor-code` for the rework itself and `<language>-coding` skills for language form, reviews judge the result.

Verification is the `refactor-code` `[06]` set, run once before a verdict with its output entering the report, and a review adds:
- Timing of the real command with `hyperfine` where a change claims speed, before and after, a cost claim without one is `Unverified:`
- Old output as evidence alone: a difference on crafted input is judged by the change's intent, a gate reverting an improvement is a regression

## [04]-[CHALLENGE]

Adversaries over code run as `refactor-code` `[05]` states, agents with no prior context over the same files and standards, and reviews judge what they leave:
- Verdicts read the diff against base hunk by hunk, growth naming no capability is reverted
- Guidance an adversary edits takes an adversary of its own over its modified lines alone, judged against code in the tree and package source

`deepen` (`args.mode` `reduce` or `deepen`) runs readers, editors, adversaries, and a verdict per file group, `tune` runs the codifying stage over a committed range, and a scope smaller than a folder runs each role as a delegated agent.

## [05]-[CODIFYING]

Facts a change settled leave it as principles and checks, mined after the verdict:
1. Readers mine the diff, tool calls, and adversary edits for principles, each naming the form replaced, the stronger form, its category, and its test
2. One combiner adjudicates: overlaps merge under the stronger statement, complements integrate, contradictions resolve by source, cost, or a live run
3. Principles join the language skill at the section owning their category: a line replaced or added, a weak line deleted, a snippet rewritten whole
4. Each principle routes through `[02]` to its checker, widening an existing rule or util before adding one, a tightening row violated nowhere
5. Checks prove once: a scan of a scratch file of violating forms and near misses, then the project at zero findings, timed before and after
6. Adversaries read the modified guidance and checks alone, then a clean-prose pass over the same lines ends with `prose.py check`

Principles hold for any file of the language: placeholder names, no path, session, run, or count of one pass, no term the field lacks, a move bound to one file is none. A wrong line goes with every mention and no warning in its place, an exception one instance needs takes no line, and an appended restatement is a regression. Use `use-ast-grep` for rule shape and cost.

## [06]-[DELEGATION]

Delegate when a subject spans more than one file group or more than one language owner. A lead keeps the merge and spawns each round in one message, disjoint scopes at once and scopes sharing a file in order, never one agent over the whole.

Each brief states:
- Scope: files owned, files read for consumers, files excluded
- Standards: `CLAUDE.md` sections and skills judging the scope, memory entries, parked areas
- Facts: the `refactor-code` `[05]` brief contents, with research on disk read and not redone and decisions changing on a new fact alone
- Rules: fix in scope in place with nothing deferred, delete before add, verify before return, no report file, no copy or backup of a file
- Return: files edited, counts before and after, changes reaching other files, `Unverified:` claims

Lead steps after every agent returns:
1. Wait for completion notices, polling an agent's output reads partial results
2. Merge findings under their file, keeping strongest evidence
3. Rerun the deciding check on a conflicting verdict
4. Read each top change against live text before it enters the report

## [07]-[REPORT]

Reports take the `refactor-code` report form per change, then principles with their owner file and section, checks with their id and hits, and `Unverified:` claims with the command settling them.

Reports leave out:
- Moves bound to one file, one session, or one run
- Guidance restating a line a skill holds
- Checks for forms code never showed
- Speed claims without a timing
- Check output beyond one run per stage
- Tests, unless the owner asked for them
