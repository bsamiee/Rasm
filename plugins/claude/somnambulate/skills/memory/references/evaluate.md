# [EVALUATE]

Evaluations judge the memory set as a system: which files exist, what each holds against the others, whether every fact has one home, and whether the index reaches the right file for each request.

## [01]-[AUDIT]

1. Read the change record with `git -C <memory-dir> log --stat <tag>..` from the `evaluate-YYYY-MM-DD` tag the last pass left, and the recall mode by `SKILL.md` `[01]`
2. Run `scripts/index.py <memory-dir>` for sizes, links, frontmatter, and index budget, as it changes nothing when the folder is current
3. Read every file whole against `SKILL.md` `[02]` admission, `[03]` kind and lead, and `[06]` evidence, checking cited paths and symbols with `rg` and `fd`
4. Recheck each claim against `CLAUDE.md`, and each `(sourced: …@<version>)` mark against the installed version, rewriting the fact or its mark when it moved
5. List facts stated in more than one file with `rg -o` over identifiers (paths, commands, keys, versions), each with the file that should hold it
6. Check files outside writers saved (harness save steps, dream, extraction) for marks and admission before trusting them
7. Run `scripts/recall.py <memory-dir>`, which scopes itself to files changed since its last pass, stating its session count before it starts

## [02]-[MOVES]

Structural moves change which files exist or which file holds a fact:
- Merge: files co-loading on most of each other's requests, or sharing a subject, fold into the better stem, every fact reread there first
- Split: facts serving disjoint request groups part at a subject, each part with its own stem, description, and requests, never numbered
- Move: a fact held twice is written into its home first, then deleted from the other file, which keeps a link clause where it depends on it
- Rename: a stem misnaming its subject takes a field term by `[04]`, harness fields carried over, every `[[<old>]]` and request owner repointed
- Delete: a file failing admission with no fact worth folding goes with its links and requests
- Route: knowledge one kind of task uses alone goes to that task's skill reference or a path-scoped rule, once the owner approves each file

Owner rulings move or merge only with their wording intact, a deletion of a stated fact goes to the owner first, and a decision memory holding files in place stops their routing. Existing stems change only in a pass the owner opens for renames with no other session running, as running sessions keep their old index.

## [03]-[SET]

Sets hold together when these stay true:
- Index within 160 lines and 20,000 characters, merges preferred over new files as it nears them
- Files within 200, the index line cap and the selector's manifest cap, and each within 4096 bytes
- Every fact at one home, every link resolving, every request row naming an existing owner
- Stems of one field sharing one facet set, and no new file named for a method of finding things
- Recall passing with no miss and no steal, and each co-load pair either one subject or linked subjects

## [04]-[REPORT]

Reports name per file its verdict (kept, edited, merged, split, renamed, moved, deleted) and the evidence for each, then the set totals from `[03]`, and the pass ends by tagging `evaluate-YYYY-MM-DD` in the folder. Choices only the owner holds (merging or deleting owner rulings, routing files out of memory, renaming a stem the owner chose) go through `AskUserQuestion` with a preview of each result before any write.
