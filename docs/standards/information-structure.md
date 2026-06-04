---
description: Cross-cutting standard for page shape, structures, diagrams, and chunks
---

# Information structure

This standard controls how documentation arranges information for scanning,
retrieval, and maintenance. Use it after choosing the document type and before
writing long sections.

## Use when

Use this standard to choose:

- headings and section boundaries;
- prose, bullets, numbered lists, definition-style blocks, or tables;
- ASCII trees, ASCII flows, Mermaid, or C4/Structurizr handoff;
- example placement and example labels;
- chunk shape for retrieval;
- metadata placement when a consumer needs page-level context.

Do not use it to decide evidence strength, sentence mechanics, or agent-facing
metadata fields. Use [proof.md](proof.md), [style-guide.md](style-guide.md),
and [agentic-documentation.md](agentic-documentation.md) for those concerns.

## Structure chooser

Use the smallest structure that preserves meaning:

- Prose: one concept, decision, caveat, or transition.
- Bullets: peer facts, requirements, unordered options, or checklist context.
- Numbered lists: ordered actions, ranked choices, lifecycle steps, or review
  gates.
- Definition-style blocks: terms, statuses, commands, roles, and short labeled
  facts.
- Tables: dense lookup, compatibility, support, status, or option comparison
  where columns stay narrow and stable.
- ASCII trees: ownership hierarchy, repository layout, artifact placement, or
  parent-child relationships.
- ASCII flows: short branching logic that fits on one screen.
- Mermaid: multi-node workflows, state transitions, sequences, or entity
  relationships that readers need rendered.
- Structurizr/C4: architecture models with multiple views, repeated elements,
  or durable relationship semantics.

Do not lean on one form because it is convenient. Change form when the reader's
question changes from explanation to lookup, ordered action, relationship, or
proof.

If a structure needs paragraph cells or list nesting beyond two levels, split
the content into subsections.

## Information types

Choose the section shape by the question it answers:

- Concept: what this is, why it exists, and what it is not.
- Task: how to complete one known outcome.
- Reference: what the exact facts, fields, options, statuses, or commands are.
- Process: what happens over time and which state changes matter.
- Principle: which rule controls decisions across cases.
- Structure: which parts exist, where boundaries sit, and how they relate.
- Fact: what is true now and what evidence refreshes it.

Do not blend types inside one section unless the section is explicitly a
chooser.

## Placement and emphasis

Use a front-and-close pattern at page, section, paragraph, and list level. Start
with the highest-value rule, scope, or outcome; end with the boundary, proof, or
next route that makes the unit safe to reuse.

Keep low-value source inventories, historical background, and implementation
detail out of the lead unless they change behavior. Put high-risk constraints
near the beginning of the smallest relevant unit, and repeat them only when the
later section changes how they apply.

Lists follow the same pattern. The first item should orient the set, middle
items should carry peers or exceptions, and the final item should close with the
most durable consequence, boundary, or review action when order is not already
fixed.

## Page anatomy

Use a predictable shape for standards:

1. Lead: scope and promise in one short paragraph.
2. Use case: when to apply the standard and when to route elsewhere.
3. Source or authority: only where source order changes behavior.
4. Required shape or rules.
5. Examples: only where misuse is likely.
6. Boundaries: adjacent owners and route-away cases.
7. Review checklist: observable author checks.

Every long standard needs a chooser, boundaries, and a checklist.

## Headings and chunks

Use headings as reader navigation and retrieval boundaries:

- Use one H1.
- Do not skip heading levels.
- Treat H2 sections as primary retrievable units.
- Use H3 sections only to refine one H2 concern.
- Avoid H4 and deeper headings unless a renderer, external standard, or
  generated reference format requires them.
- Use sentence-style headings unless a fixed template label or official name
  requires another form.
- Do not put links in headings.
- Put source of truth, scope, and highest-risk constraints near the beginning.

For retrieval, each H2 section should identify enough context to stand alone.
When a section could be a durable standard, generated mirror, task-instruction
template, or state artifact, state the artifact type where that distinction
changes how an agent should use it.

## Labeled prose

Use one label per line when a label carries meaning.

Use:

```markdown
Owner: Runtime maintainers
Review trigger: Host SDK version changes
```

Do not pack multiple labeled facts into one sentence when readers need to scan,
quote, or update them independently. Convert long labeled facts to bullets or
subsections.

## Tables

Use tables when row-and-column comparison is the point. Avoid using tables as a
layout device.

Good table subjects:

- support matrices;
- compatibility and version skew;
- option trade-offs;
- short status matrices;
- compact term lists;
- command or field lookup.

Avoid tables when:

- a row needs more than one sentence;
- a cell needs a list, paragraph, or code block;
- the table exceeds comfortable mobile or split-pane width;
- the content is a sequence of actions;
- the first column repeats the same long phrase.

If a table becomes wide, split it by profile, status, platform, or phase.

## Lists

- Use bullets for equivalent items.
- Use numbered lists only when order matters.
- Keep list items parallel in grammar and scope.
- Avoid single-item lists.
- Split lists longer than seven items into named groups when grouping improves
  scanning.
- Do not mix ordered and unordered items in one logical block.

## ASCII structures

Use ASCII when raw Markdown inspection matters more than rendered polish.

Copy-safe:

```text
docs/standards/
  README.md
  style-guide.md
  explanation/
    architecture.md
    adr.md
  reference/
    api.md
    support-matrix.md
```

Keep ASCII diagrams short, aligned, and labeled. Use Mermaid when the flow needs
multiple branches, states, or actors.

## Mermaid and C4

Use Mermaid when rendered structure adds value beyond bullets or ASCII:

- `flowchart`: branching workflow or data movement.
- `sequenceDiagram`: actor-to-actor interaction over time.
- `stateDiagram-v2`: lifecycle, statuses, or transitions.
- `erDiagram`: entities and relationships.
- `classDiagram`: type relationships when code reference is not enough.

Keep Mermaid diagrams small enough to review in source. Quote labels that
contain punctuation, parentheses, or reserved words. Add accessible titles and
descriptions when the renderer supports them.

C4, Structurizr, and architecture-specific diagram rules live in
[architecture.md](explanation/architecture.md). This standard decides whether a
diagram is needed; the architecture standard decides how architecture diagrams
are modeled.

## Examples

Use examples to show shape, not to pad the document:

- Include examples only when the rule is easy to misapply.
- Prefer one accepted shape and one rejected shape when the distinction matters.
- Put examples next to the rule they clarify.
- Keep example data realistic enough to preserve meaning.
- Mark placeholders and omitted sections explicitly.
- Label examples when they could be copied, executed, generated, or mistaken
  for current policy.

Do not publish interaction fragments, private paths, local task notes, or
machine-specific details as reusable documentation patterns.

## Metadata

Use metadata only when a renderer, indexer, generator, retrieval store, or
review workflow consumes it. Page structure may place metadata; field ownership
belongs to [agentic-documentation.md](agentic-documentation.md) and
[proof.md](proof.md).

Do not add metadata for speculative ranking, persuasion, or future tooling that
does not exist.

## Review checklist

- [ ] The document uses one primary structure per section.
- [ ] H2 sections are meaningful retrieval units.
- [ ] Source of truth, scope, and high-risk constraints appear early.
- [ ] Section and paragraph endings close with a useful boundary, proof, or
      next route.
- [ ] Tables are narrow, lookup-oriented, and free of paragraph cells.
- [ ] Ordered lists represent real order.
- [ ] ASCII trees or flows are short and readable in raw Markdown.
- [ ] Mermaid diagrams are used only when rendered structure adds value.
- [ ] C4/Structurizr semantics stay in architecture documents.
- [ ] Labeled facts are scan-friendly.
- [ ] Examples sit next to the rule they clarify.
- [ ] Metadata is present only when a consumer reads it.
