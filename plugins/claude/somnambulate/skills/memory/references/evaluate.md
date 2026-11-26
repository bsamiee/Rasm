# [EVALUATE]

Evaluations judge the memory set as a system and rebuild it from its files' facts, moving knowledge more than one file needs into one general memory and cleaning each specific memory of it.

## [01]-[READINGS]

Readings precede every judgment, each from the tool that records it:
1. Change record: `git -C <memory-dir> log --stat <tag>..`, with `<tag>` from the first command below, or the whole log when no tag exists
2. Structure: `scripts/index.py <memory-dir>` reports sizes, links, frontmatter, and index budget, and changes nothing in a current folder
3. Identifiers: the second command lists each backticked identifier more than one file holds, with those files
4. Bodies: each cluster's files read side by side, as restated facts share no identifier as often as they share one
5. Lines: each index line read against 3 requests its body answers, as a new session sees the index alone

```bash
git -C <memory-dir> describe --tags --match 'evaluate-*' --abbrev=0
rg -o --with-filename --no-line-number '`[^`]+`' --glob '!MEMORY.md' <memory-dir> | sort -u \
  | awk -F'.md:' '{n[$2]++; f[$2] = f[$2] " " $1} END {for (k in n) if (n[k] > 1) print n[k] "\t" k "\t" f[k]}' | sort -rn
```

Signals point at a cause and its remedy:

| [INDEX] | [SIGNAL]                                     | [CAUSE]                                 | [REMEDY]                                     |
| :-----: | :------------------------------------------- | :-------------------------------------- | :------------------------------------------- |
|  [01]   | Rule line carrying the imperative alone      | Sessions act on the line, never open it | Rule-file reading below                      |
|  [02]   | Line missing the words of requests it serves | File never opened for them              | Description rewritten by `SKILL.md` `[05]`   |
|  [03]   | Link clause naming no question               | Session skips the Read                  | Clause names the question its target answers |
|  [04]   | Fact one read of a tracked file gives        | Stored twice                            | Line deleted by `SKILL.md` `[02]`            |
|  [05]   | Mark citing an older build or moved path     | Fact possibly stale                     | Fact rechecked by `SKILL.md` `[06]`          |
|  [06]   | Identifier shared across files               | One fact stated twice                   | Cluster edge                                 |
|  [07]   | Fact restated across siblings                | Cross-cutting fact held twice           | Extraction into a general memory             |

Rule files whose index line carries the rule draw sessions to act on the line and never open the body. Reading the file's requests against the body decides the fix:
- Body answers what the line cannot -> description names that content (cases, scope, `Not:` lines) after the rule
- Line alone answers -> body trimmed to the line, as a one-line rule is complete
- Rewording the rule to draw more opens repeats the defect, and the rule keeps its wording

Shared identifiers and facts restated across siblings share one cause: cross-cutting facts sitting in more than one sibling. Extraction into a general memory with link clauses cures them, and rewording descriptions moves them between files. A fact a specific memory's own requests always need stays there, as a session reaches a general memory only by choosing to follow a link.

## [02]-[CLUSTERS]

Clusters group files that hold or need the same facts, built from readings over stem prefixes:
- Edges: a shared identifier, a fact both bodies state, or a sibling one file needs to answer its own rows
- Confirmation: each edge holds when reading both bodies names the shared fact, and an edge with no shared fact goes
- Crossed families: files crossing a subject axis with a concern axis (app by setting) place each fact where its value varies
- Axis rule: a per-subject value stays in its subject's file, and a fact true of every subject, or of some by a shared trait, goes to the general
- Few rows: a subject's few rows of a concern join its file nearest the concern under a group named for it, else stay in the concern's file
- Existing generals: a file already holding the cluster's shared facts becomes its general memory, renamed by `[05]` when its stem names no concern
- Over-broad files: a broad file outside any cluster holds another subject's facts, which move to that subject's file
- Independence: clusters sharing no file rebuild apart, each by its own agent holding its ledger and this reference

## [03]-[REBUILD]

Rebuilds run raw content before judgment and general memories before specifics, so no fact goes missing or lands twice:
1. Clone: `git clone -q <memory-dir> <scratch>/memory`, every change made there, as live sessions read the folder each turn
2. Ledger: every fact line of each cluster member copied verbatim to `<scratch>/ledger/<cluster>.md` before any rewrite, split per clause by `SKILL.md` `[02]`
3. Rows: one per line, holding source `<file>:<line>`, mark, identifiers, and the requests it answers
4. Tests: each row through `SKILL.md` `[02]` in order, recording its first failed test and that test's home
5. Toss: rows failing with no home are deleted, and rows with a home outside memory join the plan as routes
6. Evidence: each kept row's mark checked by `SKILL.md` `[06]`, sourced versions against installed ones, claims against `CLAUDE.md`
7. Duplicates: rows stating one fact become one, with the strongest mark, the newest evidence, and owner wording where it sets scope
8. Sort: rows more than one member states, or true of every member and needed by more than one, go general, and the rest stay with their member
9. Plan: clusters, general stems by `[05]`, per-file verdicts by `[04]`, deletions, and renames, approved at `[07]` before any write
10. General: written first by `SKILL.md` `[03]` and `[04]`, its lead the shared answer most requests need
11. Specifics: each rebuilt from its own rows with Edit, every shared fact deleted, one link clause placed in the group needing the general
12. Descriptions: each rewritten by `SKILL.md` `[05]`, a general bounded against its specifics and a specific by the subject its general lacks
13. Links: bare `Related:` lists replaced by link clauses or deleted, then `scripts/index.py <scratch>/memory` checks each resolves
14. Check: reading 5 of `[01]` over every changed file, a request its line misses sending the file back to step 12
15. Return: `git -C <memory-dir> pull --ff-only --no-tags <scratch>/memory main`
16. Refusal: a refused pull means another session wrote, so the clone rebases on it, rereads the new commits, and reruns step 14
17. Final: the `[08]` report, tag `evaluate-<YYYYMMDDTHHMMZ>`, and clone and ledgers deleted

Extraction deletes absorbed rows in the same pass, as a general memory beside unchanged specifics states every fact twice. Owner-stated rows keep their wording wherever they move.

## [04]-[MOVES]

Structural moves follow from the ledger, never from stem resemblance:

| [INDEX] | [MOVE]  | [LEDGER_SHOWS]                                   | [FORM]                                                          |
| :-----: | :------ | :----------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | Extract | Rows shared across members                       | General written first, members cleaned in one pass              |
|  [02]   | Move    | Row in a file whose subject owns none of it      | Written at its home, deleted at source, link clause if needed   |
|  [03]   | Merge   | Files with kept rows answering one request group | Folded into better stem, each row reread there, other deleted   |
|  [04]   | Split   | One file's kept rows answering disjoint groups   | Parts at a subject, each with own stem, description, and rows   |
|  [05]   | Rename  | Stem misnaming kept content, or failing `[05]`   | New stem, harness fields kept, links and row owners repointed   |
|  [06]   | Delete  | No row kept, or every row moved                  | File deleted, links repointed or removed, rows moved or deleted |
|  [07]   | Route   | Rows one kind of task alone uses                 | Skill reference or path-scoped rule, after owner approval       |

Harness fields of a merge come from the file whose stem survives, else from the largest source. Renames apply last, once `git -C <memory-dir> log --since=1.hour --format='%(trailers:key=Session,valueonly)'` names no other session, as running sessions keep their old index by `SKILL.md` `[01]`.

## [05]-[NAMES]

Names follow `SKILL.md` `[04]`, and the set adds these checks:
- General stems name the field and the concern their facts share in a request's words, never `common`, `shared`, `general`, or `misc`
- Specific stems open with their general's field word and name the subject that varies, a crossed family's cell naming subject and concern
- Siblings under one general share one concern word, and a concern no sibling shares gets renamed or folded within 4096 bytes, else stays its own file
- Catch-all concerns (`tooling`, `notes`) draw every request naming their subject, so their facts move to concerns a request names
- Stems naming one subject in synonyms merge into the stem a request uses

## [06]-[SET]

Sets hold together while each of these stays true after a pass:
- Index within the `SKILL.md` `[07]` budget, merges freeing lines before trims
- Files within 200, the selector's manifest cap, and each within 4096 bytes
- Every fact at one home and every link resolving
- Generals serving more than one specific, as a general serving one returns its facts to that file
- Specifics holding a fact of their own beyond their link clause, and a one-line rule with its own requests staying whole
- Link chains ending within 2 hops of any file, as each hop costs a read a session can skip

## [07]-[APPROVAL]

Owner decisions go through `AskUserQuestion`, each question explaining its options and offering a free response:
- Plan: clusters, each general's stem and outline, per-file verdicts, and every deletion, merge, and rename
- Generals: each new general memory before it is written, previewing body, description, and requests
- Owner statements: deleting one, changing its wording, or choosing between statements that disagree
- Routes and renames: moving a file out of memory, or renaming a stem the owner chose
- Headless: a session without `AskUserQuestion` writes the report with every preview and changes nothing until the owner asks

Passes the owner limits to a report run steps 1 to 8 in the clone, return nothing, and set no tag.

## [08]-[REPORT]

Reports open with a table of every file in scope, one row per file, as a report grouped by duplicated fact hides each file's verdict:
- Verdict: kept, edited, general (new), rebuilt on `<general>`, merged into, split into, moved, renamed to, or deleted
- Evidence: the readings and ledger rows deciding it (shared identifier, restated fact, ledger row)
- Rows: facts kept, moved to a general, and tossed

Sections after the table:
- Clusters: each general memory, its members, and the facts extracted
- Totals: files, bytes, index lines, and ledger rows kept, moved to a general, and tossed, before and after
- Open items: each naming its file and the reading or owner answer that closes it
