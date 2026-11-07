---
name: refactor-code
description: "Use when refactoring, reducing, or rebuilding code in any language, covering reading, derivation from owners, removal of declarations, rebuilt shape, adversary passes, and verification"
---

# [REFACTOR_CODE]

Refactors start from what a file is for, derive each value from its owner, delete each declaration stating no fact of its own, rebuild what remains with its consumers in one change, deepen what fails under real input as hard as they reduce, and end smaller or name the capability each line of growth serves. Use `<language>-coding` skills for language form, `review-change` for judging and codifying a finished change, `manage-repo` for check owners and entry points.

Scope names what one pass edits:

| [INDEX] | [SCOPE]      | [RUN]                                                                                     |
| :-----: | :----------- | :---------------------------------------------------------------------------------------- |
|  [01]   | File         | One agent reads it whole with every consumer and edits, then adversaries of `[05]` follow |
|  [02]   | Folder       | `deepen` workflow with `args.mode` `reduce`, readers, editors, adversaries per file group |
|  [03]   | Consumer set | Owner file first, each consumer rebuilt in the same change, no old path kept              |

## [01]-[READING]

Read before one line changes. A change from memory of an API, from a summary, or from an outline standing in for a file restates what a package holds and misses a consumer's shape:
1. Run `git status --short` and `git log -5 --stat`, the live tree with uncommitted edits is the base
2. Choose files with `ast-grep outline <path>`, then read each file to edit line by line whole
3. Read every consumer to its effect: `ast-grep outline --items exports <file>` names exports, `rg -F '<name>'` and `ast-grep run` find call sites
4. Run each entry point running the code (an Nx target, a host, a binary) once on repository input for its normal output
5. Read dependency source at its installed release through `search-code` for each member the file calls and each job it hand-writes
6. Read `git log -S '<literal>'` for why a guard, flag, or form exists, a form arriving with a file serves it until its cause is read
7. Read the language skill, its `.claude/workflows/` companion, and memory entries as owner rules
8. Read recorded research (`docs/research`, workflow findings) as inputs, a decision there changes on a new fact alone
9. Count lines and code lines (`loc`), declared types, exports, parameters, flags, files, and dependency rows, kept for the report of `[06]`
10. Copy each file at base into the scratchpad for the old-against-new run of `[06]`

Reading ends in a statement of purpose: what the file is for, who calls it, how it runs, the advanced input it serves, and the fact each declaration states, a declaration stating none is the first removal of `[03]`.

Instructions read for the problem they name: an edit follows that problem over the instruction's word, an objection carries a fact from the read, a question the read settles is settled in the report, and a file showing no finding stays as it is and is reported compliant.

## [02]-[DERIVING]

Each value has one owner, read in rank order, and code states what no owner states:
- Owner test: name who states each literal, table, list, flag, and kind, an owner stating it means code reads it from that owner at build or run time
- Rank: a language feature or standard member, then a package or host member at its installed release, then a declaration in the tree, then code
- Package depth: a traversal, parser, matcher, or conversion runs through a dependency's own member, searched for before one is written
- Gap test: an input class, option, or case a dependency handles and the code does not closes through that member, never by hand code beside it
- Host values: a host list, product name, path, or tuned value inside library code enters as caller data
- Level: a capability sits once at the lowest layer holding it whole (universal, cross-host, host, product), never split or repeated above
- Divergence: behavior kept apart from its source on purpose names the source, the behavior, and the input triggering it, in the report and briefs
- Performance: a hot path gets faster by fewer passes, allocations, and parses over one input, never by a dropped case, a lock, or a wait
- Speed claims come from one `hyperfine` run per stage on the real command, before and after, with every capability kept

## [03]-[REMOVING]

Surface is every declaration a reader must know, internal (helpers, fields, locals, nesting, parameters) and external (exports, public parameters, flags, files, entry points, dependency rows), and both reduce by deletion and folding, never by nesting or hiding. Deletions come first, each consumer is rebuilt in the same change, a file whose deletions reshape it is rewritten whole, and findings a removed guard reveals are fixed at their sites in the same change.

Each kind goes on its test and takes its replacement:

| [INDEX] | [KIND]                 | [GOES_WHEN]                                                  | [REPLACED_BY]                                  |
| :-----: | :--------------------- | :----------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Struct, record, class  | Owner states its fields, or private with one reader          | Owner's type, tuple at that reader             |
|  [02]   | Enum, union            | Variants mirror a type's, or one variant is ever built       | Mirrored type, variant of an existing enum     |
|  [03]   | Alias                  | Names a package type, a tuple, or one site's instantiation   | Named type at each site                        |
|  [04]   | Constant, string row   | Owner states the value, or one site reads it                 | Read from the owner, inline value at the site  |
|  [05]   | Bool pair, flag        | Selects among cases a type can name                          | One enum, derivation from the target           |
|  [06]   | Wrapper, adapter       | Forwards a call unchanged, or converts as a package member   | Direct call, package's conversion              |
|  [07]   | Helper                 | One reader calls it, or it dodges a name clash or a cycle    | Body at the reader, renamed owner              |
|  [08]   | Parameter              | Every caller passes it alike, or the target derives it       | Value folded into the owner, derivation        |
|  [09]   | Export                 | Exposes an internal member, or grew naming no capability     | Private declaration                            |
|  [10]   | Error variant          | No caller branches on it                                     | Source error through its owner's variant       |
|  [11]   | Guard, fallback, retry | Proves own state, or stands where a cause needs a fix        | Construction refusing it, fix at the cause     |
|  [12]   | File                   | Alone in or named for its folder, a helper, or a cycle break | Sibling file of its concept, fact at its owner |
|  [13]   | Generator              | Output equals input                                          | Source read directly                           |
|  [14]   | Dependency row         | No code in the package imports it                            | Nothing, catalog row stays                     |
|  [15]   | CLI flag, subcommand   | Tool derives the value from its target or state              | Derivation, one name per operation             |

Tests deciding a row:
- Reader test: count readers, one reader folds the declaration into that reader
- Branch test: for an error, guard, flag, or arm, name the caller branching on it or the repository input reaching it, none means it goes
- Capability test: after removal, name each input the code no longer handles, one named input reverts the removal or names a dropped capability
- Shape test: declarations differing by a value or a case become one parameterized form, quantities held apart become the one relation holding them
- Logic test: a declaration stating its own fact passes the keep test of `CLAUDE.md` `[04]-[CODE_QUALITY]` before it stays
- Cause test: a guard, fallback, retry, or clamp names the cause it covers, fixed at its owner in the same change, a supervisor's restart row is none
- Consumer count decides no public shape: unused public API stays, weak API is rebuilt on its merits

Removal is total across the tree: every mention, consumer path, manifest row, skill line, and README line goes, found by `rg -F` and `ast-grep run`, and nothing stands in.

## [04]-[REBUILDING]

What remains takes its smallest shape with its consumers, `CLAUDE.md` `[04]-[CODE_QUALITY]` and the language skill's `[PRINCIPLES]` state the forms, and a refactor applies them by test:
- Nesting test: branches past the level limit test one discriminant, lifted into one match, table, or relation over it, never an extracted helper
- Checker test: checkers disagreeing on a shape (a bool pair and a nested tuple each forbidden) point at a missing variant, never at a suppression
- Name test: one name per concept across a file and its area under `clean-prose` terms, a type and its value of one name merge into one declaration
- Growth test: each line above base names a capability absent before, in its smallest form, a consumer wanting it is no justification
- Depth test: logic failing under a file's advanced input is rebuilt to hold it, a capability its dependency source shows joins in its smallest form
- Unchanged test: a file left as it is names the advanced input it serves and each package member it uses, a file naming neither takes a pass
- Consumer test: each consumer is rebuilt to the new shape in the same change, a path or default kept for old callers is a second declaration
- Shuffle test: `GIT_EXTERNAL_DIFF=difft git diff <base>` separates reshaped from moved code, a hunk that only relocates text is finished or reverted

## [05]-[CHALLENGE]

Fresh agents with no prior context follow each pass over the same scope under the same instruction, edit in place, and lean further toward removal and depth: the first removes what the pass kept and builds what the depth test of `[04]` names, the second collapses what remains into stronger logic and restores each regression of the first, and `review-change` `[04]` judges the result hunk by hunk. A file scope runs each as a delegated agent, a folder runs them through `deepen`.

Brief contents:
- Owner's instruction verbatim
- What the file is for, who calls it, and how it runs
- Verified facts with sources (installed releases, source paths, host behaviors), challengeable with evidence alone
- Previous pass's mistakes as facts: checker conflicts met, build errors, logic defects a live run caught, rounds spent on hand-typed input
- Divergences kept on purpose with their trigger inputs
- Working tree state and the scratchpad path of the base copy
- Verification set and report form of `[06]`
- Nothing of the previous pass's report, judgments, or reasons for what it kept
- No checking, probing, or measuring instruction beyond the verification set, and no enumeration narrowing what the pass can find

Conduct:
- Fixes what it finds in place, with no finding list handed to a later pass
- Re-derives no verified fact and relitigates no recorded decision without a new fact
- Fans readers over a scope too wide for one context and edits each file through one editor
- Probes with inputs taken from the repository, a hand-typed input stands for a branch the repository never reaches alone

Success: declarations and lines fall against the pass before or each kept declaration is named with its fact, each capability holds or each dropped one is named with the input it served, each file left unchanged passes the unchanged test of `[04]`, every item of `[06]` passes, and old against new differs on intended changes alone.

Failure: a shuffle, an alias, growth naming no absent capability, a re-derived fact, a suppressed check, or a finding deferred.

## [06]-[VERIFICATION]

Refactors are finished when each item passes once, at the end, over edited files and their consumers, in order:
1. `hk fix <paths>` over changed files
2. Build or type check of each edited project and each consumer project with warnings denied
3. Project `check` target where one exists, `nx run rasm:lint:<tool> -- <paths>` for each root checker reading the files, `ast-grep` among them
4. Live run: the real entry point running the code, exit 0 with its normal output
5. Old against new as evidence, never a gate: base copy against the new build on repository input per branch, each difference judged by intent
6. Counts of `[01]` after against before, each kept declaration named with its fact, a file above base naming no capability takes a pass of `[05]`
7. No test created, a test the rebuild breaks rewritten to the new shape or deleted, no copy or backup in tree, scratchpad litter removed

Green means every check of the edited projects, not one checker, and checks run once at the end, never per step and never repository-wide.

Report: what changed, what it removed, what each kept value derives from and which member or declaration it reads, what it kept and why, counts before and after, verification output with exit codes verbatim, and `Unverified:` claims with the command settling them.
