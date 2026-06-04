---
description: Notation, status and invocation markers, table styling, and whitespace
---

# Formatting

This standard owns the presentation layer: the markers, alignment, whitespace, and heading idiom that visually render a container once form chooses its structure. It is the thin notation skin over form. It does not choose containers, decompose tables, write prose, or weigh evidence; it decides how a chosen structure looks and which markers carry status and emphasis. Keep this layer minimal: markers and styling earn their place by aiding an agent or reader, never by decorating the page.

## Use when

Apply this standard when rendering a container that form has already chosen:

- status, result, and change markers in records, checklists, tables, and reports;
- invocation markers (`IMPORTANT`, `CRITICAL`, `ALWAYS`, `NEVER`) in instruction files;
- table column alignment, header case, empty-cell notation, and the stub column;
- list marker characters and whitespace between structural elements;
- the heading-label idiom for a document, by document kind.

Container selection, table construction, and structured-record fields belong to the form standard; sentence mechanics, terminology, and `must`/`should`/`may` belong to the craft standard.

## Status and result markers

Render an inline status, result, or change as a bracketed uppercase token so an agent can filter on an exact string and a reader can scan one glyph. Use a closed set; do not invent tokens. A record's `Status:` field carries the plain lifecycle value the form standard defines (`DONE`, `BLOCKED`); the bracketed form is for inline use — a table cell, a report line, or a status callout.

- Result: `[PASS]`, `[FAIL]`, `[SKIP]`, `[PARTIAL]`, `[N/A]`.
- Change: `[ADDED]`, `[REMOVED]`, `[CHANGED]`, `[UNCHANGED]`.
- Lifecycle marker: the form standard's status value in brackets — `[DONE]`, `[IN-PROGRESS]`, `[BLOCKED]` — when a status appears inline rather than as a record field.

Reserve these tokens for status, result, and change reporting. Do not scatter bracketed tokens through ordinary prose, and do not use a marker where a definition-block field or a checkbox already carries the state.

## Invocation markers

Reserve invocation markers for instruction files — `AGENTS.md`, `CLAUDE.md`, and prompt files — where an agent must weight a constraint above surrounding text. Their compliance gain over plain imperative prose is small, so use them sparingly: at most a few per file, on genuine invariants. Prefer `CRITICAL` and `NEVER` for hard boundaries, `IMPORTANT` and `ALWAYS` for load-bearing defaults.

Do not bring invocation markers into the prose standards or ordinary documentation. There, the craft standard's `must`, `should`, and `may` carry requirement strength, and a wall of bracketed directives is the notation spam this corpus rejects. One concept never carries both an invocation marker and a prose modal.

## Table styling

Once form chooses a table, style it for scanning:

- Alignment: left-align text and identifier columns, right-align numeric and measurement columns, center status tokens, booleans, and single glyphs.
- Header case: title-case column headers in prose documents; instruction files may use bracketed uppercase rubrics to match their idiom.
- Empty cells: mark an absent or not-applicable value with an em-dash (`—`); never leave a cell blank, since a blank cell is ambiguous to an agent reading raw Markdown.
- Stub column: align the first column left, or, when it is a pure index, center it; the form standard sets what the stub column may contain.

The GFM separator row encodes the four alignment classes — left-align with `:---`, right-align with `---:`, center with `:---:`. A copy-safe skeleton:

```markdown copy-safe
| Item   | Count | Status | Active |
| :----- | ----: | :----: | :----: |
| Mesh   |    42 | [PASS] |   ✓    |
| Solver |     — | [SKIP] |   —    |
```

The stub (`Item`, text) is left-aligned, the numeric column (`Count`) is right-aligned, the status column carries a bracketed token centered, the boolean column is centered, and an absent numeric or boolean value is an em-dash.

## List and whitespace discipline

- Use `-` for bullets; do not use `*` or `+`.
- Keep no blank line between items of one list; a blank line ends the list.
- Put one blank line after a heading before its body, and one blank line on each side of a table, fenced block, or diagram.
- Let prose soft-wrap; the form standard owns the no-hard-wrap rule, and this whitespace discipline governs only the gaps between structural elements.

## Heading idiom

Two heading idioms coexist by document kind, and a document uses one throughout:

- Prose documents — standards, references, explanations, and ordinary docs — use sentence-style headings. This is the default; the form and craft standards own the sentence-style rule.
- Instruction files — `AGENTS.md` and peers — use the bracketed label idiom: `# [MANIFEST]`, `## [N][SECTION_LABEL]`, with uppercase compound labels joined by underscores. The bracket idiom suits files an agent loads as an operating overlay; it does not belong in prose documents.

Do not mix the two idioms in one file, and do not impose the bracket idiom on a prose standard to make it look machine-facing.

## Boundaries

- [information-structure.md](information-structure.md) owns container choice, table construction, and structured-record fields; this standard styles what it builds.
- [style-guide.md](style-guide.md) owns sentence mechanics, terminology, and requirement modals.
- [agentic-documentation.md](agentic-documentation.md) owns salience and where a marked constraint sits in a unit.
- [proof.md](proof.md) owns evidence strength and freshness; this standard renders the result and status markers that present an evidence table or status field.
- [README.md](README.md) owns document-type routing and cross-standard links.

## Review checklist

- [ ] Status, result, and change markers come from the closed token sets.
- [ ] Markers do not appear in ordinary prose or duplicate a field or checkbox state.
- [ ] Invocation markers appear only in instruction files, sparingly, on real invariants.
- [ ] Table columns are aligned by type, headers are cased to the document idiom, and empty cells use an em-dash.
- [ ] Bullets use `-`, list items carry no blank lines between them, and structural elements have one blank line around them.
- [ ] The document uses one heading idiom matching its kind.
