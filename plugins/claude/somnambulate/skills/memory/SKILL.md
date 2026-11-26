---
name: memory
description: "Use when saving, changing, or auditing a Claude Code auto-memory file or the memory set, covering admission, kinds, boundaries, descriptions that double as index lines, evidence marks, and recall proof"
---

# [SOMNAMBULATE_MEMORY]

This skill is loaded. Your next call Reads the reference for the work, before any read, search, or write in the memory folder, and its steps replace your own plan; Read it again after a compaction: a memory that does not exist yet -> `references/create.md`, a change inside one existing memory -> `references/edit.md`, the set or any merge, split, rename, or delete -> `references/evaluate.md`. `MEMORY.md` is generated from each file's `description` after every memory write, so no step writes it.

[REFERENCES]:
- [01]-[CREATE](references/create.md): Saving a new memory file
- [02]-[EDIT](references/edit.md): Improving one existing memory file
- [03]-[EVALUATE](references/evaluate.md): Auditing the memory set, and merging, splitting, renaming, or deleting files

[SCRIPTS]:
- [01]-[INDEX](scripts/index.py): Regenerates `MEMORY.md` and commits the folder when a memory file changes, run by the plugin
- [02]-[RECALL](scripts/recall.py): Replays stored requests through live sessions and scores which memory files each one opens

## [01]-[RECALL]

Claude Code recalls memory in either of its modes, and every file serves both:
- Mode: `jq '.cachedGrowthBookFeatures.tengu_moth_copse' ~/.claude.json` true or `CLAUDE_MEMORY_STORES` set means selector mode, otherwise index mode
- Index mode: session start loads `MEMORY.md` up to 200 lines or 25,000 characters, its lines usually the only memory text a session acts on
- Opening: a session reads a file whole only when its index line sends it there, so the line carries what a session acts on without opening
- Selector mode: `MEMORY.md` never loads, and a Sonnet call picks up to 5 files per prompt from `- [<type>] <file> (<mtime>): <description>`
- Description: the one text both modes read, as the selector's manifest line and as the generated index line
- Body: selector mode cuts a recalled file at 4096 bytes, so the lead and the facts most requests need come first in every file
- Caching: each session and its general-purpose subagents keep their starting index until compaction, `/clear`, or the plugin's refresh at the next turn
- Other sessions: running sessions keep their own copy, so a rename strands their links until they restart, and renames wait for a quiet pass
- Outside writers: harness save steps, dream, and extraction write memory without this skill, and the plugin restores the index after each

## [02]-[ADMISSION]

Each candidate line passes every test in order, and the first failure sends it to the home in its row:

| [INDEX] | [TEST]     | [QUESTION]                                                           | [ON_FAIL]                                         |
| :-----: | :--------- | :------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Derivable  | One read of code, git, instructions, docs, skill, or tool gives it   | Link the path, keep what overturns it             |
|  [02]   | Durable    | Matters a week from now, in a session on another task                | Task list, plan, commit body                      |
|  [03]   | Acting     | Names the future action it changes, in one clause                    | Delete                                            |
|  [04]   | Procedure  | Holds ordered steps a skill runs                                     | Skill running them, a correction staying here too |
|  [05]   | Checked-in | States a convention a tool or teammate reads from the repository     | Owning repository file, via the owner             |
|  [06]   | Evidenced  | Comes from the owner, a run, or a source read                        | Verify first, or delete                           |
|  [07]   | Home       | Belongs to no existing memory's subject, or states a field-wide rule | That memory, edited in place                      |

Status, progress, completed work, open-probe lists, unbuilt plans, values one run printed, and conclusions no run confirmed fail `[02]` in any wording.

## [03]-[KINDS]

Kind follows what the knowledge is, and its lead line is the text the description compresses:

| [INDEX] | [KIND]     | [TYPE]      | [LEAD_LINE]                         | [PARTS]                   |
| :-----: | :--------- | :---------- | :---------------------------------- | :------------------------ |
|  [01]   | Rule       | `feedback`  | `<condition>: <imperative>`         | Why, How to apply, `Not:` |
|  [02]   | Profile    | `user`      | Trait in the scope the owner stated | Work it changes           |
|  [03]   | Decision   | `project`   | `<choice>` by decider and date      | Why, `Holds until:`       |
|  [04]   | Fact sheet | `reference` | Answer most requests need           | Groups in work order      |
|  [05]   | Pointer    | `reference` | Location and what it holds          | When to open it           |

Kinds draw from one set of parts, and content decides which appear:
- Leads stand alone, each later line narrows or proves its lead, and a one-line rule with nothing to add is complete
- Why names an incident, constraint, or mechanism the lead lacks, sits out when none is known, and a Why citing only the owner goes
- `Not: <wrong> -> <right>` lines appear once a mistake recurs under the rule, at most 2, ending on the right form
- `Holds until:` names the unlock, expiry, or owner go-ahead ending a decision, or reads `standing`
- Bodies hold current understanding rewritten in place, facts name their subject over a pronoun, and a dead end reads `<form> fails because <cause>`
- Type changes recall in selector mode alone: its prompt holds `[user]` and `[project]` files to requests about them, so a rule saved as `feedback` draws more

## [04]-[BOUNDARIES]

Files hold one subject, and each fact lives in the narrowest file serving it:
- Subjects name a thing in the field (an app's runtime, a decision, a resource), never a method of finding things (tooling, notes, reads)
- Homes: a fact stated twice keeps one home, and the other file links `[[<slug>]]` with a clause naming what that file holds
- Merge: files co-loading on most of each other's should-load requests in `[07]` hold one subject
- Split: facts serving disjoint requests part at a subject into named siblings, never numbered parts, and a file past 4096 bytes splits or trims
- Stems: `<field>-<subject>[-<facet>]` equal to `name`, siblings sharing facets, cross-field rules named as the rule, no type, date, or status
- Routing: knowledge one kind of task uses alone belongs in that task's skill reference or a path-scoped `.claude/rules/` file, moved with the owner's approval

## [05]-[DESCRIPTIONS]

Descriptions state which situations need the file in one line of at most 150 characters, the situation inside the first 90, as the harness's fallback index keeps about that much after the link:
1. Requests: 3 should-load requests as the owner types them (direct, symptom, need unnamed), 3 near misses naming the sibling owning each
2. Near miss: a request stays a near miss only when no fact in the body changes its answer, else it is a should-load
3. Rules: should-load requests for a `feedback` or `user` file ask what its index line cannot settle, as the line itself carries the rule
4. Situation: open with the act or question every should-load request shares, in request words (verb, tool, error text)
5. Bound: state how the file differs from its nearest sibling, the term a near miss lacks (host, scope, identifier)
6. Gist: close with what the file gives, and a rule's imperative itself, as the index line acts without opening the file
7. Strip: values, counts, history, keyword lists, owner framing (`Owner runs <X>`), and words any session matches (`work`, `status`, `notes`)
8. Prove: the requests form the file's recall test, stored by the reference that wrote them

Index proxy runs over a live folder measured the form (tried 2026-11-26): description lines opened near misses 20 of 108 times against 41 for compressed hooks, with every should-load request opened.

## [06]-[EVIDENCE]

Every fact ends with one mark, and a mark opening a group covers each line under it:
- `(tried YYYY-MM-DD)` ran and seen, naming the command, and the build for host behavior
- `(sourced: <path[:line] | URL@version>)` read from a file, doc, binary, or package at the version an evaluate pass rechecks
- `(stated YYYY-MM-DD)` the owner said it, or `(stated by YYYY-MM-DD, file birth time)` when the day is lost
- `Inferred:` opens the writer's own conclusion, holds no mark, and names what would settle it
- Scope stays at its evidence: one request reads `the owner asked for <X> on <task>`, and a standing preference needs the owner's default wording
- Cautions name the cause they guard against, so a reader tests whether it still holds
- Owner statements change by a later owner statement alone, newer evidence replaces a value in place, and `CLAUDE.md` wins a contradiction
- Claims disagreeing with no later correction go to the owner through `AskUserQuestion` when found

## [07]-[INDEX_AND_PROOF]

`scripts/index.py` derives `MEMORY.md` whole from the files, and `scripts/recall.py` proves the set reaches the right files:
- Lines: `- [<stem>](<stem>.md) — <description>`, flat and sorted by stem, written only when the bytes change, then committed to the folder's git repository
- Budget: the index loads 200 lines or 25,000 characters, the set stays under 160 lines and 20,000, and merges free space before trims
- Validation: a refused file keeps its last good line, or a bare `- [<stem>](<stem>.md)` line, and the plugin reports it back to the session
- Requests: `.recall.jsonl` rows, unread by the memory scan, are `{"kind":"should","request":"<as typed>","owner":"<stem>"}` or near rows adding `"near":"<stem tested>"`
- Scope: a run covers rows naming a file changed since git tag `recall`, 3 sessions each, a no-memory baseline grading each miss
- Verdicts: hit (owner opened), miss (owner unopened), co-load (another file beside the owner), steal (another file in its place)
- Pass: no miss and no steal, as a co-load costs one extra read and its pairs feed the `[04]` merge test
