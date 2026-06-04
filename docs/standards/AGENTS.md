# [STANDARDS_ROUTER]

This file routes changes inside `docs/standards/**` to the active standards owner and the minimum validation that proves the edit.

## [1][SCOPE]

These instructions govern `docs/standards/**` only. They extend `CLAUDE.md` and the root `AGENTS.md`; they do not replace them. Keep this file a compact router for standards work, not a second style guide.

For Codex, closer files override earlier project guidance, and resolution loads nested project instruction files from the repository root toward the current directory.

Source of truth: [OpenAI Codex `AGENTS.md` guide](https://developers.openai.com/codex/guides/agents-md)
Last verified: 2026-06-04
Review trigger: OpenAI Codex project-instruction guidance changes.

## [2][READ_ORDER]

1. Read `CLAUDE.md`, root `AGENTS.md`, and `docs/usage.md`.
2. Read `docs/standards/README.md`; it is the index, reader-need map, and router.
3. If the change touches shared rules, read the five shared standards: `agentic-documentation.md`, `information-structure.md`, `style-guide.md`, `proof.md`, and `formatting.md`.
4. If the change involves a document type, read exactly one matching type standard under `explanation/`, `reference/`, `task/`, or `learning/`.
5. Read the target standards file fully before rewriting it.

`docs/standards/_TMP/**` is inactive source material. Use it only when the user explicitly asks to integrate it.

## [3][ROUTING]

- Reader need, document type, placement, lifecycle, and split/link: the index in `README.md`.
- Salience and ordering within a unit, artifact separation, provider behavior, instruction files, indexes, generated mirrors, retrieval, metadata, MCP catalogs, and structured outputs: `agentic-documentation.md`.
- Container choice, tables, structured records, lists, diagrams, code blocks, line wrapping, and chunks: `information-structure.md`.
- Prose, terminology, punctuation, links, examples, code-safe Markdown, and accessibility: `style-guide.md`.
- Evidence, freshness, source conflicts, verification, agent-surface evaluation, and preservation under refactor: `proof.md`.
- Status and invocation markers, table styling, whitespace, and the heading idiom: `formatting.md`.

## [4][REWRITE_RULES]

- [ALWAYS] Preserve every load-bearing fact when restructuring; a dropped command, version, invariant, routing pointer, or field is a regression, not a simplification.
- Pick one primary reader need before editing.
- Apply standards implicitly in the rewritten prose; do not announce the technique inside the published document.
- Prefer restructuring, deletion, and owner links over parallel rules.
- Keep one concept under one owner; link adjacent standards instead of copying their bodies.
- Put source of truth, scope, and high-risk constraints early, then close each major section with the boundary, proof, or next action that keeps the rule usable.
- Do not bury high-impact constraints, exceptions, or route-away rules mid-paragraph or mid-list.
- Use the container the form standard prescribes: prose for one relationship, bullets for peers, numbered lists for order, status-tagged records for trackable sets, tables for lookup, diagrams for relationships prose cannot show.
- Delete stale commands, obsolete tool names, compatibility aliases, placeholders, and duplicated external-standard summaries.

## [5][STRUCTURE_DISCIPLINE]

Use the front-and-close pattern at page and section level: the first sentence states the rule, outcome, or scope; the middle qualifies or proves it; the last closes with the boundary, evidence requirement, or route to the owner. Each H2 is a retrievable unit a reader understands from its first paragraph. When a section mixes concept, task, reference, process, and proof, split it or make the chooser explicit. Use examples only where misuse is likely, beside the rule, marked for reuse risk.

## [6][SENTENCE_DISCIPLINE]

- Preserve official names, commands, paths, flags, and symbols exactly.
- Use present tense for durable standards, future for planned work, past only for historical evidence.
- Prefer active voice, concrete verbs, positive form, and one controlling idea per paragraph.
- Keep parallel lists parallel in grammar and scope.
- State a gating condition before the action it controls; a rule may trail a minor exception.
- Soft-wrap prose; protect copyable code, commands, and placeholders with ASCII-safe Markdown.

## [7][EXCLUSIONS]

Do not publish live task instructions, conversation fragments, reasoning summaries, authoring notes, temporary local rationale, unverified provider behavior, private machine paths, secrets, or fixed multi-agent workflow counts in standards docs.

[NEVER] Claim a Markdown linter, link checker, docs build, renderer, CI gate, or provider behavior exists unless current repository tooling or current primary documentation proves it.

## [8][VALIDATION]

For Markdown-only standards edits, run `git diff --check` on the changed Markdown at minimum. Run configured Markdown, link, docs-build, diagram, or render checks only when the changed claim requires that proof. When no configured checker exists for changed links, run a local path and anchor validation or state the proof gap honestly.

Before finishing, search the active standards for stale owner names, forbidden process language, trailing whitespace, and old filenames. Validate that the changed file follows its own lead, section order, form choices, proof rules, and review checklist, and that no load-bearing fact was dropped.
