# [EDIT]

Edits improve one existing file: each change leaves it truer, sharper, or more complete for its subject and for the requests that open it, and any other change is junk.

## [01]-[CHANGES]

Each edit is one of these changes, and its form follows from it:

| [INDEX] | [CHANGE] | [TRIGGER]                                          | [FORM]                                                                 |
| :-----: | :------- | :------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | Extend   | New fact on the file's subject passes admission    | One bullet in its group, marked, placed by how often requests need it  |
|  [02]   | Update   | Version, path, state, or value moved               | Old value replaced in place with the new mark, no history line         |
|  [03]   | Correct  | Line proved wrong by a run, a source, or owner     | Line rewritten from the evidence, an owner statement only by the owner |
|  [04]   | Sharpen  | Rule restated, or its mistake recurring            | Lead rewritten as one rule, a `Not:` line on recurrence, never a twin  |
|  [05]   | Rescope  | Evidence widens or narrows where a fact holds      | Condition, `Holds until:`, or the rule's trigger rewritten             |
|  [06]   | Retire   | Fact failing admission now or no longer holding    | Line deleted with its links and the requests it alone answered         |
|  [07]   | Reshape  | Lead buried, parts the kind lacks, pronoun subject | Lines reordered and rewritten to the kind in `SKILL.md` `[03]`         |

## [02]-[VALUE]

Each line earns its place by changing what a later session does:

| [INDEX] | [LINE]     | [ADDS_VALUE_WHEN]                                      | [JUNK_WHEN]                                        |
| :-----: | :--------- | :----------------------------------------------------- | :------------------------------------------------- |
|  [01]   | Lead       | Acts alone, and the description compresses it          | Background, a topic label, or history first        |
|  [02]   | Why        | Names the incident, constraint, or mechanism           | Restates the rule, or cites the owner alone        |
|  [03]   | Fact       | Changes an action and passes admission today           | Derivable, a run's value, status, a sibling's fact |
|  [04]   | Identifier | Holds text a request contains (error, command, path)   | Paraphrase (`the config`, `that tool`)             |
|  [05]   | `Not:`     | Catches a mistake that recurred under the rule         | One-off incident, or a third example               |
|  [06]   | Dead end   | `<form> fails because <cause>` a session would retry   | Search narrative, or a failure with no cause       |
|  [07]   | Caution    | Names the cause it guards, so a reader can test it     | Its cause no longer holds                          |
|  [08]   | Link       | One clause naming what the linked file holds           | Body opening with routing, or siblings listed      |
|  [09]   | Mark       | Matches the claim, naming build or version where bound | Missing, overclaimed, or a build no longer present |

Owner-stated lines keep the owner's wording where it sets scope, and an edit leaves them unless a later owner statement changes them.

## [03]-[STEPS]

1. Read the file whole, its index line, its `.recall.jsonl` rows, every linker from `rg -l '\[\[<stem>\]\]' <memory-dir>`, and each source the change touches
2. Ask once before editing when `git -C <memory-dir> log -1 --format='%cr %(trailers:key=Session,valueonly)' -- <file>` shows another session within the hour
3. Name the change by `[01]`, run each new line through `SKILL.md` `[02]`, and judge each touched line by `[02]`
4. Rewrite in place with Edit, never Write, keeping the lead first, every fact marked by `SKILL.md` `[06]`, and harness and plugin metadata intact
5. Open `references/evaluate.md` when a fact belongs to another subject, the file serves disjoint request groups, or it passes 4096 bytes
6. Rewrite the description by `SKILL.md` `[05]` when the lead, scope, or gist changed, keeping requests the file still answers and adding one per new trigger
7. Run `scripts/recall.py <memory-dir>` after a description change, a miss or steal sending it back to step 6
