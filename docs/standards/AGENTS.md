# Documentation standards instructions

## Scope

These instructions apply only to `docs/standards/**`. They extend `CLAUDE.md`
and the root `AGENTS.md`; they do not replace them. Keep this file as a compact
router for standards work, not a second style guide.

For Codex, nested project instruction files are loaded from the repository root
toward the current directory, and closer files override earlier project
guidance.

Source of truth: [OpenAI Codex `AGENTS.md` guide](https://developers.openai.com/codex/guides/agents-md)
Last verified: 2026-06-04
Review trigger: OpenAI Codex project-instruction guidance changes.

## Read order

1. Read `CLAUDE.md`, root `AGENTS.md`, and `docs/usage.md`.
2. Read `docs/standards/README.md`.
3. Read `documentation-system.md`, `style-guide.md`,
   `information-structure.md`, `proof.md`, and
   `agentic-documentation.md` when the change touches shared rules.
4. Read exactly one matching type standard under `explanation/`, `reference/`,
   `task/`, or `learning/` when a document type is involved.
5. Read the target standards file fully before rewriting it.

`docs/standards/_TMP/**` is inactive source material. Use it only when the user
explicitly asks to integrate inactive material.

## Routing

- Route document type, placement, lifecycle, and split/link questions to
  `documentation-system.md`.
- Route prose, terminology, punctuation, links, examples, code-safe Markdown,
  and accessibility to `style-guide.md`.
- Route headings, lists, tables, diagrams, labeled blocks, examples, and chunks
  to `information-structure.md`.
- Route evidence, freshness, source conflicts, and verification claims to
  `proof.md`.
- Route instruction files, indexes, generated mirrors, retrieval, metadata,
  MCP catalogs, state artifacts, and structured outputs to
  `agentic-documentation.md`.

## Rewrite rules

- Pick one primary reader need before editing.
- Apply standards implicitly in the rewritten prose; do not announce the
  technique inside the published document.
- Prefer restructuring, deletion, and owner links over parallel rules.
- Keep one concept under one owner; link adjacent standards instead of copying
  their bodies.
- Put source of truth, scope, and high-risk constraints early, then close each
  major section with the boundary, proof, or next action that keeps the rule
  usable.
- Do not bury high-impact constraints, exceptions, or route-away rules in the
  middle of a paragraph or list.
- Use dense prose for one relationship, bullets for peer facts, numbered lists
  for order, tables for lookup, and diagrams for relationships that prose
  cannot make clear.
- Convert same-line labeled facts into short paragraphs, bullets, or
  definition-style blocks when that improves scanning.
- Delete stale commands, obsolete tool names, compatibility aliases,
  placeholders, and duplicated external-standard summaries.

## Structure discipline

Use the front-and-close pattern at both page and section level. The first
sentence states the rule, outcome, or scope. The middle qualifies, limits, or
proves it. The final sentence closes with the boundary, evidence requirement,
or route to the owning standard.

Each H2 should be a retrievable unit. A reader or agent should know what the
section controls from its first paragraph without reading the surrounding page.
When a section mixes concept, task, reference, process, and proof, split it or
make the chooser explicit.

Use examples only where misuse is likely. Place the example beside the rule it
clarifies, and mark reuse risk before examples that could be copied, executed,
generated, or mistaken for policy.

## Sentence discipline

- Use present tense for durable standards, future tense for planned work, and
  past tense only for historical evidence.
- Prefer active voice, concrete verbs, positive form, and one controlling idea
  per paragraph.
- Keep parallel lists parallel in grammar and scope.
- State a condition before the action it controls: `If <signal>, do <action>`.
- Preserve official names, commands, paths, flags, and symbols exactly.
- Protect copyable code, commands, config, and placeholders with ASCII-safe
  Markdown.

## Exclusions

Do not publish live task instructions, conversation fragments, reasoning
summaries, authoring notes, temporary local rationale, unverified provider
behavior, private machine paths, secrets, or fixed multi-agent workflow counts in
standards docs.

Do not claim a Markdown linter, link checker, docs build, renderer, CI gate, or
provider behavior exists unless current repository tooling or current primary
documentation proves it.

## Validation

For Markdown-only standards edits, run `git diff --check -- docs/standards` at
minimum. Run configured Markdown, link, docs-build, diagram, or render checks
only when the changed claim requires that proof. State unrun gates honestly.

Before finishing, search the active standards for stale owner names, forbidden
process language, trailing whitespace, and old filenames. Validate that the
changed file follows its own lead, section order, form choices, proof rules,
and review checklist.
