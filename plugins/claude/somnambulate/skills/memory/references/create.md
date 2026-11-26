# [CREATE]

New memory files start from a moment worth keeping, pass admission line by line, and reach later sessions through their description.

## [01]-[MOMENTS]

Moments that produce a memory, owner steering first:
- Correction: the owner rejects an approach or output, with the reason that decides the next edge case
- Ruling: the owner sets scope, a tool, or a direction, with the condition that releases it
- Finding: an investigation overturns an assumption, a source, or planned work, beyond this session
- Location: an outside resource (dashboard, ticket, runbook page, account) the work keeps reaching for

## [02]-[STEPS]

1. Draft each candidate as one line and run it through `SKILL.md` `[02]`, stopping when no line passes
2. Search `rg -il '<term>' <memory-dir>` for the subject, its synonyms and abbreviations, tool names, and error text, reading each hit whole
3. Move the work to `references/edit.md` when an existing file holds the subject
4. Pick the kind and type by `[03]` and the stem by `[04]`, checking no sibling stem sits nearer the subject
5. Write the lead first, then the parts the kind needs, each fact marked by `[06]`, owner wording kept where it sets scope
6. Write 3 should-load requests and 3 near misses with their owners by `[05]`, then the description from them
7. Clone the folder with `git clone -q <memory-dir> <scratch>/memory`, add the new file and its rows there, run `scripts/recall.py <scratch>/memory`, and rewrite until no miss or steal
8. Preview body, description, and requests through `AskUserQuestion` with a free-response option
9. In a session without `AskUserQuestion`, write on the owner's request and give body, description, and requests in the reply
10. Write the accepted file with Write, append its rows to `<memory-dir>/.recall.jsonl`, and repoint sibling near-miss rows the new file now owns
11. Fix any refusal the plugin reports back before the work ends

## [03]-[FIRST_WRITE]

Frontmatter holds `name` equal to the stem, a one-line `description`, and `metadata.type`, and the harness adds `node_type`, `originSessionId`, and `modified` on its first write and can requote values.
