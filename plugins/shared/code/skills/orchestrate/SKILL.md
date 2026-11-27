---
name: orchestrate
description: "Use when splitting a rebuild, refactor, or cleanup across delegated agents, one agent per file, covering file owners, the brief, relays, the verdict on each diff, one build, and the ledger"
---

# [CODE_ORCHESTRATE]

Rebuilds across many files run as parts: one delegated agent per file rebuilds it, and the conductor (the session that delegates) splits the work, writes one brief, relays contracts, reads each diff against the verdict, builds once, and commits. Use `code:refactor` for each file's quality and its adversary passes, which run as later rounds of parts.

## [01]-[PARTS]

Splits come from the conductor's own read of the files and their consumers, and every file has one owner:
- One file is one part, a type with its extension or a module with its manifest row stays one part
- No agent takes a phase, project, or folder, as an agent holding many files fills its context and reports late
- Whole-folder rebuilds start from an inventory file and one owner-approved layout (folders, files, schemas, exports), and parts follow its concepts
- Shared types sit in one contract file, its part runs first, and reading parts start against the finished contract
- Manifests and build, test, or tool configuration files each have one owning part, or stay with the conductor
- Consumers outside the parts change only where a contract change forces it, each change named in the report
- Cross-file decisions, owner questions, builds, test runs, and commits stay with the conductor
- One ledger file lists each part's agent, file, report path, and verdict, trusted over recollection after a compaction

Parts edit and run file-level checkers on their own file alone, and the conductor builds and tests once, as concurrent compiles (standalone type checks and probe programs included) exhaust memory and process slots and collide in shared build folders. Parts past the harness's concurrent agent cap (`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` in Claude Code, `agents.max_concurrent_threads_per_session` in Codex) start as earlier parts end.

## [02]-[BRIEF]

Parts start as fresh agents, never forks, with project instructions and the skills they load, and anything else the conductor knows reaches them through the brief alone. One brief file holds what parts share, as pasted text stays in the conductor's context, and each prompt names the part's file, that file's known defects with their source, and the brief path:

| [INDEX] | [SECTION] | [HOLDS]                                                                                      |
| :-----: | :-------- | :------------------------------------------------------------------------------------------- |
|  [01]   | Work      | Goal, scope, and the owner's words with their date                                           |
|  [02]   | Skills    | Each skill to load whole: `code:refactor`, language coding skill, `code:test`, host skill    |
|  [03]   | Sources   | Files and docs to read whole beyond the part's file and consumers, each with what it answers |
|  [04]   | Facts     | Verified facts with their source, decisions taken, failing forms with a search pattern       |
|  [05]   | Owners    | Owning part of each contract, manifest, and configuration file                               |
|  [06]   | Limits    | Runs and edits no part makes: builds, test runs, commits, peer files, other sessions' edits  |
|  [07]   | Report    | Report fields of this skill, copied whole                                                    |

Briefs add facts no loaded file holds and leave everything else out:
- Project instructions and named skills load in every part, a brief restating their rules gets them restated back in every report
- Facts enter with their source (installed file, header line, doc page, measured run), memory facts copied in, unsourced claims left out
- Steering that narrows a pass (check only this, keep that, an expected result) is left out, a boundary is stated as scope
- Later rounds add the contracts and decisions earlier rounds took, never their reports or summaries

## [03]-[RELAYS]

Parts message the conductor alone, and the conductor relays between parts:
- Parts send each need in one message and keep working, a need for a peer's file included
- Contract parts send each change whole in one message naming every moved member and reading consumer, forwarded to each reading part that turn
- Parts asking about a peer's file get one answer, done or still editing, and do other work meanwhile
- Rulings on a proposed cut go to the asking part and to each part the cut reaches
- Findings for a part still editing wait for its report and join its verdict
- Relays for an ended part go to a new agent with its report path and the change
- Uncommitted edits of another session stay as found, and a part edits around them
- Parts wait on their own background work in one foreground call and end once, as each ended turn wakes the conductor

## [04]-[VERDICT]

Conductors read each part's diff whole against every row once it reports, before the build, as passing tests and clean checks prove no row. One failing row returns the file to a fresh agent with the report path and the failing rows verbatim, as an agent rarely sees its own defect:

| [INDEX] | [CRITERION]  | [PASSES_WHEN]                                                                                                    |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | Library      | Library code names no product, app, host, or harness, and callers choose policy and runtime                      |
|  [02]   | Owner        | Each value and behavior comes from its host, package, or declaration, no literal or table copy                   |
|  [03]   | Form         | Members, rows, or tools differing by a value are one member taking it, one shape has one owner                   |
|  [04]   | Surface      | Capability arrived as better behavior of an existing member before any new export, exports fell                  |
|  [05]   | Removal      | Each declaration `code:refactor` removes is gone, none hidden from a checker, each kept one listed with its fact |
|  [06]   | Strings      | No string, constant, or enum renaming strings decides behavior, each case reads the type owning it               |
|  [07]   | Names        | Each name is its field's term from docs, functions take verbs, values nouns, nothing coined or borrowed          |
|  [08]   | Architecture | Folders name a field outliving their members, files hold one concept, none sits alone in a folder                |
|  [09]   | Checks       | Checks over external input and host replies stay, checks proving the program's own shape are gone                |
|  [10]   | Cleaning     | Removed members leave no mention, comment, dead branch, scratch file, or backup                                  |
|  [11]   | Output       | Text an agent or person reads is short and exact, names refs, and gives the next call                            |
|  [12]   | Tests        | No test that cannot fail on this machine or exceeds the request, a broken test rebuilt or deleted                |
|  [13]   | Evidence     | Diff matches no failing form of the brief, and each report claim holds its source or a live step                 |

## [05]-[BUILD]

Builds run once every part of a round passes the verdict:
1. Build in the tree
2. Fix each diagnostic at its cause and rebuild until warnings and errors are zero
3. Run the suites the parts touched, a test leaving a process, window, or file behind fails the build
4. Run each part's live step and compare its result with the screen or the host's own state
5. Commit the parts' files by path and leave other sessions' edits uncommitted

Compiler crashes and checker refusals the build found enter the brief as failing forms with their search pattern before the next round.

## [06]-[REPORT]

Parts end with one message of facts and findings, restating no skill rule, with bulky output in a file it names:
- Lines before and after, declarations and exports removed, each kept declaration with its fact
- Each host or package member called, with its source
- Contract changes, each with every consumer it reached
- File-level check output quoted, with exit codes
- Defects outside the part, each with its proven cause
- One live step proving the file, for the conductor to run

Conductors report each commit, each part's verdict, defects sent to their owner, and claims still unproven with the step proving each.
