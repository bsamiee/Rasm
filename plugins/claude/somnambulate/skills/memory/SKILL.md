---
name: memory
description: "Use when writing, editing, merging, splitting, or judging a Claude Code auto-memory file or rebuilding its MEMORY.md index, covering admission, kinds, file boundaries, index lines, recall descriptions, evidence marks, and recall checks"
---

# [SOMNAMBULATE_MEMORY]

Memories hold what no later session reads back from a source: owner rulings and corrections with their reason, facts an investigation found that a source lacks or contradicts, and pointers to outside resources. Harness memory instructions state frontmatter, the four types, `**Why:**` and `**How to apply:**` lines, `[[name]]` links, and absolute dates; this skill holds recall mechanics, the system every memory follows, and its checks.

## [01]-[RECALL]

Each part of a memory file serves one reader:
- Index: session start loads `MEMORY.md` up to 200 lines or 25 KB, its lines the only memory text every session sees, and an agent opens a file a line points at
- Selector: where the host enables it, Sonnet reads `- [<type>] <file> (<mtime>): <description>` for the 200 newest files and takes up to 5 it is certain help
- Gate: `rg -c '"type":"relevant_memories"' <project-dir>/*.jsonl` shows whether the selector ever ran, as a host flag can leave it off and the index alone serve
- Body: a recalled file arrives cut at 4096 bytes or 200 lines, frontmatter included, and recall stops near 60 KB a session
- Subagents load neither index nor recall unless forked, so a brief names each memory path its work needs

## [02]-[ADMISSION]

Each candidate line passes every test in order, and the first failure sends it to the home in its row:

| [INDEX] | [TEST]     | [QUESTION]                                                         | [ON_FAIL]                             |
| :-----: | :--------- | :----------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Derivable  | One read of code, git, instructions, docs, skill, or tool gives it | Link the path, keep what overturns it |
|  [02]   | Durable    | Matters a week from now, in a session on another task              | Task list, plan, commit body          |
|  [03]   | Acting     | Names the future action it changes, in one clause                  | Delete                                |
|  [04]   | Procedure  | Holds a trigger with ordered steps                                 | Skill running that step               |
|  [05]   | Checked-in | States a convention a tool or teammate reads from the repository   | Owning repository file, via the owner |
|  [06]   | Evidenced  | Comes from the owner, a run, or a source read                      | Verify first, or delete               |
|  [07]   | Home       | Belongs to no existing memory's subject                            | That memory, edited in place          |

Passing lines rank by the owner time they save: corrections and confirmed choices, rulings with the condition releasing them, results overturning an assumption or a source, settings flipped more than twice, then locations outside the repository. Status, progress, completed work, open-probe lists, unbuilt plans, values one run printed, and conclusions no run confirmed fail `[02]` in any wording.

## [03]-[KINDS]

Kind follows what the knowledge is, and sets the lead line, the parts after it, and how the file changes:

| [INDEX] | [KIND]     | [TYPE]      | [LEAD_LINE]                    | [PARTS]                  | [CHANGES_BY]                |
| :-----: | :--------- | :---------- | :----------------------------- | :----------------------- | :-------------------------- |
|  [01]   | Rule       | `feedback`  | Condition, then the imperative | Why, How to apply, `Not` | Sharpening the one rule     |
|  [02]   | Profile    | `user`      | Trait in the scope stated      | Work it changes          | Replacing in place          |
|  [03]   | Decision   | `project`   | Choice, decider, date          | Why, `Holds until:` end  | Superseding with both dates |
|  [04]   | Fact sheet | `reference` | Answer most requests need      | Groups in work order     | Correcting each fact        |
|  [05]   | Pointer    | `reference` | Location and what it holds     | When to open it          | Moving with its target      |

Kinds draw from one set of parts, and content decides which appear:
- Leads stand alone, each later line narrows or proves its lead, and a one-line rule with nothing to add is complete
- Why names an incident, constraint, or mechanism the lead lacks, and a Why restating the rule or the owner's authority goes
- `Not: <wrong> -> <right>` lines appear once a mistake recurs under the rule, at most 2, ending on the right form
- Bodies hold current understanding rewritten in place, facts name their subject over a pronoun, and a dead end reads `<form> fails because <cause>`

## [04]-[BOUNDARIES]

Files hold one load set, the facts one kind of request needs together:
- Merge: files loaded together for each other's requests become one, as each spends one of 5 selector slots
- Split: facts serving disjoint requests, or a file past 4096 bytes, part at a subject (app, tool, decision), never a phase or numbered part
- Homes: each fact sits in the narrowest file serving it, and others link `[[<slug>]]` with a clause naming what that file holds
- Stems: `<field>-<subject>[-<facet>]` equal to `name`, siblings sharing facets, cross-field rules named as the rule, no type, date, or status

## [05]-[INDEX]

`MEMORY.md` derives whole from the files and regenerates after every create, edit, merge, split, rename, or delete, so a hand-edited line never drifts from its file and every rebuild yields the same index:
- Lines: `- [<Title>](<file>) — <hook>` at most 150 characters, one per file, the title spelling the stem in words
- Groups: `## Rules`, `## Profile`, `## Decisions`, `## Reference` from `metadata.type`, in that order, lines sorted by stem so field stems cluster
- Hooks compress the lead line in request words: a rule's condition and imperative, a decision's state, a fact sheet's questions, a pointer's location

## [06]-[DESCRIPTIONS]

Descriptions predict which requests need the file, and selector runs over a real folder showed situation-and-bound lines drawing no near-miss load in 15 runs against 14 of 21 for keyword, label, or abstract lines, a specific stem or description loading a file alone, and owner framing loading near misses sharing its noun (tried 2026-11-25):
1. Requests: 3 should-load requests as the owner types them (direct, symptom, need unnamed), and 3 near misses from sibling files
2. Situation: open with the act or question every should-load request shares, in request words (verb, tool, error text)
3. Bound: add the term separating should-load from near misses (host, scope, identifier, phase), as field words alone draw the whole field
4. Gist: close with what the file gives (the rule, a decision and its state, the questions a fact sheet answers)
5. Strip: delete stem words, values, counts, history, and words any session matches (`work`, `status`, `notes`), and fit 25 words, 30 at most
6. Prove: run `[08]`, a missed load adding the request's own term and a near-miss load losing the word it matched

## [07]-[EVIDENCE]

Every fact ends with one mark, and a mark opening a group covers each line under it:
- `(tried YYYY-MM-DD)` ran and seen, naming the command or observation where the line lacks it
- `(sourced: <path[:line] | URL@version>)` read from a file, doc, binary, or package, rechecked when that version moves
- `(stated YYYY-MM-DD)` the owner said it, or `(stated by YYYY-MM-DD, file birth time)` when the day is lost
- `Inferred:` opens the writer's own conclusion, holds no mark, and names what would settle it
- Scope stays at its evidence: one request reads `the owner asked for <X> on <task>`, and a standing preference needs the owner's default wording
- Owner statements change by a later owner statement alone, newer evidence replaces a value in place, and `CLAUDE.md` wins a contradiction
- Claims repeated across sessions count once, and claims with no later correction become `Conflict: <A> (<mark>) vs <B> (<mark>)` for the owner

## [08]-[CHECK]

Checks run each request 3 times from `$TMPDIR` against the folder as it stands: the selector check replays the installed selector prompt and passes at 3 of 3 loads per should-load request and 0 of 3 per near miss, and the index check, a proxy for the session model reading `MEMORY.md`, passes when the file is named for each should-load request its hook leaves open and for no near miss:

```bash
strings -n 8 "$(readlink -f "$(command -v claude)")" | sed -n '/^You are selecting memories/,/selected_memories": \[/p' > <scratch>/selector.txt
for f in $(ls -t <memory-dir>/*.md | rg -v '/MEMORY\.md$' | head -200); do printf -- '- [%s] %s (%s): %s\n' "$(yq --front-matter=extract '.metadata.type' "$f")" "$(basename "$f")" "$(date -u -r "$f" +%FT%TZ)" "$(yq --front-matter=extract '.description' "$f")"; done > <scratch>/manifest.txt
printf 'Available memories:\n%s\n\nSelect memories relevant to:\n%s\n' "$(cat <scratch>/manifest.txt)" '<request>' | CLAUDE_CODE_DISABLE_AUTO_MEMORY=1 claude -p --model sonnet --system-prompt "$(cat <scratch>/selector.txt)" --tools "" --strict-mcp-config --no-session-persistence --output-format json --json-schema '{"type":"object","properties":{"selected_memories":{"type":"array","items":{"type":"string"}}},"required":["selected_memories"]}' | jq -c '.[-1].structured_output.selected_memories'
CLAUDE_CODE_DISABLE_AUTO_MEMORY=1 claude -p --tools "" --strict-mcp-config --no-session-persistence --system-prompt "$(cat <memory-dir>/MEMORY.md)" 'List the memory files you would open before acting, or none: <request>'
```

## [09]-[CREATE]

1. Run the candidate through `[02]`, owner steering first, then search `rg -il '<term>' <memory-dir>` for subject, synonyms, tool, and error text, a covering file moving the work to `[10]`
2. Pick the kind `[03]`, name the file `[04]`, write lead and needed parts, and mark each fact `[07]`
3. Derive the description `[06]` and index line `[05]` from the finished body, then run `[08]` with the draft in place
4. Preview file, description, and index line through `AskUserQuestion` with a free-response option, then write within 4096 bytes and rebuild the index

## [10]-[EDIT]

1. Read the file whole, its index line, and every linker from `rg -l '\[\[<slug>\]\]' <memory-dir>`, asking once first when another session wrote it minutes ago
2. Retest boundaries `[04]`: a merge rereads the kept file before the other goes, a rename keeps harness fields and repoints `[[<old>]]`
3. Rebuild description and index `[05]` from the new body, rerunning `[08]` with the requests the old description served

## [11]-[EVALUATE]

Evaluations read `MEMORY.md` and each file whole, run `wc -c <memory-dir>/*.md` against 4096, and list unresolved links with `comm -23 <(rg -o '\[\[([^]]+)\]\]' -r '$1' -N --no-filename *.md | sort -u) <(ls *.md | sed 's/\.md$//' | sort)`. Each row passes or fails, a fail on `Admission` deletes or folds the file, and every other fail takes its fix in the same pass:

| [INDEX] | [CRITERION] | [PASS]                                      | [FAIL]                                         |
| :-----: | :---------- | :------------------------------------------ | :--------------------------------------------- |
|  [01]   | Admission   | Every line passes `[02]`                    | Task state, a run's values, or restated source |
|  [02]   | Kind        | One kind, lead acts alone, parts earn place | Mixed kinds, empty slots, Why restating        |
|  [03]   | Boundary    | One load set, one home, links resolving     | Co-loads, broken link, tail past 4096 bytes    |
|  [04]   | Recall      | Both `[08]` checks pass, hook acts unopened | Missed load, near-miss load, topic-only hook   |
|  [05]   | Evidence    | Every fact marked, exact identifiers        | Unmarked, overclaimed, paraphrased             |
|  [06]   | Current     | Agrees with `CLAUDE.md`, siblings, sources  | Superseded value, dead path, retired name      |
|  [07]   | Index       | Matches a rebuild from the files by `[05]`  | Missing, stale, or hand-grouped line           |
