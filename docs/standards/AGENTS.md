# [STANDARDS_ROUTER]

This file routes changes inside `docs/standards/**` to the active standards owner and the minimum validation that proves the edit.

## [1][SCOPE]

These instructions govern `docs/standards/**` only. They extend `CLAUDE.md` and the root `AGENTS.md`; they do not replace them. Keep this file a compact router for standards work, not a second style guide.

For Codex, closer files override earlier project guidance, and resolution loads nested project instruction files from the repository root toward the current directory.


## [2][READ_ORDER]

1. Read `CLAUDE.md`, root `AGENTS.md`, and `docs/usage.md`.
2. Read `docs/standards/README.md`; it is the index, reader-need map, and router.
3. If the change touches shared rules, read the five shared standards: `agentic-documentation.md`, `information-structure.md`, `style-guide.md`, `proof.md`, and `formatting.md`.
4. If the change involves a document type, read exactly one matching type standard under `explanation/`, `reference/`, `task/`, or `learning/`.
5. Read the target standards file fully before rewriting it.

## [3][ROUTING]

- Reader need, document type, placement, lifecycle, and split/link: the index in `README.md`.
- Salience and ordering within a unit, artifact separation, provider behavior, instruction files, indexes, generated mirrors, retrieval, MCP catalogs, and structured outputs: `agentic-documentation.md`.
- Container choice, tables, structured records, lists, diagrams, code blocks, line wrapping, and chunks: `information-structure.md`.
- Prose, terminology, punctuation, links, examples, code-safe Markdown, and accessibility: `style-guide.md`.
- Evidence, freshness, source conflicts, verification, agent-surface evaluation, and preservation under refactor: `proof.md`.
- Status and invocation markers, table styling, whitespace, and the heading idiom: `formatting.md`.

## [4][REWRITE_RULES]

- Preserve every load-bearing fact when restructuring; a dropped command, version, invariant, routing pointer, or field is a regression.
- Pick one primary reader need before editing.
- Apply standards implicitly in the rewritten prose.
- Link adjacent standards instead of copying their bodies.
- Put controlling source, scope, and high-risk constraints early.
- Close each major section with the boundary, proof, or next action that keeps the rule usable.
- Split a section that mixes concept, task, reference, process, and proof, or make the chooser explicit.
- Delete stale commands, removed tool names, legacy aliases, placeholders, and duplicated external-standard summaries.
- Never add document-level YAML frontmatter metadata blocks to standards files; keep document facts in the rendered body and route claim evidence through `proof.md`.

## [5][STYLE_DISCIPLINE]

Preserve official names, commands, paths, flags, symbols, fields, status vocabularies, and qualifiers exactly. Use present tense for durable standards, future for planned work, and past only for historical evidence. Keep high-impact constraints, exceptions, and route-away rules at the edge of the unit that depends on them.

## [6][EXCLUSIONS]

Do not publish live task instructions, chat excerpts, rationale summaries, draft notes, session-local rationale, unverified provider behavior, nonpublic machine paths, secrets, or fixed multi-agent workflow counts in standards docs.

[NEVER] Claim a Markdown linter, link checker, docs build, renderer, CI gate, or provider behavior exists unless current repository tooling or current primary documentation proves it.

## [7][VALIDATION]

For Markdown-only standards edits, run `git diff --check` on the changed Markdown at minimum. Run configured Markdown, link, docs-build, diagram, or render checks only when the changed claim requires that proof. When no configured checker exists for changed links, run a local path and anchor validation or state the proof gap honestly.

Before finishing, search the active standards for stale owner names, forbidden process language, trailing whitespace, and renamed files. Validate that the changed file follows its own lead, section order, form choices, proof rules, and review checklist, and that no load-bearing fact was dropped.
