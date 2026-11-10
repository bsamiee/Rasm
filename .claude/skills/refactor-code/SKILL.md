---
name: refactor-code
description: "Use when refactoring, reducing, or rebuilding code or configuration in any language, covering reading, host and package derivation, removal, member structure, and adversary passes"
---

# [REFACTOR_CODE]

Refactors read a file whole with its consumers, hosts, and packages, call each behavior and value at its owner in rank (a language feature or standard member, then a package or host member at its installed release, then a declaration in the tree, then code), delete each declaration stating no fact of its own, rebuild each remaining member into one structure, deepen through packages and hosts as hard as they reduce, and end smaller, growth stands for a capability the base lacked alone, in its smallest form, after a smaller form was searched. Code, configuration, build, rule, and checker files take the same passes. Use `<language>-coding` skills for language form, `review-change` for judging and codifying a finished change, `.claude/workflows/languages.js` for check owners.

Scope names what one pass edits:

| [INDEX] | [SCOPE]      | [RUN]                                                                                     |
| :-----: | :----------- | :---------------------------------------------------------------------------------------- |
|  [01]   | File         | One agent reads it whole with every consumer and edits, then adversaries of `[06]` follow |
|  [02]   | Folder       | `deepen` workflow with `args.mode` `reduce`, readers, editors, adversaries per file group |
|  [03]   | Consumer set | Owner file first, each consumer rebuilt in the same change, no old path kept              |

## [01]-[READING]

Read before one line changes, a change from memory of an API, a summary, or an outline standing in for a file restates what a package or host holds and misses a consumer's shape:
1. Run `git status --short` and `git log -5 --stat`, the live tree with uncommitted edits is the base
2. Choose files with `ast-grep outline <path>`, then read each file to edit line by line whole
3. Read every consumer to its effect: `ast-grep outline --items exports <file>` names exports, `rg -F '<name>'` and `ast-grep run` find call sites
4. List every entry point reaching the code: Nx target and its `--args`, CLI switch, `dotnet publish -o`, host launch, hook rewrite, served product
5. Run each entry point once on repository input for its normal output
6. Read dependency and host source at installed release through `search-code` per member called, job hand-written, and behavior performed
7. Read `git log -S '<literal>'` for why a guard, flag, or form exists, a form with no cause in history, source, or a repository input goes
8. Read the language skill, its `.claude/workflows/` companion, and memory entries as rules, `docs/research` as inputs changing on a new fact alone
9. Count lines and code lines (`loc`), declared types, exports, parameters, flags, files, dependency rows, nesting depth, and branches per member

Reading ends in a statement of purpose: what the file is for, who calls it, how it runs, the advanced input it serves, each behavior with the host or package member performing it, and the fact each declaration states, a declaration stating none is the first removal of `[04]`.

Decisions come from facts read: an instruction is followed for the problem it names over its word, an objection carries a fact from the read, a question is settled by the read, a source, or one run and decided in the pass with the fact reported, never asked, and a finding is fixed in the pass that found it, never deferred or handed on as a list.

## [02]-[HOSTS]

Hosts (applications, runtimes, build systems, checkers, generators) perform behavior and state values, code calls the host member for the behavior and reads the host declaration for the value, and a hand copy of either is wrong whatever its quality and diverges at the host's next release:
- Behavior: for each behavior a member performs (open, show, select, save, quit, style, layout, launch), name the host member performing it
- Behavior outcome: a member found is called, a behavior the host performs on its own (dialog, save, instance, focus) leaves the library
- Value: each literal (name, path, size, version, instance count, dialog kind, key) names the host declaration stating it, code reads it there
- Category: a host property (single instance, own dialogs, argv as files, save on quit) derives from the installation or the running process
- Category outcome: an enumerated list of apps per property is a hand copy and goes from every file, library code names no host
- Version: one code path runs on the newest host release and its betas, a version-keyed branch, path, or file kind goes
- Version outcome: older installs exist to load built products, no code targets them
- Interface: panels, dialogs, prompts, themes, and documents are host kinds built through host members, diagnostic text goes to the logger
- Proof: a host behavior claim holds when one run on a real document in the running host showed it, a run in another environment proves nothing
- Instance: documents open in the running instance behind the user's windows by z-order, no pass hides, kills, relaunches, or duplicates a host
- Dialog: a host dialog is read and answered through the host, never silenced by hiding, a host event is awaited, never polled or waited out

Finding the member and the case the host lacks it:
- Source: read the host's installed source (decompiled assembly, scripting dictionary, interpreter reference, typings) through `search-code`
- Reference: read the newest release's reference for a member the installed source lacks, then drive the running host for the proving run
- Driving: `use-rhino`, `use-blender`, `watch-apps`, or the host's MCP, the running instance alone
- Absent value: a value no host declaration states enters as caller data at the boundary, stated once, the library derives, defaults, and composes
- Absent value outcome: library code holds no product name, style name, palette, sheet size, margin, path, or tuned value
- Absent behavior: a behavior no host member performs composes host kinds and package members over caller data, never a copy of a host member
- Level: a capability sits once at the lowest level holding it whole (universal, cross-host, host, product), never split or repeated above

## [03]-[PACKAGES]

Depth of package use is co-equal with reduction, a pass that deletes alone is half a pass, and a package member replaces hand code even when the member is the less convenient one:
- Depth: for each file, name the advanced input it serves and each package member it uses, a file left unchanged without both takes a pass
- Depth outcome: "left as is" is a failure report, a file naming both is reported with them
- Member: before a traversal, parser, matcher, codec, conversion, logger, launcher, or layout is written, search the dependency's installed API
- Member outcome: a member found replaces the hand code, a private module reachable through a public inference path is reached that way
- Gap: an input class, option, or case the advanced input needs and the dependency handles closes through its member, never by code beside it
- Feature: a capability behind a feature flag, an extra, or a companion package enters by enabling or adding it in the owning manifest row
- Source: a package semantic (concurrency, accumulation, stack depth, limit) comes from installed source and a measured run
- Source outcome: a split respecting a measured limit stays, a semantic no source read or run settled writes no code
- Row: a package row leaves with the last code importing it, a row removed with a folder while its capability is wanted is over-removal
- Port: a library copying another language's library in the tree is a hand copy, each language holds its own ecosystem's package for the job
- Release: a form from memory of an older release takes the installed release's form, newest per `CLAUDE.md` `[03]-[DEPENDENCY_POLICY]`

## [04]-[REMOVING]

Surface is every declaration a reader must know, internal (helpers, fields, locals, nesting, parameters) and external (exports, public parameters, flags, files, entry points, dependency rows), and both reduce by deletion and folding, never by nesting or hiding. Deletions come first, each consumer is rebuilt in the same change, a file whose deletions reshape it is rewritten whole, and findings a removed guard reveals are fixed at their sites in the same change. Declarations are junk by default per `CLAUDE.md` `[04]-[CODE_QUALITY]`, one stays when no owner states its fact and keeping it removes more logic than it adds.

Each kind goes on its criterion and takes its replacement:

| [INDEX] | [KIND]                 | [GOES_WHEN]                                                           | [REPLACED_BY]                         |
| :-----: | :--------------------- | :-------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Struct, record, class  | Owner states its fields, or keeping it removes no logic               | Owner's type, tuple at the reader     |
|  [02]   | Enum, union            | Variants mirror a type's, one is built, or no caller branches on them | Mirrored type, existing variant       |
|  [03]   | Alias                  | Renames a type, has one reader in its module, or dodges a clash       | Named or qualified type at each site  |
|  [04]   | Constant, string row   | Owner states the value, or one site reads it                          | Read from the owner, inline value     |
|  [05]   | Bool pair, flag        | Selects among cases, or holds a fact the target derives               | One enum, derivation from the target  |
|  [06]   | Wrapper, adapter       | Forwards a call unchanged, or converts as a package member does       | Direct call, package's conversion     |
|  [07]   | Helper                 | Born of this pass, one reader, or dodges a name clash                 | Body at the reader, renamed owner     |
|  [08]   | Parameter              | Every caller passes it alike, or the target derives it                | Folded into the owner, derivation     |
|  [09]   | Export                 | Internal member exposed or no capability named, never lack of readers | Private declaration                   |
|  [10]   | Re-export              | Forwards a whole crate or a member no public signature exposes        | Own capability, direct reference      |
|  [11]   | Error variant          | No caller branches on it                                              | Owner's error                         |
|  [12]   | Guard, fallback, retry | Proves own state, no listed entry point reaches it, or hides a cause  | Construction refusing it, cause fixed |
|  [13]   | File                   | Alone in or named for its folder, or one helper                       | Sibling file, fact at its owner       |
|  [14]   | Generator              | Output equals input                                                   | Source read directly                  |
|  [15]   | Dependency row         | No code imports it and no capability of the pass wants it             | Nothing, catalog row stays            |
|  [16]   | CLI flag, subcommand   | Tool derives the value from its target or state                       | Derivation, one name per operation    |

Criteria deciding a row:
- Reader: one reader folds a declaration into that reader, consumer count decides no public shape, unused public API stays, weak API is rebuilt
- Re-export: each consumer use of a re-export becomes library capability, a type a signature exposes, or a direct import of the dependency
- Library: a library choosing a caller's entry point, runtime, transport, or configuration becomes parts callers compose over dependency types
- Entry: a guard, flag, arm, or default goes when no entry point of `[01]` step 4 reaches it, a named entry keeps it until it derives the value
- Capability: name each input a removal stops handling, one named input restores the removed code, a dropped capability fails the pass
- Shape: declarations differing by a value or a case become one parameterized form, quantities held apart become the one relation holding them
- Cause: a guard, fallback, retry, clamp, lock, or wait names the cause it covers, fixed at its owner in the same change
- Cause outcome: a supervisor's restart row is host configuration and no retry
- Divergence: behavior kept apart from its source names source, behavior, and a repository input triggering it, no input means it goes
- Count: code kept short by an erased error, an untyped read, a discarded message, a dropped case, or a loose decode takes its typed form
- Suppression: each suppression names its reason, one a catalog or manifest row makes false goes and its finding is fixed
- String: a string deciding behavior goes, each case derived from the value's owner (type, member, host), never an enum renaming it
- Fragile: logic reading text shape, order, or spelling (a pattern over a message, an index into a split) reads the typed source in its place
- Injection: values a build tool or host supplies reach code through its typed channel (an import, a generated module), never a declared global
- Credit: a defect present before the pass and one the pass introduced are both fixed in the pass
- Triage: mixed work is read against the last clean commit, each change kept for a fact or capability it names or undone in place
- Logic: a defect a read finds is proven by one discriminating input from repository data on old and new code
- Output: a changed wire shape compares old and new outputs through one normalizer, never a gate on old output
- Consumer: a behavior change reaches every consumer site in the same change, each rebuilt site run on repository input

Removal is total across the tree: every mention, consumer path, manifest row, skill line, and README line goes, found by `rg -F` and `ast-grep run`, nothing stands in, and a removed mode, export, flag, or field takes every consumer in the same change, with served products run live after.

## [05]-[STRUCTURE]

Inside each member, flat spam (near-identical statements, arms, or rows) and stacked decisions collapse into one structure stating each decision once, `CLAUDE.md` `[04]-[CODE_QUALITY]` and the language skill's `[PRINCIPLES]` name the forms. Extraction is the forbidden form everywhere: a helper, a closure bound to a name, or a type born of the pass keeps the shape and adds a name.

Each shape replaces one spam or nesting form:

| [INDEX] | [SHAPE]                        | [REPLACES]                                         |
| :-----: | :----------------------------- | :------------------------------------------------- |
|  [01]   | Match over a tuple of subjects | Nested conditions over more than one discriminant  |
|  [02]   | Arm guard                      | Inner condition one arm alone reads                |
|  [03]   | Binding chain                  | Early returns and stacked unwrap guards            |
|  [04]   | Shared step hoisted            | Same statement repeated in every arm               |
|  [05]   | Table or relation              | Arms or rows differing by one value                |
|  [06]   | Iterator pipeline              | Index loops, side collections, collect then filter |
|  [07]   | Accumulating collect           | Loop with a mutable error list                     |
|  [08]   | Combinator choosing a value    | Decision inside a default-supplying closure        |
|  [09]   | Builder fold                   | Sequence of mutations on one object                |
|  [10]   | One parameterized member       | Members differing by a value, flag, or case        |

Algebraic forms hold where a relation is real and are never forced:
- Relation: quantities a caller computes from each other are one derivation solvable for any of them, a member or file per quantity fails
- Choice: a caller's choice among methods is an enum parameter, a `bool` holds a computed fact
- Deliberate: an enum, struct, or nesting stays when a caller branches per variant or it names a real relation, a type naming neither goes
- Catalog: a standard set callers extend is the generated enumeration beside one validated custom case, never a hand list beside a constructor

Every member gets faster through logic, the language skill's hot-path line holds for each, and speed from a lock, a wait, a narrowed class, a serialized stage, or a dropped case is a regression:
- Pass: name the pass, allocation, parse, or materialization removed (a collect before a filter, a second walk zipping by index)
- Level: name the level touched (grammar, regex class, rule shape, util, file walk, cache key, checker option), another level hides the cause
- Capability: every input handled before is handled after, a narrowed grammar, class, or skipped case is a regression with a timing
- Cost: linear-scan lookups, repeated walks or parses, unbounded recursion, per-element copies, and resources held past use are rebuilt
- Concurrency: many agents share the tree, correctness comes from hashing, caching, and per-task output folders

## [06]-[ADVERSARY]

Fresh agents with no prior context follow each pass over the same scope under the same instruction, edit in place, and lean further toward removal and depth: the first removes what the pass kept, calls hosts and packages where the pass hand-wrote, and builds what the depth criterion of `[03]` names, the second collapses what remains into one structure per member, deepens again, and restores each regression of the first, then `review-change` judges the result hunk by hunk. File scopes run each as a delegated agent, folders run them through `deepen`.

Brief contents:
- Owner's instruction verbatim
- What the file is for, who calls it, how it runs, and every entry point of `[01]` step 4
- Verified facts with sources (installed releases, source paths, host members, measured limits), challengeable with evidence alone
- Previous pass's mistakes as facts: checker conflicts met, build errors, logic defects a live run caught, host facts a proving run corrected
- Divergences with their triggering repository input
- Working tree state, the change read through `git diff HEAD -- <paths>` and the untracked files `git status --short` lists
- Nothing of the previous pass's report, judgments, or reasons for what it kept, and no checking, probing, or measuring instruction narrowing the pass

Conduct:
- Fixes each regression in place in the same pass, no finding list goes to a later pass
- Reads "a consumer needed it", "left as is", "kept for the lint", and "to be safe" as failure reports
- Re-derives no verified fact and relitigates no recorded decision without a new fact

Regressions an adversary restores in place:

| [INDEX] | [KIND]            | [SIGN]                                                                                                      |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | Growth            | Lines, declarations, exports, or files above base, growth tied to a named consumer included                 |
|  [02]   | Lost capability   | Input, grammar class, outline depth, or output the base served and the new code does not                    |
|  [03]   | Weakened check    | Lint lowered, rule narrowed, suppression, `files` exclusion, or dead branch kept for a lint                 |
|  [04]   | Snapshot gate     | Check comparing output to a pre-change copy, rebuilt output differs on purpose                              |
|  [05]   | Shuffle           | Move, split, rename, alias, forwarder, barrel, whole-crate or non-entry re-export, extracted helper         |
|  [06]   | Hidden cause      | Lock, wait, retry, guard, hidden launch, kill, or forwarded flag standing where a cause needs a fix         |
|  [07]   | Hand-written host | Behavior a host performs moved into code, host value as a literal, app list grown, version branch           |
|  [08]   | Package loss      | Member replaced by hand code, row removed with its capability wanted, feature turned off                    |
|  [09]   | Wrong removal     | Guard or flag removed while an entry point reaches it, consumer left broken                                 |
|  [10]   | Naive fix         | Checker finding fixed by a form `[04]`, `[05]`, or another checker refuses, or by a fix that does not build |
|  [11]   | Product break     | Served product, plumbing, or host integration failing after the change                                      |

Verdict: declarations and lines fall against the pass before with each kept declaration listed with its fact, a pass changing nothing fails unless every file passes the depth criterion of `[03]`, and a shuffle, growth naming no absent capability, a re-derived fact, a weakened check, a hand-written host behavior, or a finding deferred fails the pass.

Refactors are finished when, once at the end over edited projects and their consumers, `hk fix <paths>` ran, `nx run <project>:check` of each edited and consumer project (`hk check <paths>` for a file-level checker) passes with zero findings and warnings denied, and each entry point ran on repository input with its normal output. Checks run once at the end, never per step and never repository-wide, no test is created, a test the rebuild breaks is rewritten to the new shape or deleted, and no copy or backup sits in tree.

Report: what changed, what it removed, each kept value with the owner member or declaration it reads, kept declarations listed with their fact and count, counts of `[01]` step 9 before and after, check output with exit codes verbatim, and `Unverified:` claims with the command settling them.
