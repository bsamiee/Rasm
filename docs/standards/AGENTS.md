# [STANDARDS_AGENT_ROUTER]

This file governs edits inside `docs/standards/**`. Keep it a compact behavioral overlay for agent-only standards work: read the active corpus, route each rule to its owner, preserve load-bearing facts, and never turn this file into a second standards library.

## [1][SCOPE]

These instructions extend `CLAUDE.md` and the root `AGENTS.md`; they do not replace them. They apply only to the standards authoring corpus under `docs/standards/**`.

The active corpus is the Markdown set discovered by `fd -H . docs/standards -t f -e md`. Treat prompt notes, session history, deprecated source material, external research, and sub-agent critiques as inputs only when the task explicitly names them; promote only durable rules into the owning standard.

## [2][READ_ORDER]

[ALWAYS]:
- For any standards edit, read `CLAUDE.md`, root `AGENTS.md`, and `docs/standards/README.md` before changing files.
- For edits to `AGENTS.md`, `README.md`, any shared standard, cross-type routing, boundary language, provider behavior, or `agents-md.md`, read every active standards file before editing.
- For a narrow type-standard edit, use `README.md` to classify the reader need, read the shared owner files needed by the change, read every affected type standard, and then read the full target file.
- Read `docs/usage.md` only when the edit changes cross-stack owner precedence, source-truth order, implementation proof, host-library routing, or command/tooling claims outside this standards corpus.

## [3][RULE_OWNERS]

Use the owner that controls the changed rule; do not copy that owner's body into this file.

| [INDEX] | [OWNER]                    | [CONTROLS]                                                        |
| :-----: | :------------------------- | :---------------------------------------------------------------- |
|   [1]   | `README.md`                | reader need, type choice, placement, split/link, lifecycle        |
|   [2]   | `agentic-documentation.md` | agent salience, instruction files, artifact separation, providers |
|   [3]   | `agents-md.md`             | `AGENTS.md` semantic slots, profiles, route-away, anti-fragility  |
|   [4]   | `information-structure.md` | containers, records, tables, diagrams, checklists, cardinality    |
|   [5]   | `style-guide.md`           | prose, sentence mechanics, terminology, links, code-safe Markdown |
|   [6]   | `proof.md`                 | evidence, preservation, proof gaps, docs-as-code gate selection   |
|   [7]   | `formatting.md`            | bracketed headings, invocation markers, table styling, whitespace |
|   [8]   | type standards             | artifact-specific structure, status vocabulary, local proof slots |

## [4][ARTIFACT_CONTRACT]

Produced standards are executable guidance for future agents. Put scope, reader action, controlling source or status, highest-risk constraint, and route-away at the opening edge; close with boundaries, proof, or the next safe route.

Every type standard must define agent use, required produced structure, section cardinality, adjacent checks, maintenance triggers, and stale-source events before examples, taxonomies, or background. Required, conditional, optional, and repeatable sections must be distinguishable before an agent can copy the shape.

Use adjacent-document relation records only when the adjacent fact changes reader action, proof, status interpretation, validation, or maintenance. Ordinary background links stay ordinary, and missing adjacent content routes away instead of being embedded.

## [5][EDIT_INVARIANTS]

[ALWAYS]:
- Preserve every command, path, version, flag, field, qualifier, status value, route pointer, proof field, invariant, and source-truth claim.
- Pick one primary reader need before rewriting; split or route mixed concept, task, reference, process, proof, and status bodies.
- Use `information-structure.md` as law for section form: prose for one idea, lists for peer or ordered facts, records for independently maintained state, tables only for real comparison or lookup, and diagrams only when rendered topology or flow adds meaning.
- Use `style-guide.md` and `agentic-documentation.md` as law for prose: front-and-close paragraphs, condition-before-action instructions, direct present-tense wording, exact source names, and no prompt/session/process narration.
- Use `agents-md.md` as law for `AGENTS.md` semantic slots, route-away decisions, anti-fragility, trust boundaries, root profile, and corpus rebuild rules.
- Omit absent fields instead of filling them with `none`, `n/a`, placeholders, or filler records.

## [6][FORBIDDEN_PATTERNS]

[NEVER]:
- Add people/process metadata such as `owner`, `role`, `team`, `maintainer`, `reviewer`, `stakeholder`, `audience`, `business`, `enterprise`, `PM`, `RACI`, approval ladder, or organizational accountability unless a literal local tool output or source standard consumes that exact field.
- Publish live task instructions, chat excerpts, critique summaries, prompt-source narration, rewrite rationale, fixed sub-agent counts, session state, secrets, or nonpublic machine paths.
- Claim a linter, link checker, docs build, renderer, CI gate, provider behavior, static rail, test rail, or bridge rail exists or passed unless current repository tooling, current primary documentation, or current command output proves it.
- Preserve stale commands, removed tool names, legacy aliases, empty conditional headings, decorative diagrams, duplicated table/diagram bodies, unsupported progress markers, `Consumed by: none`, or generic metadata whitelists.
- Hand-maintain generated catalogs, mirrors, or provider claims as independent truth.

## [7][CLOSE_CHECK]

Before finishing a standards edit, verify:
- [ ] Required read scope was satisfied.
- [ ] Each changed rule routes to its owner instead of a duplicate body.
- [ ] Load-bearing facts, qualifiers, proof fields, and route pointers survived.
- [ ] Links, anchors, diagrams, and provider/tooling claims are proved or gapped when those surfaces changed.
- [ ] No forbidden people/process metadata, session commentary, placeholder fields, stale commands, decorative markers, or invented tooling claims remain.
