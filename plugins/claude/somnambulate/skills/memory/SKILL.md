---
name: memory
description: "Use when saving, changing, or auditing a Claude Code auto-memory file or the memory set, covering admission, kinds, general and specific memories, links, names, descriptions that double as index lines, evidence marks, and recall proof"
---

# [SOMNAMBULATE_MEMORY]

This skill is loaded. Your next call Reads the reference for the work, before any read, search, or write in the memory folder, and its steps replace your own plan; Read it again after a compaction: a memory that does not exist yet -> `references/create.md`, a change inside one existing memory -> `references/edit.md`, the set or any merge, split, rename, or delete -> `references/evaluate.md`. `MEMORY.md` is generated from each file's `description` after every memory write, so no step writes it.

[REFERENCES]:
- [01]-[CREATE](references/create.md): Saving a new memory file
- [02]-[EDIT](references/edit.md): Improving one existing memory file
- [03]-[EVALUATE](references/evaluate.md): Auditing the memory set, and merging, splitting, renaming, or deleting files

[SCRIPTS]:
- [01]-[INDEX](scripts/index.py): Regenerates `MEMORY.md` and commits the folder when a memory file changes, run by the plugin
- [02]-[RECALL](scripts/recall.py): Replays stored requests through live sessions with and without the memory folder and grades each reply

## [01]-[RECALL]

Claude Code recalls memory in an index mode or a selector mode, and every file serves both:
- Mode: `jq '.cachedGrowthBookFeatures.tengu_moth_copse' ~/.claude.json` true, or `CLAUDE_MEMORY_STORES` set by a remote memory store, means selector mode, otherwise index mode
- Index mode: session start loads `MEMORY.md` up to 200 lines or 25KB after stripping frontmatter, its lines usually the only memory text a session acts on
- Opening: a session Reads a file whole only when its index line sends it there, so that line holds what a session acts on and what opening adds
- Selector mode: `MEMORY.md` never loads, and per prompt a Sonnet call picks up to 5 files it is certain help from `- [<type>] <file> (<mtime>): <description>`
- Description: the one text both modes read, as the selector's manifest line and as the generated index line
- Body: selector mode injects a picked file cut at 4096 bytes, so the lead and the facts most requests need come first in every file
- Links: neither mode resolves `[[<stem>]]`, and a session Reads a linked file only when the link clause names what that file answers
- Caching: each session and its subagents, against the docs, keep their starting index until compaction, `/clear`, or the plugin's refresh (tried 2026-11-26)
- Other sessions: running sessions keep their own copy, so a rename strands their links until they restart, and renames wait for a quiet pass
- Outside writers: harness save steps, dream, and extraction write memory without this skill, and the plugin restores the index after each

## [02]-[ADMISSION]

Each candidate line passes every test in order, and the first failure sends it to the home in its row:

| [INDEX] | [TEST]     | [QUESTION]                                                       | [ON_FAIL]                                         |
| :-----: | :--------- | :--------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Derivable  | One read of a tracked file, tool, doc, or skill gives it         | Delete, keeping incidents of it broken            |
|  [02]   | Durable    | Matters a week from now, in a session on another task            | Task list, plan, commit body                      |
|  [03]   | Acting     | Names the future action it changes, in one clause                | Delete                                            |
|  [04]   | Procedure  | Holds ordered steps a skill runs                                 | Skill running them, a correction staying here too |
|  [05]   | Checked-in | States a convention a tool or teammate reads from the repository | Owning repository file, via the owner             |
|  [06]   | Evidenced  | Comes from the owner, a run, or a source read                    | Verify first, or delete                           |
|  [07]   | Home       | Belongs to no existing general or specific memory                | Memory `[04]` places it in, edited in place       |

Lines whose clauses take different verdicts split into one line per clause before the tests run. Status, progress, completed work, open-probe lists, plans no owner decided, values one run printed, and conclusions no run confirmed fail `[02]` in any wording.

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

Knowledge sits in a general and a specific layer, and each fact lives once, in the memory every request needing it opens:

[LAYERS]:
- General memory: holds what stays true across the subjects of one field or concern (a mechanism, a probe form, a rule), a `<placeholder>` where values differ
- Subsets: a fact some subjects share sits in the general memory under a condition naming their shared trait, never a list of the subjects
- Specific memory: holds its subject's own facts word for word, the value each `<placeholder>` takes for it, and where it departs from its general memory
- Generality: a fact is general when it stays true with its subject swapped for any sibling's, or when requests about 2 subjects need it
- Warrant: a general memory exists once 2 or more specific memories need one set of facts, else the facts stay in the one specific memory
- Order: shared facts enter their general memory first and specific memories build on it, so none holds what a sibling also needs
- Content: a general memory holds facts and never a list of its specific memories, as the index shows each field's files under one heading

Anthropic's wiki trials found a shared table of per-subject values dropped 16 of 57 facts that one page per subject, quoting each value, kept (sourced: anthropic.com/research/components-of-agent-memory).

[LINKS]:
- Link clause: `[[<stem>]]` plus the words naming what that file answers, the one form every link takes
- Direction: links run from a specific memory to any general memory a line needs, a general names no specific, and a sibling link marks a relied fact
- Placement: the clause stands at the line needing the linked fact, in place of that fact, never as an opening routing line or a closing list
- Following: a session weighs the clause's words to decide whether a Read pays, so a clause names the question its target answers, never `see also`
- Silence: a specific memory restates nothing its general memory holds, neither summary, value, caution, nor example, and keeps its own facts alone
- Resolution: `scripts/index.py` refuses a link naming no file, and a rename repoints every `[[<old>]]` in the same pass

[NAMES]: Stems equal `name` in kebab-case from the field's own words, so a specific memory's stem shows its general memory:

| [INDEX] | [MEMORY]      | [STEM]                         |
| :-----: | :------------ | :----------------------------- |
|  [01]   | General       | `<field>-<concern>`            |
|  [02]   | Specific      | `<field>-<subject>-<concern>`  |
|  [03]   | Whole subject | `<field>-<subject>`            |
|  [04]   | Rule          | `[<field>-]<rule as a phrase>` |

Each part of a stem keeps one meaning across the set:
- Field: the area's own word (a vendor, a tool, an OS), shared by every stem of that area, as the index heads each field's group with it
- Concern: one term per concern, hyphenated where the field writes it so (`native-mcp`), the same in its general memory and every specific memory under it
- Subject: one thing in the field (an app, a tool, a service), absent from a general memory's stem
- Whole subject: a memory with no concern split and no general memory above it
- Rule: a rule of one field opens with its field's word, one crossing fields takes none, and the phrase states the rule or its rules' shared condition
- Never: type, date, status, sequence number, a method as concern (`tooling`, `notes`, `facts`), or a stand-in subject (`host`, `common`, `core`)

[SIZE]: Files hold what their requests need together, neither a grab bag of subjects nor one file per fact:
- Extract: a fact 2 files state moves into its general memory, created when the warrant holds, and each file keeps a link clause where it relied on it
- Overlap: siblings stating one fact open together on each request about it, and extraction cures it where rewording only moved it (tried 2026-11-26)
- Merge: files whose requests each need the other's body hold one subject
- Split: facts serving disjoint requests part at a subject or concern into named files, never numbered, and a file past 4096 bytes splits or trims
- Fold: a specific memory holding nothing its general memory lacks folds into it, and a general memory serving one specific memory folds back
- Routing: knowledge one kind of task uses alone belongs in that task's skill reference or a path-scoped `.claude/rules/` file, with the owner's approval

## [05]-[DESCRIPTIONS]

Descriptions state which situations need the file in one line of at most 150 characters, the situation inside the first 90, as the harness's fallback index keeps about that much after the link:
1. Requests: 3 should-load requests as the owner types them (direct, symptom, need unnamed), beside the nearest sibling's requests the line leaves out
2. Situation: open with the act or question every should-load request shares, in request words (verb, tool, error text)
3. Layer: a general memory's situation names its concern by the concern's identifiers, never its whole field, which every request there matches
4. Silence: a specific memory's line omits identifiers its general memory owns, so a request on the shared concern opens the general memory alone
5. Bound: state how the file differs from its nearest sibling, the term the sibling's requests lack (host, scope, identifier)
6. Gist: close with what the file gives
7. Rules: a `feedback` or `user` line reads `<situation>: <imperative>; <what the body answers>`, as sessions act on the imperative and open for an answer
8. Answers: the last part names in request words the cases, proofs, exceptions, or forms the body holds past the rule, as should-load requests ask
9. Strip: values, counts, history, keyword lists, owner framing (`Owner runs <X>`), and words any session matches (`work`, `status`, `notes`)
10. Prove: the should-load requests become the file's rows in `.recall.jsonl`, written by the reference that wrote the file

Rule lines carrying the imperative alone drew most misses of a full replay, as each request needed a body answer the line never named (tried 2026-11-26).

## [06]-[EVIDENCE]

Every fact ends with one mark, and a mark opening a group covers each unmarked line under it:
- `(tried YYYY-MM-DD)` ran and seen, naming the command and the build for host behavior, a recheck on a newer build moving both, `tried by` with file birth time
- `(sourced: <path[:line] | URL@version>)` read from a tracked file, doc, binary, or package at the version an evaluate pass rechecks
- `(stated YYYY-MM-DD)` the owner said it, listing each date of statements joined into one rule, or `(stated by YYYY-MM-DD, file birth time)` when the day is lost
- `Inferred:` opens the writer's own conclusion, holds no mark, and names what would settle it
- Scope stays at its evidence: one request reads `the owner asked for <X> on <task>`, and a standing preference needs the owner's default wording
- Cautions name the cause they guard against, so a reader tests whether it still holds
- Owner statements change by a later owner statement alone and keep their wording where they move, a clause for a general splitting out on approval
- Newer evidence replaces a value in place, and `CLAUDE.md` or a loaded skill wins a contradiction until evidence corrects that file
- Claims disagreeing with no later correction go to the owner through `AskUserQuestion` when found

## [07]-[INDEX_AND_PROOF]

`scripts/index.py` derives `MEMORY.md` whole from the files, `scripts/recall.py` proves the set improves answers, and `claude plugin eval` proves the skill:
- Lines: `- [<stem>](<stem>.md) — <description>` under one `## <field>` heading per stem's first word, headings and lines sorted, committed when the bytes change
- Budget: the index loads 200 lines or 25KB, headings included, the set stays under 160 lines and 20,000 bytes, and merges free space before trims
- Validation: a refused file keeps its last good line, or a bare `- [<stem>](<stem>.md)` line, and the plugin reports it back to the session
- Requests: `.recall.jsonl` rows, unread by the memory scan, are `{"request":"<as typed>","owner":"<stem>"}`, each a question the owner's body answers
- Scope: a run covers rows naming a file changed since git tag `recall`, `RECALL_RUNS` sessions each (3 by default) with the memory folder and without it
- Turns: 3 per session, so a session can Read a specific memory, follow its link clause, and still answer
- Grade: a judge scores whether each reply applies what the owner's body says, and a row's delta is its score with memory less its score without
- Trend: each run stores row, file, and total scores under `.recall-results/` in the folder's history and prints deltas against the last run
- Pass: every row scores higher with memory, as an even score marks a derivable fact, an unopened body, or a link clause no session followed
- Skill: `claude plugin eval` over the plugin's `evals/` cases scores create, edit, and evaluate work with and without the plugin, as its runs load no memory folder
