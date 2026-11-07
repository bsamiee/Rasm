---
name: review-repo
description: "Use when mapping, auditing, or reviewing the repository or one area or concern of it, covering owner mapping, fact tracing, verification, delegation, and finding format"
---

# [REVIEW_REPO]

Reviews find facts stated away from their owner, capabilities rebuilt by hand, variants enumerated by hand, rows nothing reads, and entry points an agent cannot use in one call, each proven against live tree, installed dependency, and recorded decision.

Arguments name a subject and a depth, a missing depth means `review`:

| [INDEX] | [SUBJECT]  | [EXAMPLE]                                | [SPLIT]                                    |
| :-----: | :--------- | :--------------------------------------- | :----------------------------------------- |
|  [01]   | Repository | Every root file, `eng/`, `tools/`, CI    | One reader per concern, a lead merges      |
|  [02]   | Area       | `tools/nx`, `.github`, project manifests | One reader, boundary facts become notes    |
|  [03]   | Concern    | Config unification, caches, process env  | One reader per owner file the concern hits |
|  [04]   | Question   | Why a flag exists, whether a row is read | No split, answer with evidence             |

| [INDEX] | [DEPTH]  | [RESULT]                                                         |
| :-----: | :------- | :--------------------------------------------------------------- |
|  [01]   | `map`    | Owner per fact, consumers, live state, no findings               |
|  [02]   | `review` | Ranked findings, owner calls, checked-and-right list             |
|  [03]   | `verify` | Each listed claim confirmed or refuted in a fixture, nothing new |

## [01]-[OWNER_MAP]

Map before judging. A finding drafted before its owner and decision record are known is rewritten or dropped later:
1. Run `git log -5 --stat` and `git status --short` to see work in progress, then review the live tree, uncommitted edits included
2. Read README `[OWNERS]` and `[STRUCTURE]` for the owner of each concern
3. Read `manage-repo` and each reference for the subject's toolchains, rows there record decisions and their tool behavior
4. Read each skill naming a subject file (`rg -l '<file>' .claude/skills`), skills record decisions (why a secret reaches CI as a file)
5. Read the project memory folder, each entry is an owner rule a finding obeys
6. Resolve projects and their targets with `nx show projects` and `nx show project <project> --json`
7. Read what a target runs and hashes with `nx show target <project>:<target>` and `nx show target inputs <project>:<target>`
8. Choose what to read in an unread file or folder with `ast-grep outline <path>`, and read a file whole before stating a fact about it

Owner checks per concern:
- Process env: `mise config ls`, `mise env --json`, `mise settings ls --all --json`, user-global mise files included
- CI and repository settings: `gh api` for runs, job step timings, caches, and repository settings, set against the `infra/` program declaring them
- Packages: lock files and installed versions through `search-code`
- Build: resolved properties and items from the build tool, a binary log through `dotnet-msbuild-diagnostics`
- Tool state: each log, cache, and state path a tool documents, read at its default location, a setting moving one file leaves the rest there

## [02]-[TRACING]

Each fact gets one trace, declaration to effect:
- Declaration: one file per fact as README owners assign, a second declaration is a finding against the copy
- Restatements: literal search (`rg -F '<value>'`) and code-form search (`ast-grep run`) across the tree, skills and README included
- Consumers: every reader of a value, followed to its effect, a value no consumer reads is a row nothing reads
- Live state: what a declaration claims to produce, read from its tool or service, drift between them is a finding
- History: `git log -S '<literal>'` and `git show <commit>` for why a form exists, read both ways (a flag arriving with a file serves it)
- Members: each list, name suffix, path set, or flag value traces to the declaration stating its members, a list owning nothing is a finding
- Additions: adding one project, package, version, or platform edits its own declaration alone, every other file the addition needs names a finding

Entry points trace as one inventory before any finding: every target with configurations and forwarded arguments (`nx show project <project> --json`), every task (`mise tasks ls`), and every tool subcommand and flag (its `--help`):
- Group names by operation, a name per subject or mode is a variant the tool's own axis replaces
- Run each operation once, a derivable flag, a result missing its state, a causeless failure, or a wrong exit status is a finding
- Concerns reached through more than one entry system (target, task, script) are one finding

Behavior claims follow every edge out of a file before concluding:
- Includes, imports, `extends`, `Import`, `#include`, and path sources lead to code a single-file search misses
- Generated files lead to their generator and source, fetched files to their pinned upstream commit
- Dependency behavior comes from installed source or source at the pinned tag through `search-code`, docs and web pages rank below both
- Hand-rolling is proven by naming a dependency member doing that work at its installed version and showing matching output

## [03]-[VERIFICATION]

Reviews write nothing in the tree and run no install, Nx target, daemon restart, or deploy. Read-only commands (`nx show`, `mise env`, `gh api` reads, `hyperfine` on read-only commands) run freely.

Experiments run in a fixture under the scratchpad:
- Redirect `HOME`, every `XDG_*`, and each tool home (`CARGO_HOME`, `UV_CACHE_DIR`, `NX_WORKSPACE_DATA_DIRECTORY`) into it as `$HOME` paths
- Unset the `mise` and `cd` shell functions, run `env -i` with an explicit `PATH` when inherited env decides a result
- Hash `git status --porcelain` before and after a run against the tree to prove it wrote nothing
- Delete the fixture and every scratch file outside it before reporting

Forbidden targets keep their steps open to test:
- Run a target's single tool step, read through `nx show target`, on a fixture copy (`cc -c`, `tsc --noEmit`, `hk check --plan --json`)
- Minimal reproductions decide one behavior (empty template export, default `types`, argument order of `fd --exec-batch`)
- Claims nothing can verify carry `Unverified:` with a command that settles them, and stay out of the ranked list

Measure only claims about cost. One measurement showing a cost is no problem goes in the checked-and-right list, more timing of one path is waste.

## [04]-[FINDINGS]

Real findings break a repository standard or an owner rule with consequence:

| [INDEX] | [KIND]          | [TEST]                                                                  |
| :-----: | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | Duplicate fact  | Same value or list stated by more than one owner, a copy can drift      |
|  [02]   | Hand-rolled     | Dependency member at its installed version does the same work           |
|  [03]   | Hardcoded       | Value a host, package, or declaration states, written as a literal      |
|  [04]   | String-typed    | String or string union deciding behavior where a typed handle exists    |
|  [05]   | Wrong level     | Row one project reads sits in a root file, or shared policy per project |
|  [06]   | Dead row        | Row, input, or setting no consumer reads and no tool output changes     |
|  [07]   | Drift           | Live state differs from its declaration, or README from the tree        |
|  [08]   | Hidden cause    | Wrapper, fallback, flag, or retry covering a defect at its root         |
|  [09]   | Self-check      | Code proving its own code, config, or output                            |
|  [10]   | Parallel script | Script or one-off flag doing what an owning target or tool option does  |
|  [11]   | Enumerated      | Name, row, file, or list per subject or mode a declaration derives      |
|  [12]   | Agent interface | Entry point breaking a `manage-repo` entry point rule                   |

Owner rules decide what never becomes a finding:
- Package and dependency rows stay, a row with no importer goes in an observed-not-proposed list
- Public API with no consumer stays, weak or redundant API still gets a finding on its merits
- Areas the owner parked (tests while parked, `docs/research/` while rebuilt) take no finding
- Memory entries marking a row intentional (disabled MCP rows) settle it
- Tool defects take a form change in the repository, never an upstream issue or pull request

Recorded decisions are challenged openly:
- Findings against a decision in a skill, reference, or README cite its line and state a fact its record missed
- Findings repeating what a record already rejects, without a new fact, are noise

Fixes pass the same standards as code:
- Fixes remove files, rows, members, or lines, a fix adding a service, bucket, script, or root file states why no smaller owner exists
- Fixes move a fact to its owner, a wrapper over a wrapper is a second defect
- Fixes for an enumerated variant take the tool mechanism `manage-repo` derivation lists, else code in the layer `manage-repo` names
- Fixes follow the tool's cost model: a forced split keeps its runs and derives them, a split a parameter covers becomes one name
- Fixes touching a secret keep its value out of logs and uploaded artifacts

## [05]-[REPORT]

Each finding holds five parts:
- Wrong: the fact, its owner, and the copy or hand-rolled form, by file and line in the live tree
- Evidence: commands run and their output, dependency source file and line, fixture result
- Change: an exact edit against live text, file deletions, or row moves, every consumer adjusted
- Removes: files, rows, members, and lines the change deletes
- Risk: behavior that changes, and any `Unverified:` claim

Order and grouping:
- Rank by consequence across the whole report: broken or misleading state, then drift, duplicate owners, hand-rolling, dead rows
- Fold low items into one short list after the ranked findings
- Owner calls (policy trade-offs, a recorded decision challenged) sit in their own section with each side's cost
- Checked-and-right list: each item a form that looked wrong, with the dependency fact or record making it correct
- Observed-not-proposed list: items owner rules withhold
- Cross-area notes: facts owned by another area, with its owning file, for a lead to route

## [06]-[DELEGATION]

Delegate when a subject spans more than one owner file family or more than an hour of reading. A lead keeps the merge and spawns all readers in one message.

Split by concern and owner, never by folder alone. Facts at a boundary (an `nx.json` input naming a `.github` file, a CI step restating a mise row) go to whichever reader owns their declaration, other readers record a cross-area note.

Each reader brief states:
- Slice: files owned, files read only for composition, files excluded
- Standards: `CLAUDE.md` sections that judge the slice, owner rules from memory, parked areas
- Records: manage-repo references and skills covering the slice, read before any finding
- Capabilities: dependency features to check hand-rolling against, by package and installed version
- Rules: read-only tree, fixture folder with redirected homes, cleanup, no report file
- Return: findings in the five-part form, `Unverified:` labels, checked-and-right list, cross-area notes

Lead steps after every reader returns:
1. Wait for completion notices, polling a reader's output wastes calls and reads partial results
2. Merge duplicates under their owner file, keeping strongest evidence
3. Reconcile conflicting verdicts on one fact by evidence, re-running the deciding check in a fixture
4. Re-check each top finding against live text before it enters the report
5. Rank the merged set once across all readers

## [07]-[EXCLUSIONS]

Reports leave out:
- Style preferences and formatting a formatter owns
- Counts, timings, and line numbers that change with the next commit, outside evidence
- Symptoms with no owner found, the trace continues or the item becomes `Unverified:`
- Findings from one file search over code that includes, imports, or generates other code
- Proposals restating a recorded decision without a new fact
- Tool versions in findings, versions belong to their lock or catalog row
