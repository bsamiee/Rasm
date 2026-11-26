# [EDIT]

Edits improve one existing memory, and its general memory when a new fact belongs there: each change leaves a file truer, sharper, or more complete for the requests that open it, and any other change is junk.

## [01]-[CHANGES]

Requests name the change, and an edit makes that change alone, so a sharpen request rewrites lines and adds none:

| [INDEX] | [CHANGE] | [TRIGGER]                                     | [FORM]                                                              |
| :-----: | :------- | :-------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Extend   | New fact on the subject passes admission      | One marked bullet in its group, ordered by request need             |
|  [02]   | Update   | Version, path, state, or value moved          | Value replaced in place with its new mark, no history line          |
|  [03]   | Correct  | Run, source, or owner proves a line wrong     | Line rewritten from that evidence, owner lines by `SKILL.md` `[06]` |
|  [04]   | Sharpen  | Rule restated, or its mistake recurring       | One rewritten lead, a `Not:` line on recurrence                     |
|  [05]   | Refine   | Lead buried, parts missing, pronoun subject   | Lines reordered and rewritten to their kind in `SKILL.md` `[03]`    |
|  [06]   | Rescope  | Evidence widens or narrows where a fact holds | Condition, trigger, or `Holds until:` rewritten                     |
|  [07]   | Retire   | Fact failing admission or no longer true      | Line deleted with its links and the rows it alone answered          |
|  [08]   | Relink   | Link bare, broken, or restating its target    | Clause rebuilt by `[04]`                                            |

## [02]-[PLACEMENT]

Each new fact goes to the narrowest file whose requests need it, by the layering in `SKILL.md` `[04]`:

| [INDEX] | [FACT]                                          | [HOME]                                                |
| :-----: | :---------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Holds for this file's subject alone             | This file                                             |
|  [02]   | Holds for every subject a general memory spans  | That general memory, written once                     |
|  [03]   | Departs from the general memory for one subject | That subject's file, naming the departure             |
|  [04]   | Holds for subjects no general memory spans      | `references/evaluate.md`, extracting a general memory |
|  [05]   | Belongs to another memory's subject             | That memory, by these steps                           |
|  [06]   | Belongs to no memory's subject                  | `references/create.md`                                |

General facts sit in one file and reach specific memories through their link clause alone:
- Specific memories gain a line from a general fact only where their subject departs from it or adds its own value
- Values of one shared concern differing by subject (a log folder per app) sit in each subject's specific memory, the general holding the `<placeholder>` rule
- Values a request reaches through one subject alone stay in that subject's file
- Specific memories stay silent on what their general memory holds: a line restating it goes, and one link clause names what the general memory holds
- General memories state facts in their field's terms, a `<placeholder>` where subjects differ and a trait where some share a fact, naming no subject

## [03]-[VALUE]

Each line earns its place by changing what a later session does:

| [INDEX] | [LINE]     | [ADDS_VALUE_WHEN]                                    | [JUNK_WHEN]                                            |
| :-----: | :--------- | :--------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | Lead       | Acts alone, and the description compresses it        | Background, a topic label, or history first            |
|  [02]   | Why        | Names the incident, constraint, or mechanism         | Restates the rule, or cites the owner alone            |
|  [03]   | Fact       | Changes an action and passes admission today         | Derivable, a run's value, status, or a general fact    |
|  [04]   | Identifier | Holds text a request contains (error, command, path) | Paraphrase (`the config`, `that tool`)                 |
|  [05]   | `Not:`     | Catches a mistake that recurred under the rule       | One-off incident, or a third example                   |
|  [06]   | Dead end   | `<form> fails because <cause>` a session would retry | Search narrative, or a failure with no cause           |
|  [07]   | Caution    | Names the cause it guards, so a reader can test it   | Its cause no longer holds                              |
|  [08]   | Link       | Clause naming what its target holds, beside its need | Bare `Related:` list, routing lead, or restated facts  |
|  [09]   | Mark       | Ends its claim, naming build or version where bound  | Missing, mid-sentence, overclaimed, or a removed build |

Extends trim their group first, as stale, repeated, and derivable lines go before a new line joins. Changes make a file worse whatever each line's value, and each takes its right form:
- Second phrasing appended beside a rule -> rule rewritten once
- General rule added beside the narrow rules it explains -> narrow rules deleted in the same edit, as keeping both leaves a log with a summary on top
- One fact copied into specific memories -> general memory holds it once, specifics keep their link clause
- New line contradicting an existing one -> evidence decides which line stays
- Third example under a rule -> new example replaces the weaker one
- Replaced value kept as history -> value replaced, git holds the old one
- File grown past its subject or 4096 bytes -> `references/evaluate.md` splits or extracts it
- Change resting on one ambiguous event -> owner statement, run, or source first

## [04]-[LINKS]

Link clauses (`SKILL.md` `[04]`) sit beside the line whose fact depends on their target, and an edit repairs each one in a file it touches:
- Bare `Related:` list -> each stem a line depends on takes a clause at that line, and the rest go
- Stem missing from the folder -> clause repointed to the file holding that content now, found by `rg` over its identifiers, else deleted
- Clause naming content its target no longer holds -> clause rewritten to the current holder
- Clause restating its target's facts -> clause cut to the words naming what its target holds
- Specific memory lacking a clause to its general memory -> one clause naming what the general memory holds
- Retired or rescoped content another file's clause names -> that clause rewritten in the same edit

## [05]-[STEPS]

1. Read the file whole, its index line, every linker from `rg -l '\[\[<stem>\]\]' <memory-dir>`, its general memory, and each source the change touches
2. Ask once before editing when `git -C <memory-dir> log -1 --format='%cr %(trailers:key=Session,valueonly)' -- <file>` shows a session other than `$CLAUDE_CODE_SESSION_ID` within the hour
3. Name the change by `[01]`, place each new fact by `[02]`, and move to the reference its row names when its home is neither this file nor its general memory
4. Run each new line through `SKILL.md` `[02]`, judge each touched line and the change as a whole by `[03]`, and repair link clauses by `[04]`
5. Rewrite in place with Edit, never Write, so harness and plugin metadata survive, keeping the lead first and every fact marked by `SKILL.md` `[06]`
6. Open `references/evaluate.md` when the file serves disjoint request groups, passes 4096 bytes, or holds a fact its siblings repeat
7. Rewrite each touched description by `SKILL.md` `[05]` when its lead, scope, or gist changed, or a rule or profile body outgrew its index line
8. Read 3 requests the changed body answers against its index line alone, rewriting the line by step 7 until each request lands on this file
9. Fix any refusal the plugin reports back before the work ends
