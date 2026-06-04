---
description: Container selection, structured records, tables, diagrams, and page shape
---

# Information structure

This standard owns form: which container carries a piece of information, and how that container is structured for scanning, retrieval, and maintenance. Choose the container after the document type is known and before writing long sections. It does not decide where high-value content sits inside a unit, what the words say, how a container is visually styled, or how strong the evidence is.

## Use when

Apply this standard to choose and shape containers:

- prose, bullets, numbered lists, checklists, definition blocks, status-tagged records, tables, or code blocks;
- decision tables, lookup tables, ASCII trees, Mermaid diagrams, or a C4 architecture handoff;
- callouts, collapsible blocks, and footnotes;
- code-block intent labels and example placement;
- headings, section boundaries, line wrapping, and retrieval chunk shape;
- page anatomy for a standard or reference page.

Salience and ordering within a unit belong to the position standard, sentence mechanics to the craft standard, evidence strength to the proof standard, and visual styling (alignment, markers, whitespace) to the formatting standard.

## Container chooser

Use the smallest container that preserves meaning, and change container when the reader's question changes from explanation to lookup, ordered action, relationship, or proof. Structured containers are not decoration: bullets and key-value blocks outperform prose for option selection and field extraction, and tables outperform both for dense factual lookup, so reach for structure whenever the content is a set of peers, records, or comparisons.

- Prose: one concept, decision, caveat, or transition where a sentence is genuinely clearer than a list.
- Bullets: peer facts, requirements, unordered options.
- Numbered lists: ordered actions, ranked choices, lifecycle steps, or gates.
- Checklists (`- [ ]`): verification, acceptance, or status items whose completion is asserted and checked.
- Definition blocks: terms, statuses, commands, roles, and short labeled facts, one `label: value` per line.
- Status-tagged records: any finite enumerable set whose items carry status over time — milestones, decisions, requirements, risks, tasks.
- Tables: dense row-and-column comparison or lookup across a homogeneous set.
- Decision tables: an action or rule determined by a finite combination of conditions.
- Code blocks: commands, literal files, config, schemas, or copyable snippets.
- ASCII trees and flows: hierarchy or short branching where raw-Markdown inspection matters more than rendered polish.
- Mermaid: multi-node workflows, sequences, state, or relationships that readers need rendered.
- Callouts, collapsible blocks, footnotes: constraint interrupts, low-salience reference, and inline provenance.

A single record read by field belongs in a definition block, not a one-row table. Sparse data compared across rows still belongs in a table, not flattened into prose. When no two items share a comparison question, abandon the table and give each item its own record.

## Tables

Use a table when row-and-column comparison or lookup across a homogeneous set is the point, and keep it within bounds that current models and split-pane readers handle. A table degrades past roughly 15 columns or 20 rows; degradation is continuous, so treat these as the line past which decomposition is mandatory, not optional. Tables are the most token-efficient structured format up to moderate size, but an oversized table suffers the same long-context degradation as any other oversized unit.

Decompose by the dominant violation, never both at once:

- Row count over 20, columns 4 or fewer: split by a natural row axis — status, phase, platform, owner — into sibling tables each under the ceiling, and lead each sibling with one sentence naming the axis value it covers.
- Column count over 15, rows 4 or fewer: pivot — transpose so subjects become rows — then apply the row-split rule if the transposed table still exceeds 20 rows.
- Rows that are heterogeneous records rather than comparisons: abandon the single table for the summarize-then-detail form below.

When you pivot, name the output form in a one-sentence lead before it. The permitted pivot outputs are a transposed table (subjects to rows when attributes are many and subjects few), a profile-split into sibling tables (one per categorical value of the dominant column), and a key-value expansion (each former row becomes a definition block when no comparison question spans rows).

When a topic needs more rows than the ceiling across all axis values combined, use the summarize-then-detail form: a summary table of at most 5 columns with one row per axis value, where one column carries the heading anchor of the detail section, followed immediately by the detail tables or record sections. The summary table is the retrieval entry point; the detail sections are the lookup surface.

Avoid a table entirely when the content is a sequence of actions, when the first column repeats the same long phrase, or when a single record is read by field rather than compared across rows.

## Table content discipline

A cell holds one atomic fact: a single value, a short phrase, a status token, or a Markdown inline such as a code span or link. Keep cells to about 8 words. A column whose cells average more than 8 words is a prose column; a table may carry at most one prose column and it must be the last column. When two or more columns would be prose columns, the content is not a comparison — convert it to definition blocks or labeled subsections.

When a cell would need a constraint, exception, or version qualifier longer than the cell limit, place a short token in the cell and carry the qualification in a footnote or a notes block immediately after the table. The stub column — the first column — must be a short, unique, scannable key: an identifier, command, proper noun, or status token, not a sentence.

## Tables and prose

A table and its surrounding prose each own a distinct role; neither restates the other. Pair them deliberately:

- Promote prose to a table when a paragraph compares three or more items across two or more attributes. Comparison prose that an agent must parse into an implicit table should have been a table.
- Frame a table with one or two sentences immediately before it when the reader cannot act on the table alone — when a status value is contextual, when which row applies is not obvious, or when an invariant governs the whole set. The framing carries what the table cannot; it never reads the cells back in sentences.
- Do not follow a complete table with prose that restates its cells. Follow-on prose may state a consequence or exception, nothing already in the grid.
- In a mixed block, assign each container one role and keep them disjoint: prose owns the decision criteria or invariant, the table owns the per-item values.

## Structured records

A finite enumerable set whose items carry state — milestones, decisions, requirements, risks, tasks, gates — must be rendered as structured records, never as flat prose. This is the form that prevents a roadmap from becoming paragraphs with no status, no dependency, and no exit proof. Each item is a record carrying machine-readable fields; the choice between a record table and a per-item record block follows the table rules above (a table while items stay homogeneous and short-celled, a per-item block once any field needs more than a cell).

Use this closed `Status` vocabulary so an agent can filter on exact strings: `PLANNED`, `IN-PROGRESS`, `BLOCKED`, `DONE`, `DROPPED`. A type standard may define a domain-specific status set in place of this default — a roadmap names lifecycle stages, a decision log names decision states — and may extend or rename the recurring fields for its domain — a roadmap carries `Exit criteria` and `Proof surface` — provided each status set stays closed, each field stays one `label: value` per line, and both are defined before first use. The recurring record fields carry fixed meanings:

- `Status`: the current lifecycle state, from the closed set above.
- `Exit`: the single observable, falsifiable condition that moves the item to `DONE` — a shipped artifact, a merged path, or a passing gate.
- `Depends`: the item titles or anchors whose `Status` must be `DONE` first; omit when there is no prerequisite.
- `Owner`: the role accountable for the item; include when more than one owner exists across the set.
- `Proof`: the artifact path, command output, or link that substantiates completion; required where `Exit` is not self-evident.

A per-item record block names the item, then carries its fields one `label: value` per line:

```markdown conceptual
### Single-file AST tier
Status: PLANNED
Exit: every AST rule ships passing and failing fixtures; the suite is green.
Depends: Design corpus
Proof: tests/ fixture run output.
```

Escalate from a record table to per-item record blocks when any item has more than 5 fields, when any field needs a list or code block, or when items are updated independently over the document's life.

## Checklists

A checklist is the form for items whose completion is asserted and verified. Use a checkbox list (`- [ ]` / `- [x]`), not plain bullets, for acceptance gates, release readiness, onboarding steps, and author self-checks. Three forms differ by what each item carries:

- Verification checklist: an author self-check of observable, falsifiable conditions; item text only, no owner or proof. The review checklist closing each standard is this form.
- Acceptance checklist: an external gate; each item carries an `Owner` and an `Exit` condition, with `Proof` populated on completion.
- Status checklist: a living tracker; each item carries a `Status` and, where they exist, `Owner` and `Depends`.

The fields trail the item text after an em dash, so a checkbox item carrying them stays a single line and never widens into a record block:

```markdown conceptual
- [ ] Migration applied to production — Owner: Platform; Exit: schema_version = 14; Proof: <link>
- [ ] AST tier fixtures green — Status: IN-PROGRESS; Owner: Runtime; Depends: #design-corpus
```

The first line is an acceptance item (`Owner` + `Exit`, `Proof` on completion); the second is a status item (`Status` plus `Owner` and `Depends`). A verification item carries item text alone.

Use a checklist rather than prose whenever a document asserts that gates, steps, or criteria are complete; prose cannot encode completion state, and a plain bullet cannot be checked.

## Lists

- Use bullets for equivalent items and numbered lists only when order is real.
- Keep items parallel in grammar and scope, and avoid single-item lists.
- Limit nesting to two levels; split deeper structure into subsections.
- Split a list past seven items into named groups, each group introduced by a bold inline label on its own line followed by its sub-list.
- Do not mix ordered and unordered items in one logical block.

## Definition blocks

Use one label per line when a label carries meaning a reader will scan, quote, or update independently:

```markdown conceptual
Owner: Runtime maintainers
Review trigger: Host SDK version changes
```

When several records share one schema, use a grouped definition block: a plain group-name line, then the shared `label: value` fields indented beneath it, with a blank line between groups. Move to a subsection-per-record block — an H3 heading as the record identifier and a definition block as its body — once a record exceeds 5 fields or any field needs a list or code block. Do not pack several labeled facts into one sentence, and do not widen a record into a one-row table.

## Decision and lookup tables

Two table forms answer a different question than a comparison table:

- Decision table: rows are condition combinations, left columns are the inputs and right columns the resulting action or rule. Use it when two or more independent conditions jointly determine an outcome over a finite, enumerable combination space. Prefer prose for a single condition with one outcome, and a flowchart when the conditions are sequential rather than combinatorial.
- Lookup table: a flat mapping from a discrete key to a value, behavior, or next state, optimized for O(1) retrieval rather than cross-row comparison. Use it for command-to-effect, code-to-meaning, or status-to-policy maps.

A decision table carries one column per input condition on the left and the resolved action on the right, one row per combination in the enumerated space:

| Authenticated | Quota remaining | Action       |
| ------------- | --------------- | ------------ |
| no            | —               | 401 reject   |
| yes           | no              | 429 throttle |
| yes           | yes             | serve        |

A lookup table is a single key column mapping to its value, read by key rather than compared across rows:

| Status token  | Retry policy        |
| ------------- | ------------------- |
| `TRANSIENT`   | backoff, 3 attempts |
| `PERMANENT`   | fail fast, no retry |
| `RATE-LIMITED`| honor `Retry-After` |

## Code blocks

Fence every command, literal file, config, schema, or copyable snippet, and mark its intent — in the info string after the language, or as a leading comment where the language has one — so a reader knows whether the block is safe to run, study, or avoid. Use one intent label per block:

- `copy-safe`: run or paste as written. For a config or data block, use this when the block is byte-equivalent to a named source-of-truth file, and name that file in the label (`copy-safe — config.yml`).
- `conceptual`: an illustrative or proposed shape, not a verbatim or runnable artifact.
- `test-only`: valid only in a test or fixture context.
- `generated`: produced by a generator; edit the source, not the block.
- `output-only`: sample output, not an input to run.
- `deprecated`: retained for recognition; do not adopt.
- `rejected`: a counter-example shown to prevent misuse.

Keep blocks short enough to review. Summarize long machine output and link the generated contract, manifest, or source file instead of pasting it whole.

## ASCII structures

Use ASCII when raw-Markdown inspection matters more than a rendered image: file trees, repository layout, artifact placement, small stacks or matrices, and tiny flows embedded in code comments where no render step exists.

```text conceptual
project/
  README.md
  guide.md
  reference/
    api.md
    options.md
```

Keep ASCII diagrams short, aligned, and labeled. When a flow needs multiple branches, actors, or states, it has outgrown ASCII; move it to Mermaid.

## Mermaid and C4

Use Mermaid when rendered structure adds value beyond bullets or ASCII. Mermaid source is compact and well understood by current models, so prefer it over embedded images for any diagram an agent may need to read or revise. Map the shape to the diagram type:

- `flowchart`: branching workflow or data movement.
- `sequenceDiagram`: actor-to-actor interaction over time.
- `stateDiagram-v2`: lifecycle, statuses, or transitions.
- `erDiagram`: entities and relationships.
- `classDiagram`: type relationships when names alone are insufficient.
- `quadrantChart`, `sankey`, `architecture`, and C4 views: comparative positioning, flow volume, or system structure when a simpler type loses meaning.

Keep diagrams small enough to review in source. Quote labels containing punctuation, parentheses, or reserved words, and add accessible titles and descriptions when the renderer supports them. For architecture, a C4 Context and Container pair is the baseline; deeper Component, dynamic, or deployment views are added only where internal structure or runtime behavior is the subject. Choosing whether a diagram is needed is this standard's call; how an architecture model is structured belongs to the architecture type standard.

## Callouts, collapsible, and footnotes

Three forms separate special-purpose content from the reading path. Each carries a portability caveat, so use it only where the corpus renderer supports it:

- Callouts (`> [!NOTE]`, `> [!WARNING]`, `> [!IMPORTANT]`, `> [!CAUTION]`): a single constraint, safety boundary, or non-obvious invariant that must interrupt the reader. GitHub-flavored; one callout per concern, never as decoration. The invocation-marker vocabulary is the formatting standard's concern.
- Collapsible blocks (`<details>` / `<summary>`): low-salience material referenced but off the primary path — full stack traces, exhaustive option dumps, long sample output. The summary line states what is inside.
- Footnotes (`[^label]`): provenance attached inline to a specific claim — a version, a behavioral source, a table-cell qualification — without breaking the sentence. The evidence the footnote carries is the proof standard's concern.

## Line wrapping

Do not hard-wrap Markdown prose. Write each paragraph as one logical line and let the renderer and editor soft-wrap it. A fixed-width manual wrap inflates line count, creates reflow churn on every edit, and inserts newline tokens mid-sentence for no agent benefit; the repository sets no Markdown line-length limit, so the wrap is pure noise. Insert a manual line break only where it is structural — between list items, table rows, and definition-block fields, inside fenced blocks, or at a deliberate hard break.

## Markdown economy

Carry meaning with structure, not ornament. Frontier models are robust to Markdown, so the optimization is to spend tokens on signal: meaningful headings, lists, records, and tables, not nested decoration. Avoid stacked emphasis and redundant rules, and let one structure own each section. Decorative markup consumes context budget without improving comprehension.

## Page anatomy

Render the page shape as a copy-safe heading skeleton, not a narrated list of section names — a skeleton is itself a structure prescription, and an author copies it rather than reconstructing it from prose:

```markdown conceptual
# <Title>

<Lead: scope and promise in one short paragraph.>

## Use when
## Source or authority
## <Required shape or rules>
## Examples
## Boundaries
## Review checklist
```

Section cardinality:

- `Lead`, `Use when`, the rules section(s), `Boundaries`, `Review checklist` — required.
- `Source or authority` — conditional; include only where source order changes behavior.
- `Examples` — conditional; include only where misuse is likely.

Every long standard needs a chooser, boundaries, and a checklist. A type standard additionally carries a required-structure section: a copy-safe heading skeleton plus a section-cardinality block, so an author cannot omit a mandatory section. Show that artifact, do not name it — give the type standard the same fenced skeleton plus cardinality tagging that this page uses, so the omission becomes mechanically impossible:

```markdown conceptual
# <Scope> <type>

<Lead.>

## <Section A>
## <Section B>
## <Conditional section C>
## Boundaries
## Review checklist
```

Tag each heading `required | conditional | optional | repeatable` in a cardinality block beneath the skeleton — `required` sections always appear, `conditional` sections appear only when their condition holds, `optional` sections appear at author discretion, and `repeatable` records appear once per item.

## Headings and chunks

Treat headings as navigation and retrieval boundaries:

- Use one H1 and do not skip heading levels.
- Treat H2 sections as primary retrievable units that stand alone.
- Use H3 only to refine one H2 concern; avoid H4 and deeper unless a renderer or generated format requires them.
- Use sentence-style headings unless a fixed template label or official name requires another form, and do not put links in headings.

Each H2 should identify enough context to be read out of order. When a section could be reused as a generated mirror, task template, or state artifact, state that artifact type where the distinction changes how an agent uses it.

## Metadata placement

Place metadata only when a renderer, indexer, generator, retrieval store, or review workflow consumes it. This standard decides where page-level metadata sits; which fields exist and what they prove belongs to the position and evidence standards. Do not add metadata for speculative ranking or for tooling that does not exist.

## Examples

Use examples to show shape, not to pad:

- Include an example only when the rule is easy to misapply.
- Put the example beside the rule it clarifies and keep its data realistic.
- Mark placeholders and omitted sections explicitly, and label any block a reader could copy, run, or mistake for current policy with its intent.

Do not publish interaction fragments, private paths, or local task notes as reusable patterns.

## Boundaries

- [agentic-documentation.md](agentic-documentation.md) owns salience and the placement of content within the containers this standard shapes, plus metadata field ownership.
- [formatting.md](formatting.md) owns the visual styling of these containers — table alignment, status and invocation markers, whitespace, and the heading-label idiom.
- [style-guide.md](style-guide.md) owns the words inside every container.
- [proof.md](proof.md) owns evidence strength and freshness for the facts a table, record, diagram, or block presents.
- [README.md](README.md) owns document-type routing and links to type standards such as the architecture standard.

## Review checklist

- [ ] The page follows the prescribed anatomy: lead, use when, rules, examples, boundaries, checklist.
- [ ] One primary container owns each section, and mixed blocks assign disjoint roles.
- [ ] Tables stay within column and row bounds, hold no paragraph cells beyond one trailing prose column, and decompose by the dominant violation when over.
- [ ] A finite enumerable set of trackable items uses status-tagged records with `Status`, `Exit`, and `Depends`, never flat prose.
- [ ] Checklists use the checkbox form and carry the fields their checklist form requires.
- [ ] A single record uses a definition block; record clusters use grouped or subsection-per-record blocks.
- [ ] Decision and lookup tables are used for condition-action and key-value content respectively.
- [ ] Lists nest no deeper than two levels and group past seven items with bold labels.
- [ ] Code blocks carry an intent label.
- [ ] ASCII is short and raw-Markdown readable; Mermaid is used only when rendering adds value.
- [ ] Callouts, collapsible blocks, and footnotes are used for their purpose and the renderer supports them.
- [ ] Prose is not hard-wrapped; manual breaks are structural only.
- [ ] Headings form standalone retrievable H2 units and carry no links.
- [ ] Examples sit beside the rule they clarify; metadata is present only where a consumer reads it.
