# [FORMATTING]

This standard owns the presentation layer: the markers, alignment, whitespace, and heading idiom that visually render a container once form chooses its structure. It is the thin notation skin over form. It does not choose containers, decompose tables, write prose, or weigh evidence; it decides how a chosen structure looks and which markers carry status and emphasis. Keep this layer minimal: markers and styling earn their place by aiding an agent or reader, never by decorating the page.

## [1][USE_WHEN]

Apply this standard when rendering a container that form has already chosen:

- status, result, and change markers in records, checklists, tables, and reports;
- invocation markers (`IMPORTANT`, `CRITICAL`, `ALWAYS`, `NEVER`) in instruction files;
- table column alignment, header case, empty-cell notation, escaping, and the stub column;
- list marker characters, continuation indentation, and whitespace between structural elements;
- the heading-label idiom, anchor stability, and hidden source-comment notation for a document by kind.

Container selection, table construction, and structured-record fields belong to the form standard; sentence mechanics, terminology, and `must`/`should`/`may` belong to the craft standard.

## [2][STATUS_RESULT_MARKERS]

Render an inline status, result, change, or compact state as a bracketed token so an agent can filter on an exact string and a reader can scan one glyph. Use a closed set; do not invent tokens, emojis, or decorative alternates. A record's `Status:` field carries the plain lifecycle value the form standard defines (`DONE`, `BLOCKED`); the bracketed form is for inline use — a table cell, a report line, or a status callout. Prefer the most specific family below and do not use two tokens that mean the same thing in one column.

- Result: `[PASS]`, `[FAIL]`, `[SKIP]`, `[PARTIAL]`, `[N/A]`. Use `[PASS]` and `[FAIL]` for gate outcomes; reserve `[OK]` and `[ERROR]` for runtime health or operational state, and do not mix them with pass/fail results in one table.
- Change: `[ADDED]`, `[REMOVED]`, `[CHANGED]`, `[UNCHANGED]`.
- Lifecycle marker: the form standard's exact default status value in brackets — `[PLANNED]`, `[IN-PROGRESS]`, `[BLOCKED]`, `[DONE]`, `[DROPPED]` — when a status appears inline rather than as a record field. Domain status vocabularies keep their declared casing as field values; bracketed inline lifecycle markers uppercase the canonical token and replace spaces with hyphens.
- Compact glyph: `[o]` pass, `[x]` fail, `[!]` attention, `[?]` unknown, `[+]` added, `[-]` removed, `[=]` unchanged, `[/]` skipped, `[~]` partial, `[$]` cached.
- Explicit state: `[OK]`, `[PASSED]`, `[FAILED]`, `[ERROR]`, `[WARNING]`, `[CAUTION]`, `[PENDING]`, `[UNKNOWN]`, `[NEW]`, `[DELETED]`, `[SAME]`, `[NULL]`, `[APPROX]`, `[CACHED]`, `[SAVED]`.

Use compact glyphs only where density matters, such as validation lists, delta summaries, or table cells. Use explicit states when clarity matters more than width. Reserve these tokens for status, result, change, and state reporting; do not scatter bracketed tokens through ordinary prose or use a marker where a definition-block field or a checkbox already carries the state. Use a checkbox when completion is asserted; use `[x]` only as a compact fail marker, never as a replacement for `- [x]`. Use `—` for an empty or absent table value, `n/a` when the domain's visible vocabulary requires a not-applicable term, `[N/A]` when a result itself is not applicable, `[SKIP]` when a gate intentionally did not run, `[UNKNOWN]` when a value should exist but is not known, and `[NULL]` only when a literal null value is the fact.

## [3][INVOCATION_MARKERS]

Reserve invocation markers for instruction files — `AGENTS.md`, `CLAUDE.md`, and prompt files — where an agent must weight a constraint above surrounding text. Their compliance gain over plain imperative prose is small, so use them sparingly: at most a few per file, on genuine invariants. Prefer `CRITICAL` and `NEVER` for hard boundaries, `IMPORTANT` and `ALWAYS` for load-bearing defaults.

Do not bring invocation markers into the prose standards or ordinary documentation. There, the craft standard's `must`, `should`, and `may` carry requirement strength, and a wall of bracketed directives is the notation spam this corpus rejects. One concept never carries both an invocation marker and a prose modal.

## [4][TABLE_STYLING]

Once form chooses a table, style it for scanning:

- Alignment: left-align text, identifiers, paths, commands, code spans, enum words, and prose phrases; right-align numeric, measurement, count, and date columns; center only `[INDEX]`, compact markers, booleans, tri-state values, and glyph-like cells.
- Centering ceiling: a centered non-index cell must be a compact marker or a value whose visible text is three letters or fewer, such as `yes`, `no`, `n/a`, `—`, `[o]`, or `[x]`. Do not center whole words such as `accepted`, `Supported`, `Standard`, `Contract`, or `Required`.
- Identifier exception: numeric-looking identifiers are text, not quantities. Left-align ADR numbers, version ranges, issue IDs, package names, endpoint paths, and code spans unless the column represents a count, measurement, date, duration, score, or other value that readers compare arithmetically or chronologically.
- Mixed columns: align by the column's semantic type, not by its shortest cell. If a column mixes compact tokens with longer words, code spans, or phrases, left-align it unless the whole column is numeric or date-like.
- Header case: every table column header is a bracketed uppercase rubric such as `[SOURCE_PATH]`, `[REQUIRED_GATE]`, or `[STATUS]`; compound rubrics use underscores and avoid spaces.
- Index column: every enumerable Markdown table starts with `[INDEX]`, center-aligned, with body rows numbered `[1]` through `[n]`. A non-enumerable matrix may use a short bracketed stub rubric instead only when row order or row identity is not enumerable.
- Empty cells: mark an absent or not-applicable value with an em-dash (`—`); never leave a cell blank, since a blank cell is ambiguous to an agent reading raw Markdown.
- Stub column: align the first column left, or, when it is a pure index, center it; the form standard sets what the stub column may contain.
- Literal pipes: escape a literal pipe inside a table cell as `\|`, including inside code spans, because unescaped pipes split GFM cells.
- Multiline cells: do not use `<br>`, embedded lists, or multiline HTML inside Markdown table cells. When one cell needs that structure, move the row to a definition block or subsection-per-record block.
- Visible notes: put reader-facing qualifications, generated-table caveats, and "replace with verified data" warnings in visible prose, a note block, or a footnote immediately after the table. Hidden comments are source-only hints, never the only carrier of safety or proof.

The GFM separator row encodes the four alignment classes — left-align with `:---`, right-align with `---:`, center with `:---:`. A template:

```markdown template
| [INDEX] | [ITEM] | [COUNT] | [STATUS] | [ACTIVE] |
| :-----: | :----- | ------: | :------- | :------: |
|   [1]   | Mesh   |      42 | [PASS]   |   [o]    |
|   [2]   | Solver |       — | [N/A]    |    —     |
```

The index column is centered, the text column (`[ITEM]`) is left-aligned, the numeric column (`[COUNT]`) is right-aligned, full result tokens in `[STATUS]` are left-aligned, the compact boolean/state column is centered, and an absent numeric or boolean value is an em-dash. A column containing full lifecycle words, long code spans, or prose-like status text is text and stays left-aligned.

## [5][LIST_WHITESPACE_DISCIPLINE]

- Use `-` for bullets; do not use `*` or `+`.
- Keep no blank line between items of one list; a blank line ends the list.
- Put one blank line after every H1, H2, and H3 heading before the next line, including when the next line is another heading, and one blank line on each side of a table, fenced block, or diagram.
- Indent child bullets, ordered-list continuations, and field continuations with four spaces. A bold group label that introduces a sub-list sits on its own line, followed immediately by the child list with no blank line.
- Keep short checklist fields inline after an em dash. If a checklist item needs a list, code block, or more than three trailing fields, promote it to a record block instead of inventing an indented checklist schema.
- Keep definition-block labels in sentence case or verified field casing, followed by one colon and one space. Indent wrapped or list-valued field content four spaces beneath the label; when two or more fields need continuations, promote the item to a subsection-per-record block.
- Let prose soft-wrap; the form standard owns the no-hard-wrap rule, and this whitespace discipline governs only the gaps between structural elements.

## [6][HEADING_IDIOM]

Use one bracketed heading format throughout documentation and instruction files:

- H1: `# [DOCUMENT_TITLE]`; the H1 carries only the semantic title label and never a heading-tier prefix.
- H2: `## [N][SECTION_LABEL]`; `N` is the section number in document order.
- H3: `### [N.M][SUBSECTION_LABEL]`; `N.M` is the parent section number plus the subsection number.

Labels are uppercase, use underscores for compounds, and ideally contain 1-2 semantic words. Use at most 3 words for normal headings; go longer only when a verified official name, command family, or document type would lose meaning if shortened. Do not mix bracketed heading labels with sentence-style headings. Examples inside fenced templates use the same bracketed grammar with placeholders only where the author must replace the label.

## [7][ANCHORS_COMMENTS]

Treat heading anchors and hidden Markdown comments as source-level notation:

- Anchor stability: a bracketed heading slug is the lowercased heading text with brackets, punctuation, dots, and underscores removed; `## [10][FOLDER_LAYOUT]` becomes `#10folderlayout`. When a heading number or label changes, update every in-repo link to that anchor in the same change.
- Duplicate headings: do not create duplicate bracket-heading anchors in one file. Rename the semantic label rather than relying on renderer-specific duplicate suffixes.
- Renumbering: renumber headings only when the document structure changes; cosmetic rewrites preserve existing heading numbers so links stay stable.
- Hidden comments: use HTML comments only for source-view authoring hints, generator hints, or maintenance notes that should not render: `<!-- source-only: <short reason> -->`.
- Comment placement: put a hidden comment immediately before the block it annotates, separated by the same blank-line rules as the surrounding block. Keep comments one line when possible; multi-line comments are allowed only for generated/source-only metadata that would be noisy when rendered.
- Comment limits: never use a hidden comment as the only carrier of safety, proof, intent, "replace with verified data," or required constraints. Do not put comments inside table rows or cells; use visible prose, a table note, or a definition block instead.

## [8][BOUNDARIES]

- [information-structure.md](information-structure.md) owns container choice, table construction, code-block intent labels, and structured-record fields; this standard styles what it builds.
- [style-guide.md](style-guide.md) owns sentence mechanics, terminology, and requirement modals.
- [agentic-documentation.md](agentic-documentation.md) owns salience and where a marked constraint sits in a unit.
- [proof.md](proof.md) owns evidence strength and freshness; this standard renders the result and status markers that present an evidence table or status field.
- [README.md](README.md) owns document-type routing and cross-standard links.

## [9][REVIEW_CHECKLIST]

- [ ] Status, result, and change markers come from the closed token sets.
- [ ] Markers do not appear in ordinary prose or duplicate a field or checkbox state.
- [ ] Invocation markers appear only in instruction files, sparingly, on real invariants.
- [ ] Table columns are aligned by type: text left, numeric/date values right, and only indexes plus compact markers or short booleans centered.
- [ ] Table cells escape literal pipes, stay single-line, and move long qualifications to visible notes, footnotes, or record blocks.
- [ ] Bullets use `-`, list items carry no blank lines between them, and structural elements have one blank line around them.
- [ ] The document uses the bracketed heading format consistently.
- [ ] Heading links match the rendered anchor slug, and hidden comments carry only source-view hints.
