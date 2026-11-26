# [CREATE]

New memory files start from a moment worth keeping, take their place among general and specific memories by `SKILL.md` `[04]`, and reach the owner as a preview before any write.

## [01]-[MOMENTS]

Moments that produce a memory, owner steering first:
- Correction: the owner rejects an approach or output, with the reason that decides the next edge case
- Decision: the owner sets scope, a tool, or a direction, with the condition that ends it
- Finding: an investigation overturns an assumption, a source, or planned work, beyond this session
- Location: an outside resource (dashboard, ticket, runbook page, account) the work keeps reaching for

Each moment yields candidate lines in the owner's wording where it sets scope, and the tests of `SKILL.md` `[02]` decide which survive.

## [02]-[PLACEMENT]

Each surviving line goes to the narrowest home that holds it for every subject it is true of, and only a line no memory holds starts a file:

| [INDEX] | [LINE_HOLDS_FOR]                                     | [HOME]                                                            |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | One subject an existing memory covers                | That memory, by `references/edit.md`                              |
|  [02]   | Every subject under an existing general memory       | That general memory, by `references/edit.md`, specifics untouched |
|  [03]   | 2 or more subjects, each memory stating it already   | Extraction into a general memory, by `references/evaluate.md`     |
|  [04]   | 2 or more subjects of one field, stated in no memory | New general memory, written before any specific memory it serves  |
|  [05]   | One subject no memory covers                         | New specific memory, linking its general memory where one exists  |

Lines from one moment often split across rows, and each row runs its own work:
- General memories serve 2 or more subjects, existing or new in this pass, else their lines stay in the one specific memory
- Lines sharing one concern build its general memory first, then each specific memory on it
- Existing specific memories take a new general memory's link clause only where their own lines rely on it
- Line test: a line true of every sibling belongs in the general memory, and a line naming one subject's identifier stays specific

## [03]-[STEPS]

1. Draft each candidate as one line and run it through `SKILL.md` `[02]`, stopping when no line passes
2. Search `rg -il '<term>' <memory-dir>` over the subject, synonyms, abbreviations, tool names, and error text, reading each hit whole
3. List the field's stems with `fd '^<field>-' <memory-dir>`, reading its general memory whole
4. Place each line by `[02]`, sending rows `[01]` to `[03]` to their reference before any new file
5. Pick each new file's kind and type by `SKILL.md` `[03]` and its stem by `SKILL.md` `[04]`, as a general or specific name of its field
6. Write a new general memory first: a lead naming the concern, then the lines true of every subject under it, each marked by `SKILL.md` `[06]`
7. Write each specific memory with its own subject's lines alone, none of its general memory's, and the link clause `SKILL.md` `[04]` places
8. Write 3 should-load requests per new file by `SKILL.md` `[05]`, then the description from them
9. Apply `SKILL.md` `[04]` merge and split to the requests: all needing a sibling's body fold the file into it, no shared situation splits it
10. Clone with `git clone -q <memory-dir> <scratch>/memory`, write the files and link clauses there, and run `scripts/index.py <scratch>/memory`
11. Read each request against the generated index alone, as a new session sees it: a request no line sends to its file rewrites that description
12. Preview each new file's body, description, and requests, and each added link clause, through `AskUserQuestion` with a free-response option
13. In a session without `AskUserQuestion`, write only on the owner's own request with the preview in the reply, else reply with the preview alone
14. Return to step 10 when the owner changes a description or moves a line, so the check covers what the owner approves
15. Write the accepted general memory, then its specific memories, with Write, and add the link clauses to existing files with Edit
16. Delete the scratch clone
17. Fix each refusal the plugin reports after a write before the work ends

Requests shape each layer's description:
- Rule files: should-load requests ask what the index line cannot settle, by `SKILL.md` `[05]` items 7 and 8
- General memories: requests arrive in one subject's words, so each specific's link clause names the question the general answers
- Specific memories: requests ask what the subject's own facts answer

## [04]-[ANTI_PATTERNS]

Created files go wrong in these forms:

| [INDEX] | [WRONG]                                                          | [RIGHT]                                                    |
| :-----: | :--------------------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | Specific memory restating a general line to read alone           | Link clause naming what the general memory holds           |
|  [02]   | Shared line kept in the first specific memory written            | General memory written first, the line in it               |
|  [03]   | General memory listing each subject's identifiers                | Identifiers in the specific memory of their subject        |
|  [04]   | General memory over one subject                                  | Lines in that subject's specific memory until a second one |
|  [05]   | One file per line of one subject                                 | One specific memory holding every line of its subject      |
|  [06]   | File of unrelated lines under a method name (`notes`, `tooling`) | Each line in the memory of its subject                     |
|  [07]   | Description reworded so a shared line's requests open one file   | Shared line moved into its general memory                  |

## [05]-[FIRST_WRITE]

Frontmatter holds `name` equal to the stem, a one-line `description`, and `metadata.type`, and the harness adds `node_type`, `originSessionId`, and `modified` on its first write and can requote values.
