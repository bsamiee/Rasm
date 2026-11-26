---
name: memory
description: "Use when writing, editing, merging, splitting, or judging a Claude Code auto-memory file or its MEMORY.md line, covering admission, kinds, file boundaries, names, recall descriptions, evidence marks, and the selector check"
---

# [SOMNAMBULATE_MEMORY]

Memories hold what no later session reads back from a source: owner rulings and corrections with their reason, facts an investigation found that a source lacks or contradicts, and pointers to outside resources. Harness memory instructions state frontmatter, the four types, `**Why:**` and `**How to apply:**` lines, `[[name]]` links, index line form, and absolute dates; this skill holds recall mechanics, the system every memory follows, and its checks.

## [01]-[RECALL]

Each part of a memory file serves one reader:
- Index: session start loads `MEMORY.md` up to 200 lines or 25 KB, its hooks the only memory text every session sees, and an agent opens a file a hook points at
- Selector: where the host enables it, Sonnet reads `- [<type>] <file> (<mtime>): <description>` for the 200 newest files and takes up to 5
- Matching: Sonnet picks files it is certain help by what a request is about, never shared words, skipping its earlier picks
- Gate: `rg -c '"type":"relevant_memories"' <project-dir>/*.jsonl` shows whether the selector ran, as a host flag can leave it off
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
- Fact sheet groups open `**<Group>:**` in the subject's own terms, ordered as work meets them, never a set copied from a sibling
- Bodies hold current understanding rewritten in place, facts name their subject over a pronoun, and a dead end reads `<form> fails because <cause>`

## [04]-[BOUNDARIES]

Files hold one load set, the facts one kind of request needs together:
- Merge: files loaded together for each other's requests become one, as each spends one of 5 selector slots
- Split: facts serving disjoint requests, or a file past 4096 bytes, part at a subject (app, tool, decision), never a phase or numbered part
- Homes: each fact sits in the narrowest file serving it, and others link `[[<slug>]]` with a clause naming what that file holds
- Stems: `<field>-<subject>[-<facet>]` equal to `name`, siblings sharing facets, cross-field rules named as the rule, no type, date, or status
- Index lines: `- [<Title>](<file>) — <hook>` under `## <field>`, 150 characters at most, the title spelling the stem
- Hooks state what a session acts on unopened (the rule, a decision's state, the questions a fact sheet answers), never a copied description

## [05]-[DESCRIPTIONS]

Descriptions predict which requests need the file, written from requests that exist before the line:
1. Requests: 3 should-load requests as the owner types them (direct, symptom, need unnamed), and 3 near misses from sibling files
2. Situation: open with the act or question every should-load request shares, in request words (verb, tool, error text)
3. Bound: add the term separating should-load from near misses (host, scope, identifier, phase), as field words alone draw the whole field
4. Gist: close with what the file gives (the rule, a decision and its state, the questions a fact sheet answers)
5. Strip: delete stem words, values, counts, history, and words any session matches (`work`, `status`, `notes`), and fit 25 words, 30 at most
6. Prove: run `[07]`, a missed load adding the request's own term and a near-miss load losing the word it matched

Selector runs over a real memory folder showed the cost of each form (tried 2026-11-25): situation-and-bound lines drew no near-miss load in 15 runs against 14 of 21 for keyword, label, or abstract lines; a specific stem or a specific description each loads a file alone, and both vague load it for nothing; owner framing (`Owner runs <X>`) loads near misses sharing `<X>`.

## [06]-[EVIDENCE]

Every fact ends with one mark, and a mark opening a group covers each line under it:
- `(tried YYYY-MM-DD)` ran and seen, naming the command or observation where the line lacks it
- `(sourced: <path[:line] | URL@version>)` read from a file, doc, binary, or package, rechecked when that version moves
- `(stated YYYY-MM-DD)` the owner said it, or `(stated by YYYY-MM-DD, file birth time)` from `stat -f %SB` when the day is lost
- `Inferred:` opens the writer's own conclusion, holds no mark, and names what would settle it
- Scope stays at its evidence: one request reads `the owner asked for <X> on <task>`, and a standing preference needs the owner's default wording
- Owner statements change by a later owner statement alone, newer evidence replaces a value in place, and `CLAUDE.md` wins a contradiction
- Claims repeated across sessions count once, and claims with no later correction become `Conflict: <A> (<mark>) vs <B> (<mark>)` for the owner

## [07]-[CHECK]

Checks replay the installed selector prompt over the folder's manifest from `$TMPDIR`, each request 3 times, passing at 3 of 3 loads per should-load request and 0 of 3 per near miss, and scratch files go once the verdict is recorded:

```bash
strings -n 8 "$(readlink -f "$(command -v claude)")" | sed -n '/^You are selecting memories/,/selected_memories": \[/p' > <scratch>/selector.txt
for f in $(ls -t <memory-dir>/*.md | rg -v '/MEMORY\.md$' | head -200); do printf -- '- [%s] %s (%s): %s\n' "$(yq --front-matter=extract '.metadata.type' "$f")" "$(basename "$f")" "$(date -u -r "$f" +%FT%TZ)" "$(yq --front-matter=extract '.description' "$f")"; done > <scratch>/manifest.txt
printf 'Available memories:\n%s\n\nSelect memories relevant to:\n%s\n' "$(cat <scratch>/manifest.txt)" '<request>' | CLAUDE_CODE_DISABLE_AUTO_MEMORY=1 claude -p --model sonnet --system-prompt "$(cat <scratch>/selector.txt)" --tools "" --strict-mcp-config --no-session-persistence --output-format json --json-schema '{"type":"object","properties":{"selected_memories":{"type":"array","items":{"type":"string"}}},"required":["selected_memories"]}' | jq -c '.[-1].structured_output.selected_memories'
```

## [08]-[CREATE]

1. Run the candidate through `[02]`, owner steering first, then search `rg -il '<term>' <memory-dir>` for subject, synonyms, tool, and error text, a covering file moving the work to `[09]`
2. Pick the kind `[03]`, name the file `[04]`, write lead and needed parts, and mark each fact `[06]`
3. Derive the description `[05]` and hook `[04]` from the finished body, then run `[07]` with the draft in the manifest
4. Preview file, description, and hook through `AskUserQuestion` with a free-response option, then write both within 4096 bytes, links resolving

## [09]-[EDIT]

1. Read the file whole, its index line, and every linker from `rg -l '\[\[<slug>\]\]' <memory-dir>`, asking once first when another session wrote it minutes ago
2. Retest boundaries `[04]`: merges write every fact into the kept file and reread it before the other goes, splits give each part its own description
3. Renames carry harness fields (`node_type`, `originSessionId`, `modified`), repoint every `[[<old>]]`, and replace the index line
4. Rebuild description and hook from the new body, rerunning `[07]` with the requests the old description served

## [10]-[EVALUATE]

Evaluations read `MEMORY.md` and each file whole, run `wc -c <memory-dir>/*.md` against 4096, and list unresolved links with `comm -23 <(rg -o '\[\[([^]]+)\]\]' -r '$1' -N --no-filename *.md | sort -u) <(ls *.md | sed 's/\.md$//' | sort)`. Each row passes or fails, a fail on `[01]` or `[02]` deletes or folds the file, and every other fail takes its fix in the same pass:

| [INDEX] | [CRITERION] | [PASS]                                      | [FAIL]                                         |
| :-----: | :---------- | :------------------------------------------ | :--------------------------------------------- |
|  [01]   | Durable     | Changes work in later sessions              | Task state, progress, or a run's values        |
|  [02]   | Underivable | Memory alone holds it, pointing at owners   | Restates code, git, docs, `CLAUDE.md`, a skill |
|  [03]   | Kind        | One kind, lead acts alone, parts earn place | Mixed kinds, empty slots, Why restating        |
|  [04]   | Boundary    | One load set, one home per fact, 4096 bytes | Co-loads with a sibling, tail past the cut     |
|  [05]   | Recall      | `[07]` passes, hook acts unopened           | Missed load, near-miss load, topic-only hook   |
|  [06]   | Evidence    | Every fact marked, exact identifiers        | Unmarked, overclaimed, paraphrased             |
|  [07]   | Current     | Agrees with `CLAUDE.md`, siblings, sources  | Superseded value, dead path, retired name      |
|  [08]   | Links       | Each resolves and names what it points at   | Broken, bare, or silent overlap                |
